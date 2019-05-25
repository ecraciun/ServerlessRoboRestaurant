using Core;
using Core.Entities;
using Core.Services.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using Willezone.Azure.WebJobs.Extensions.DependencyInjection;

namespace Restaurant
{
    public static class CreateSupplierOrderAndWaitActivity
    {
        [FunctionName(Constants.CreateSupplierOrderAndWaitActivityFunctionName)]
        public static async Task<bool> Run(
            [ActivityTrigger]SupplierOrder supplierOrder,
            [Inject]IBaseRepositoryFactory<SupplierOrder> supplierOrdersRepositoryFactory,
            ILogger log)
        {
            if (supplierOrder != null &&
                !string.IsNullOrEmpty(supplierOrder.SupplierId) &&
                (supplierOrder.OrderedItems?.Any() ?? false))
            {
                var cosmosDbEndpoint = Environment.GetEnvironmentVariable(Constants.CosmosDbEndpointKeyName);
                var cosmosDbKey = Environment.GetEnvironmentVariable(Constants.CosmosDbKeyKeyName);
                var repo = supplierOrdersRepositoryFactory.GetInstance(cosmosDbEndpoint, cosmosDbKey, Constants.SupplierOrdersCollectionName);

                supplierOrder.CreatedAt = DateTime.UtcNow;
                supplierOrder.LastModified = supplierOrder.CreatedAt;
                supplierOrder.Status = SupplierOrderStatus.Processing;

                var orderId = await repo.AddAsync(supplierOrder);
                
                await Task.Delay(TimeSpan.FromSeconds(supplierOrder.DeliveryETAInSeconds));

                supplierOrder = await repo.GetAsync(orderId);

                return await repo.TryUpdateWithRetry(supplierOrder, (order) =>
                {
                    order.Status = SupplierOrderStatus.PickedUp;
                });
            }

            return false;
        }
    }
}