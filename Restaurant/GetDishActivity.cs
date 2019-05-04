using Core;
using Core.Entities;
using Core.Services.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Willezone.Azure.WebJobs.Extensions.DependencyInjection;

namespace Restaurant
{
    public static class GetDishActivity
    {
        [FunctionName(Constants.GetDishActivityFunctionName)]
        public static async Task<Dish> Run(
            [ActivityTrigger]string dishId,
            [Inject]IBaseRepositoryFactory<Dish> dishesRepositoryFactory,
            ILogger log)
        {
            Dish result = null;

            if (!string.IsNullOrEmpty(dishId))
            {
                var cosmosDbEndpoint = Environment.GetEnvironmentVariable(Constants.CosmosDbEndpointKeyName);
                var cosmosDbKey = Environment.GetEnvironmentVariable(Constants.CosmosDbKeyKeyName);
                var repo = dishesRepositoryFactory.GetInstance(cosmosDbEndpoint, cosmosDbKey, Constants.DishesCollectionName);
                result = await repo.GetAsync(dishId);
            }

            return result;
        }
    }
}