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
    public static class GetStockIngredients
    {
        [FunctionName("GetStockIngredients")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log,
            [Inject]ITableRepositoryFactory<StockIngredient> tableRepositoryFactory)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var storageConnectionString = System.Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            var repo = tableRepositoryFactory.GetInstance(storageConnectionString, Constants.StockTableName);

            var result = await repo.GetAll();

            return new JsonResult(result);
        }
    }
}