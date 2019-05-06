using Core;
using Core.Entities;
using Core.Services.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using Willezone.Azure.WebJobs.Extensions.DependencyInjection;

namespace Restaurant
{
    public static class SupplierFinderActivity
    {
        [FunctionName(Constants.SupplierFinderActivityFunctionName)]
        public static async Task<SupplierQueryResponse> Run(
            [ActivityTrigger]SupplierQueryRequest request,
            [Inject]IBaseRepositoryFactory<Supplier> suppliersRepositoryFactory,
            ILogger log)
        {
            SupplierQueryResponse result = null;

            if (request != null && !string.IsNullOrEmpty(request.IngredientName))
            {
                var cosmosDbEndpoint = Environment.GetEnvironmentVariable(Constants.CosmosDbEndpointKeyName);
                var cosmosDbKey = Environment.GetEnvironmentVariable(Constants.CosmosDbKeyKeyName);
                var repo = suppliersRepositoryFactory.GetInstance(cosmosDbEndpoint, cosmosDbKey, Constants.SuppliersCollectionName);

                var potentialSuppliers = await repo.GetWhereAsync(s =>
                    s.IngredientsForSale.Any(i => i.Name == request.IngredientName));

                if (potentialSuppliers?.Any() ?? false)
                {
                    switch (request.QueryStrategy)
                    {
                        case SupplierQueryStrategy.OptimizeCost:
                            var potentialResponses = potentialSuppliers.Select(x =>
                            {
                                return new SupplierQueryResponse
                                {
                                    SupplierId = x.Id,
                                    IngredientName = request.IngredientName,
                                    TimeToDelivery = x.TimeToDelivery,
                                    UnitPrice = x.IngredientsForSale.First(i => i.Name.Equals(request.IngredientName, StringComparison.OrdinalIgnoreCase)).UnitPrice
                                };
                            });
                            result = potentialResponses.FirstOrDefault(
                                x => x.UnitPrice == potentialResponses.Min(s => s.UnitPrice));
                            break;
                        case SupplierQueryStrategy.OptimizeDelivery:
                            var supplier = potentialSuppliers.FirstOrDefault(x => x.TimeToDelivery == potentialSuppliers.Min(s => s.TimeToDelivery));
                            result = new SupplierQueryResponse
                            {
                                SupplierId = supplier.Id,
                                IngredientName = request.IngredientName,
                                TimeToDelivery = supplier.TimeToDelivery,
                                UnitPrice = supplier.IngredientsForSale.First(i => i.Name.Equals(request.IngredientName, StringComparison.OrdinalIgnoreCase)).UnitPrice
                            };
                            break;
                    }
                }
            }

            return result;
        }
    }
}