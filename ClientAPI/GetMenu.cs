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

namespace ClientAPI
{
    public static class GetMenu
    {
        [FunctionName("GetMenu")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log,
            [Inject]IBaseRepositoryFactory<Dish> dishesRepositoryFactory)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var cosmosDbEndpoint = System.Environment.GetEnvironmentVariable("CosmosDbEndpoint");
            var cosmosDbKey = System.Environment.GetEnvironmentVariable("CosmosDbKey");
            var repo = dishesRepositoryFactory.GetInstance(cosmosDbEndpoint, cosmosDbKey, Constants.DishesCollectionName);

            var result = await repo.GetAll();

            return new JsonResult(result);
        }
    }
}