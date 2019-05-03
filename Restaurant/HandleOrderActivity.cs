using Core;
using Core.Entities;
using Core.Services.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Willezone.Azure.WebJobs.Extensions.DependencyInjection;

namespace Restaurant
{
    public static class HandleOrderActivity
    {
        [FunctionName(Constants.HandleOrderActivityFunctionName)]
        public static async Task<bool> Run(
            [ActivityTrigger]OrderDTO order,
            [Inject]IBaseRepositoryFactory<Order> ordersRepositoryFactory,
            ILogger log)
        {

            if (order != null && !string.IsNullOrWhiteSpace(order.OrchestratorId) && !string.IsNullOrWhiteSpace(order.OrderId))
            {
                var cosmosDbEndpoint = Environment.GetEnvironmentVariable(Constants.CosmosDbEndpointKeyName);
                var cosmosDbKey = Environment.GetEnvironmentVariable(Constants.CosmosDbKeyKeyName);
                var repo = ordersRepositoryFactory.GetInstance(cosmosDbEndpoint, cosmosDbKey, Constants.OrdersCollectionName);

                var orderEntity = await repo.GetAsync(order.OrderId);
                if (orderEntity != null &&
                    (
                        string.IsNullOrWhiteSpace(orderEntity.OrchestaratorId) ||
                        orderEntity.OrchestaratorId.Equals(order.OrchestratorId, StringComparison.OrdinalIgnoreCase)
                    ) &&
                    orderEntity.Status != order.OrderStatus
                )
                {
                    orderEntity.OrchestaratorId = order.OrchestratorId;
                    orderEntity.Status = order.OrderStatus;
                    await repo.UpsertAsync(orderEntity);
                    return true;
                }
            }

            return false;
        }
    }
}