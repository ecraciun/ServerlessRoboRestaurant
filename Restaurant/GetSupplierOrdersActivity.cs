using Core;
using Core.Entities;
using Core.Services.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Willezone.Azure.WebJobs.Extensions.DependencyInjection;

namespace Restaurant
{
    public static class GetSupplierOrdersActivity
    {
        [FunctionName(Constants.GetSupplierOrdersActivityFunctionName)]
        public static async Task<IList<SupplierOrder>> Run(
            [ActivityTrigger]List<string> supplierOrderIds,
            [Inject]IBaseRepositoryFactory<SupplierOrder> supplierOrdersRepositoryFactory,
            ILogger log)
        {
            IList<SupplierOrder> result = null;

            if (supplierOrderIds?.Any() ?? false)
            {
                var cosmosDbEndpoint = Environment.GetEnvironmentVariable(Constants.CosmosDbEndpointKeyName);
                var cosmosDbKey = Environment.GetEnvironmentVariable(Constants.CosmosDbKeyKeyName);
                var repo = supplierOrdersRepositoryFactory.GetInstance(cosmosDbEndpoint, cosmosDbKey, Constants.SupplierOrdersCollectionName);
                result = await repo.GetWhereAsync(x => supplierOrderIds.Contains(x.Id));
            }

            return result;
        }
    }
}