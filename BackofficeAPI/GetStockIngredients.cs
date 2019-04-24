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
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log,
            [Inject]IBaseRepositoryFactory<StockIngredient> stockIngredientsRepositoryFactory)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var cosmosDbEndpoint = System.Environment.GetEnvironmentVariable("CosmosDbEndpoint");
            var cosmosDbKey = System.Environment.GetEnvironmentVariable("CosmosDbKey");
            var repo = stockIngredientsRepositoryFactory.GetInstance(cosmosDbEndpoint, cosmosDbKey, Constants.StockCollectionName);

            var result = await repo.GetAll();

            return new JsonResult(result);
        }
    }
}