using Core;
using Core.Entities;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Restaurant
{
    public static class SupplierOrderMonitorOrchestrator
    {
        [FunctionName(Constants.SupplierOrderMonitorOrchestratorFunctionName)]
        public static async Task RunOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext context,
            [OrchestrationClient]DurableOrchestrationClientBase client)
        {
            var orderIds = context.GetInput<List<string>>();
            if (orderIds?.Any() ?? false)
            {
                var timeoutMoment = context.CurrentUtcDateTime.AddSeconds(Constants.DefaultOrchestratorTimeoutInSeconds);

                while (context.CurrentUtcDateTime < timeoutMoment)
                {
                    var supplierOrders = await context.CallActivityAsync<IList<SupplierOrder>>(Constants.GetSupplierOrdersActivityFunctionName,
                        orderIds);

                    if (!supplierOrders.Any()) break;

                    var ordersToRemove = supplierOrders
                        .Where(
                            x =>
                                x.Status == SupplierOrderStatus.Delivered ||
                                x.Status == SupplierOrderStatus.Refused ||
                                (
                                    (x.Status == SupplierOrderStatus.Created || x.Status == SupplierOrderStatus.Processing) &&
                                    x.LastModified.AddSeconds(x.DeliveryETAInSeconds) > context.CurrentUtcDateTime
                                )
                            ).ToList();
                    var rejectedOrders = ordersToRemove.Where(x => x.Status == SupplierOrderStatus.Refused);
                    var completedOrders = ordersToRemove.Where(x => x.Status != SupplierOrderStatus.Refused);
                    var stillPendingOrders = supplierOrders.Where(x => !ordersToRemove.Select(o => o.Id).Contains(x.Id));

                    await MarkSupplierOrdersAsDelivered(completedOrders, context);
                    await SendEventsForCompletedOrders(completedOrders, client);

                    if (!stillPendingOrders.Any()) break;

                    var nextCheck = context.CurrentUtcDateTime.AddSeconds(Constants.DefaultSupplierOrderCheckSleepInSeconds);
                    await context.CreateTimer(nextCheck, CancellationToken.None);
                }
            }
        }

        private static async Task SendEventsForCompletedOrders(IEnumerable<SupplierOrder> completedOrders, DurableOrchestrationClientBase client)
        {
            foreach (var order in completedOrders)
            {
                await client.RaiseEventAsync(Constants.SupplierOrderReceiverOrchestratorId, Constants.SupplierOrderReceivedEventName, order);
            }
        }

        private static async Task MarkSupplierOrdersAsDelivered(IEnumerable<SupplierOrder> completedOrders, DurableOrchestrationContext context)
        {
            var updateTasks = new List<Task<bool>>();
            foreach (var order in completedOrders)
            {
                updateTasks.Add(context.CallActivityAsync<bool>(
                    Constants.UpdateSupplierOrderActivityFunctionName, (order.Id, SupplierOrderStatus.Delivered)));
            }
            await Task.WhenAll(updateTasks);
        }

#if DEBUG
        [FunctionName("SupplierOrderMonitorOrchestrator_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")]HttpRequestMessage req,
            [OrchestrationClient]DurableOrchestrationClient starter,
            ILogger log)
        {
            string requestBody = await req.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(requestBody))
            {
                var orders = JsonConvert.DeserializeObject<List<string>>(requestBody);
                // Function input comes from the request content.
                string instanceId = await starter.StartNewAsync(Constants.SupplierOrderMonitorOrchestratorFunctionName, orders);

                return starter.CreateCheckStatusResponse(req, instanceId);
            }

            return new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.BadRequest
            };
        }
#endif
    }
}