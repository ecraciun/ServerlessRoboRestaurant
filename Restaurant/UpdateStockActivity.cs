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
    public static class UpdateStockActivity
    {
        [FunctionName(Constants.UpdateStockActivityFunctionName)]
        public static async Task<bool> Run(
            [ActivityTrigger](string IngredientId, int Delta) stockIngredientUpdate,
            [Inject]IBaseRepositoryFactory<StockIngredient> stockRepositoryFactory,
            ILogger log)
        {
            if (!string.IsNullOrEmpty(stockIngredientUpdate.IngredientId) && stockIngredientUpdate.Delta != 0)
            {
                var cosmosDbEndpoint = Environment.GetEnvironmentVariable(Constants.CosmosDbEndpointKeyName);
                var cosmosDbKey = Environment.GetEnvironmentVariable(Constants.CosmosDbKeyKeyName);
                var repo = stockRepositoryFactory.GetInstance(cosmosDbEndpoint, cosmosDbKey, Constants.StockCollectionName);

                Action<StockIngredient> updateAction = (stockIngredient) =>
                {
                    stockIngredient.StockQuantity += stockIngredientUpdate.Delta;
                };

                var stockIngredientEntity = await repo.GetAsync(stockIngredientUpdate.IngredientId);
                if (stockIngredientEntity != null)
                {
                    return await repo.TryUpdateWithRetry(stockIngredientEntity, updateAction);
                }
            }

            return false;
        }
    }
}