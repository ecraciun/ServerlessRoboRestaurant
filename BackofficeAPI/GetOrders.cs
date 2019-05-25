using Core;
using Core.Entities;
using Core.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Willezone.Azure.WebJobs.Extensions.DependencyInjection;

namespace BackofficeAPI
{
    public static class GetOrders
    {
        /// <summary>
        ///     Gets all orders in the system, or filtered by their status
        /// </summary>
        /// <param name="req"></param>
        /// <param name="ordersRepositoryFactory"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName(Constants.GetOrdersFunctionName)]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            [Inject]IBaseRepositoryFactory<Order> ordersRepositoryFactory,
            ILogger log)
        {
            var cosmosDbEndpoint = Environment.GetEnvironmentVariable(Constants.CosmosDbEndpointKeyName);
            var cosmosDbKey = Environment.GetEnvironmentVariable(Constants.CosmosDbKeyKeyName);
            var repo = ordersRepositoryFactory.GetInstance(cosmosDbEndpoint, cosmosDbKey, Constants.OrdersCollectionName);

            string status = req.Query["status"];
            IList<Order> result = null;
            if (string.IsNullOrEmpty(status))
            {
                result = await repo.GetAllAsync();
            }
            if (Enum.TryParse(status, out OrderStatus orderStatus) &&
                Enum.IsDefined(typeof(OrderStatus), orderStatus))
            {
                result = await repo.GetWhereAsync(x => x.Status == orderStatus);
            }

            var groupedResult = result.OrderByDescending(o => o.TimePlacedUtc).GroupBy(o => o.Status).Select(go =>
            new {
                Status = go.Key.ToString(),
                Orders = go.ToList()
            });

            return new JsonResult(groupedResult);
        }
    }
}