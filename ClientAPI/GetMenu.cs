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

namespace ClientAPI
{
    public static class GetMenu
    {
        /// <summary>
        ///     Gets restaurant available Dish items, filtered by type
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <param name="dishesRepositoryFactory"></param>
        /// <returns></returns>
        [FunctionName(Constants.GetMenuFunctionName)]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log,
            [Inject]IBaseRepositoryFactory<Dish> dishesRepositoryFactory)
        {
            var cosmosDbEndpoint = Environment.GetEnvironmentVariable(Constants.CosmosDbEndpointKeyName);
            var cosmosDbKey = Environment.GetEnvironmentVariable(Constants.CosmosDbKeyKeyName);
            var repo = dishesRepositoryFactory.GetInstance(cosmosDbEndpoint, cosmosDbKey, Constants.DishesCollectionName);

            string type = req.Query["type"];
            IList<Dish> result = null;
            if (string.IsNullOrEmpty(type))
            {
                result = await repo.GetAllAsync();
            }
            if (Enum.TryParse(type, out DishType dishType) &&
                Enum.IsDefined(typeof(DishType), dishType))
            {
                result = await repo.GetWhereAsync(x => x.Type == dishType);
            }

            return new JsonResult(result);
        }
    }
}