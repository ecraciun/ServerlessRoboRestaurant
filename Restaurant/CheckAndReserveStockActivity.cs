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
    public static class CheckAndReserveStockActivity
    {
        [FunctionName(Constants.CheckAndReserveStockActivityFunctionName)]
        public static async Task<(bool Reserved, string IngredientName)> Run(
            [ActivityTrigger]DishIngredient neededIngredient,
            [Inject]IBaseRepositoryFactory<StockIngredient> stockRepositoryFactory,
            ILogger log)
        {
            if (neededIngredient == null ||
                string.IsNullOrEmpty(neededIngredient.StockIngredientId) ||
                neededIngredient.QuantityNeeded <= 0) return (false, string.Empty);

            var cosmosDbEndpoint = Environment.GetEnvironmentVariable(Constants.CosmosDbEndpointKeyName);
            var cosmosDbKey = Environment.GetEnvironmentVariable(Constants.CosmosDbKeyKeyName);
            var repo = stockRepositoryFactory.GetInstance(cosmosDbEndpoint, cosmosDbKey, Constants.StockCollectionName);

            var stockIngredient = await repo.GetAsync(neededIngredient.StockIngredientId);
            if (stockIngredient != null)
            {
                if (stockIngredient.StockQuantity - neededIngredient.QuantityNeeded > 0)
                {
                    var updated = await repo.TryUpdateWithRetry(stockIngredient, (si) =>
                    {
                        si.StockQuantity -= neededIngredient.QuantityNeeded;
                    });

                    if (updated)
                    {
                        return (true, string.Empty);
                    }
                }
            }
            else
            {
                //TODO: we have a problem, log maybe?
            }
            return (false, neededIngredient.Name);
        }
    }
}