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
    public static class UpdateSupplierOrderActivity
    {
        [FunctionName(Constants.UpdateSupplierOrderActivityFunctionName)]
        public static async Task<bool> Run(
            [ActivityTrigger](string SupplierOrderId, SupplierOrderStatus NewStatus) updateDetails,
            [Inject]IBaseRepositoryFactory<SupplierOrder> supplierOrdersRepositoryFactory,
            ILogger log)
        {

            if (!string.IsNullOrEmpty(updateDetails.SupplierOrderId))
            {
                var cosmosDbEndpoint = Environment.GetEnvironmentVariable(Constants.CosmosDbEndpointKeyName);
                var cosmosDbKey = Environment.GetEnvironmentVariable(Constants.CosmosDbKeyKeyName);
                var repo = supplierOrdersRepositoryFactory.GetInstance(cosmosDbEndpoint, cosmosDbKey, Constants.SupplierOrdersCollectionName);

                var supplierOrderEntity = await repo.GetAsync(updateDetails.SupplierOrderId);
                if (supplierOrderEntity != null &&
                    supplierOrderEntity.Status != updateDetails.NewStatus)
                {
                    return await repo.TryUpdateWithRetry(supplierOrderEntity, (o) =>
                    {
                        o.LastModified = DateTime.UtcNow;
                        o.Status = updateDetails.NewStatus;
                    });
                }
            }

            return false;
        }
    }
}