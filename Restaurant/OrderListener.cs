using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core;
using Core.Entities;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
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
            [OrchestrationClient] DurableOrchestrationClient starter,
            ILogger log)
        {
            foreach(var document in inputDocuments)
            {
                var order = JsonConvert.DeserializeObject<Order>(document.ToString());
                if( order != null && 
                    order.LastModifiedUtc == order.TimePlacedUtc && 
                    order.Status == OrderStatus.New)
                {
                    var instanceId = await starter.StartNewAsync(Constants.OrderOrchestratorFunctionName, order);

                    // TODO: maybe log something about order and instance id?
                }
            }
        }
    }
}