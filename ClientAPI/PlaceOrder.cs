using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Core.Entities;
using Core.Services.Interfaces;
using Willezone.Azure.WebJobs.Extensions.DependencyInjection;
using Core;

namespace ClientAPI
{
    public static class PlaceOrder
    {
        [FunctionName("PlaceOrder")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            [Inject]IBaseRepositoryFactory<Order> ordersRepositoryFactory,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            try
            {
                var order = JsonConvert.DeserializeObject<Order>(requestBody);
                var cosmosDbEndpoint = Environment.GetEnvironmentVariable("CosmosDbEndpoint");
                var cosmosDbKey = Environment.GetEnvironmentVariable("CosmosDbKey");
                var repo = ordersRepositoryFactory.GetInstance(cosmosDbEndpoint, cosmosDbKey, Constants.OrdersCollectionName);
                var id = await repo.Add(order);
                return new CreatedResult($"api/{nameof(GetOrderStatus)}/{id}", null);
            }
            catch(JsonSerializationException ex)
            {
                log.LogError("Order deserialization error", ex);
                return new BadRequestObjectResult("Invalid order");
            }
        }
    }
}