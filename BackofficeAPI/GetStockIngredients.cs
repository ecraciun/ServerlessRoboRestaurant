using Core;
using Core.Entities;
using Core.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Willezone.Azure.WebJobs.Extensions.DependencyInjection;

namespace BackofficeAPI
{
    public static class GetStockIngredients
    {
        /// <summary>
        ///     Gets all ingredients in the stock
        /// </summary>
        /// <param name="req"></param>
        /// <param name="stockIngredientsRepositoryFactory"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName(Constants.GetStockIngredientsFunctionName)]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            [Inject]IBaseRepositoryFactory<StockIngredient> stockIngredientsRepositoryFactory,
            ILogger log)
        {
            var cosmosDbEndpoint = System.Environment.GetEnvironmentVariable(Constants.CosmosDbEndpointKeyName);
            var cosmosDbKey = System.Environment.GetEnvironmentVariable(Constants.CosmosDbKeyKeyName);
            var repo = stockIngredientsRepositoryFactory.GetInstance(cosmosDbEndpoint, cosmosDbKey, Constants.StockCollectionName);

            var result = await repo.GetAllAsync();

            return new JsonResult(result);
        }
    }
}