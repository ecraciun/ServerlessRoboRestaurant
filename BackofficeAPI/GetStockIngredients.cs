using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Core.Services.Interfaces;
using Willezone.Azure.WebJobs.Extensions.DependencyInjection;
using Core.Entities;
using Core;

namespace BackofficeAPI
{
    public static class GetStockIngredients
    {
        [FunctionName("GetStockIngredients")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            [Inject]IBaseRepositoryFactory<StockIngredient> stockIngredientsRepositoryFactory,
            ILogger log)
        {
            var cosmosDbEndpoint = System.Environment.GetEnvironmentVariable(Constants.CosmosDbEndpointKeyName);
            var cosmosDbKey = System.Environment.GetEnvironmentVariable(Constants.CosmosDbKeyKeyName);
            var repo = stockIngredientsRepositoryFactory.GetInstance(cosmosDbEndpoint, cosmosDbKey, Constants.StockCollectionName);

            var result = await repo.GetAll();

            return new JsonResult(result);
        }
    }
}