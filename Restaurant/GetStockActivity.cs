using Core;
using Core.Entities;
using Core.Services.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Willezone.Azure.WebJobs.Extensions.DependencyInjection;

namespace Restaurant
{
    public static class GetStockActivity
    {
        [FunctionName(Constants.GetStockActivityFunctionName)]
        public static async Task<IList<StockIngredient>> Run(
            [ActivityTrigger]string trigger,
            [Inject]IBaseRepositoryFactory<StockIngredient> stockRepositoryFactory,
            ILogger log)
        {
            IList<StockIngredient> result = null;

            var cosmosDbEndpoint = Environment.GetEnvironmentVariable(Constants.CosmosDbEndpointKeyName);
            var cosmosDbKey = Environment.GetEnvironmentVariable(Constants.CosmosDbKeyKeyName);
            var repo = stockRepositoryFactory.GetInstance(cosmosDbEndpoint, cosmosDbKey, Constants.StockCollectionName);
            result = await repo.GetAllAsync();

            return result;
        }
    }
}