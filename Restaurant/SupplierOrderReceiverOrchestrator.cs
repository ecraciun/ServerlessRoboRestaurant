using Core;
using Core.Entities;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Restaurant
{
    public static class SupplierOrderReceiverOrchestrator
    {
        [FunctionName("SupplierOrderReceiverOrchestrator")]
        public static async Task RunOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext context)
        {
            var order = await context.WaitForExternalEvent<SupplierOrder>(Constants.SupplierOrderReceivedEventName);

            if (order != null && order.OrderedItems.Any())
            {
                await UpdateStock(order.OrderedItems, context);
                await context.CallActivityAsync<bool>(Constants.UpdateSupplierOrderActivityFunctionName, (order.Id, SupplierOrderStatus.PickedUp));
            }

            context.ContinueAsNew(null);
        }

        private static async Task UpdateStock(List<SupplierOrderIngredientItem> orderedItems, DurableOrchestrationContext context)
        {
            var inventory = await context.CallActivityAsync<IList<StockIngredient>>(Constants.GetStockActivityFunctionName, null);
            var updateTasks = new List<Task<bool>>();

            foreach (var item in orderedItems)
            {
                var id = inventory.FirstOrDefault(x => x.Name.Equals(item.Name, StringComparison.OrdinalIgnoreCase));
                updateTasks.Add(context.CallActivityAsync<bool>(Constants.UpdateStockActivityFunctionName, (id, item.Quantity)));
            }

            await Task.WhenAll(updateTasks);
        }

#if DEBUG
        [FunctionName("SupplierOrderReceiverOrchestrator_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")]HttpRequestMessage req,
            [OrchestrationClient]DurableOrchestrationClient starter,
            ILogger log)
        {
            // Check if an instance with the specified ID already exists.
            var existingInstance = await starter.GetStatusAsync(Constants.SupplierOrderReceiverOrchestratorId);
            if (existingInstance == null || existingInstance.RuntimeStatus == OrchestrationRuntimeStatus.Failed)
            {
                if (existingInstance != null)
                {
                    await starter.PurgeInstanceHistoryAsync(Constants.SupplierOrderReceiverOrchestratorFunctionName);
                    //await starter.TerminateAsync(Constants.InventoryCheckerEternalOrchestratorFunctionName, "Cleanup failed run");
                }
                // An instance with the specified ID doesn't exist, create one.
                await starter.StartNewAsync(Constants.SupplierOrderReceiverOrchestratorFunctionName,
                    Constants.SupplierOrderReceiverOrchestratorId, null);
                return starter.CreateCheckStatusResponse(req, Constants.SupplierOrderReceiverOrchestratorId);
            }
            else
            {
                await starter.TerminateAsync(Constants.SupplierOrderReceiverOrchestratorFunctionName, "Force start new");
                await starter.PurgeInstanceHistoryAsync(Constants.SupplierOrderReceiverOrchestratorFunctionName);

                // An instance with the specified ID exists, don't create one.
                return req.CreateErrorResponse(
                    HttpStatusCode.Conflict,
                    $"An instance with ID '{Constants.SupplierOrderReceiverOrchestratorId}' already exists.");
            }
        }
#endif
    }
}