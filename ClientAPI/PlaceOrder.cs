using Core;
using Core.Entities;
using Core.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Willezone.Azure.WebJobs.Extensions.DependencyInjection;

namespace ClientAPI
{
    public static class PlaceOrder
    {
        /// <summary>
        ///     Creates a new order and returns the URL to the GetOrderStatus endpoint for this current order
        /// </summary>
        /// <param name="req"></param>
        /// <param name="ordersRepositoryFactory"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName(Constants.PlaceOrderFunctionName)]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            [Inject]IBaseRepositoryFactory<Order> ordersRepositoryFactory,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            if (!string.IsNullOrEmpty(requestBody))
            {
                try
                {
                    var order = JsonConvert.DeserializeObject<Order>(requestBody);
                    if (order.OrderItems == null || !order.OrderItems.Any())
                    {
                        return new BadRequestObjectResult("Invalid order");
                    }
                    order.LastModifiedUtc = order.TimePlacedUtc = DateTime.UtcNow;
                    order.Status = OrderStatus.New;

                    var cosmosDbEndpoint = Environment.GetEnvironmentVariable(Constants.CosmosDbEndpointKeyName);
                    var cosmosDbKey = Environment.GetEnvironmentVariable(Constants.CosmosDbKeyKeyName);
                    var repo = ordersRepositoryFactory.GetInstance(cosmosDbEndpoint, cosmosDbKey, Constants.OrdersCollectionName);
                    var id = await repo.AddAsync(order);
                    return new CreatedResult($"api/{nameof(GetOrderStatus)}/{id}", null);
                }
                catch (Exception ex) when (ex is JsonReaderException || ex is JsonSerializationException)
                {
                    log.LogError("Order deserialization error", ex);
                    return new BadRequestObjectResult("Invalid order");
                }
            }

            return new BadRequestResult();
        }
    }
}