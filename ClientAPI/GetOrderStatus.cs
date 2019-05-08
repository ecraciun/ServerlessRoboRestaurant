using Core;
using Core.Entities;
using Core.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Willezone.Azure.WebJobs.Extensions.DependencyInjection;

namespace ClientAPI
{
    public static class GetOrderStatus
    {
        /// <summary>
        ///     Get an order's status
        /// </summary>
        /// <param name="req"></param>
        /// <param name="ordersRepositoryFactory"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName(Constants.GetOrderStatusFunctionName)]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            [Inject]IBaseRepositoryFactory<Order> ordersRepositoryFactory,
            ILogger log)
        {
            string orderId = req.Query["orderId"];
            if (Guid.TryParse(orderId, out var orderGuid))
            {
                var cosmosDbEndpoint = Environment.GetEnvironmentVariable(Constants.CosmosDbEndpointKeyName);
                var cosmosDbKey = Environment.GetEnvironmentVariable(Constants.CosmosDbKeyKeyName);
                var repo = ordersRepositoryFactory.GetInstance(cosmosDbEndpoint, cosmosDbKey, Constants.OrdersCollectionName);
                var result = await repo.GetAsync(orderId);
                if (result == null) return new NotFoundResult();
                return new JsonResult(result.Status);
            }

            return new BadRequestObjectResult("Missing order id, or not a valid id");
        }
    }
}