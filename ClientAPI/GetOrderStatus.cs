using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Core.Services.Interfaces;
using Core.Entities;
using Willezone.Azure.WebJobs.Extensions.DependencyInjection;
using Core;

namespace ClientAPI
{
    public static class GetOrderStatus
    {
        [FunctionName("GetOrderStatus")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            [Inject]IBaseRepositoryFactory<Order> ordersRepositoryFactory,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string orderId = req.Query["orderId"];
            if(Guid.TryParse(orderId, out var orderGuid))
            {
                var cosmosDbEndpoint = Environment.GetEnvironmentVariable("CosmosDbEndpoint");
                var cosmosDbKey = Environment.GetEnvironmentVariable("CosmosDbKey");
                var repo = ordersRepositoryFactory.GetInstance(cosmosDbEndpoint, cosmosDbKey, Constants.OrdersCollectionName);
                var result = await repo.Get(orderId);
                if (result == null) return new NotFoundResult();
                return new JsonResult(result.Status);
            }

            return new BadRequestObjectResult("Missing order id, or not a valid id");
        }
    }
}