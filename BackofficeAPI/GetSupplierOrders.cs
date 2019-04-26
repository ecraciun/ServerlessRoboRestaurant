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
using System.Threading.Tasks;
using Willezone.Azure.WebJobs.Extensions.DependencyInjection;

namespace BackofficeAPI
{
    public static class GetSupplierOrders
    {
        [FunctionName("GetSupplierOrders")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            [Inject]IBaseRepositoryFactory<SupplierOrder> supplierOrdersRepositoryFactory,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var cosmosDbEndpoint = Environment.GetEnvironmentVariable(Constants.CosmosDbEndpointKeyName);
            var cosmosDbKey = Environment.GetEnvironmentVariable(Constants.CosmosDbKeyKeyName);
            var repo = supplierOrdersRepositoryFactory.GetInstance(cosmosDbEndpoint, cosmosDbKey, Constants.SupplierOrdersCollectionName);

            string status = req.Query["status"];
            IList<SupplierOrder> result = null;
            if (string.IsNullOrEmpty(status))
            {
                result = await repo.GetAll();
            }
            if (Enum.TryParse(status, out SupplierOrderStatus orderStatus))
            {
                result = await repo.GetWhere(x => x.Status == orderStatus);
            }

            return new JsonResult(result);
        }
    }
}
