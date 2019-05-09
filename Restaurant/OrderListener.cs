using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core;
using Core.Entities;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Restaurant
{
    public static class OrderListener
    {
        [FunctionName(Constants.OrderListenerFunctionName)]
        public static async Task Run(
            [CosmosDBTrigger(
                databaseName: Constants.DatabaseName,
                collectionName: Constants.OrdersCollectionName,
                ConnectionStringSetting = Constants.CosmosDbConnectionStringKeyName,
                LeaseCollectionName = "leases",
                CreateLeaseCollectionIfNotExists = true)]IReadOnlyList<Document> inputDocuments,
            [OrchestrationClient] DurableOrchestrationClientBase starter,
            ILogger log)
        {
            await EnsureInventoryCheckerIsRunning(starter, Constants.InventoryCheckerOrchestratorId, Constants.InventoryCheckerEternalOrchestratorFunctionName);
            await EnsureInventoryCheckerIsRunning(starter, Constants.SupplierOrderReceiverOrchestratorId, Constants.SupplierOrderReceiverOrchestratorFunctionName);

            foreach (var document in inputDocuments)
            {
                var order = JsonConvert.DeserializeObject<Order>(document.ToString()); // or (Order)(dynamic)document;
                if( order != null && 
                    order.LastModifiedUtc == order.TimePlacedUtc && 
                    order.Status == OrderStatus.New &&
                    (order.OrderItems?.Any() ?? false))
                {
                    var instanceId = await starter.StartNewAsync(Constants.OrderOrchestratorFunctionName, order);

                    log.LogInformation($"Order {order.Id} was picked up by {instanceId}");
                }
            }
        }

        
        private static async Task EnsureInventoryCheckerIsRunning(DurableOrchestrationClientBase starter, string singletonOrchestratorId, 
            string singletonOrchestratorFunctionName)
        {
            // Check if an instance with the specified ID already exists.
            var existingInstance = await starter.GetStatusAsync(singletonOrchestratorId);
            if (existingInstance == null)
            {
                // An instance with the specified ID doesn't exist, create one.
                await starter.StartNewAsync(singletonOrchestratorFunctionName, singletonOrchestratorId, null);
            }
            else
            {
                if( existingInstance.RuntimeStatus == OrchestrationRuntimeStatus.Canceled ||
                    existingInstance.RuntimeStatus == OrchestrationRuntimeStatus.Failed ||
                    existingInstance.RuntimeStatus == OrchestrationRuntimeStatus.Terminated)
                {
                    await starter.PurgeInstanceHistoryAsync(singletonOrchestratorId);
                    await starter.StartNewAsync(singletonOrchestratorFunctionName, singletonOrchestratorId, null);
                }
            }
        }
    }
}