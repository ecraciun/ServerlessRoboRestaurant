using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Core;
using Core.Entities;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace Restaurant
{
    public static class StockCheckerOrchestrator
    {
        [FunctionName(Constants.StockCheckerOrchestratorFunctionName)]
        public static async Task<bool> RunOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext context)
        {
            var neededIngredients = context.GetInput<List<DishIngredient>>();
            var checkTasks = new List<Task<(bool Reserved, string IngredientName)>>();

            foreach(var ingredient in neededIngredients)
            {
                checkTasks
                    .Add(context.CallActivityAsync<(bool Reserved, string IngredientName)>(Constants.CheckAndReserveStockActivityFunctionName, ingredient));
            }

            var checkResult = await Task.WhenAll(checkTasks);
            var ingredientsToOrder = checkResult.Where(x => x.Reserved == false).Select(x => x.IngredientName).ToList();

            if (ingredientsToOrder.Any())
            {
                var supplierQueryTasks = new List<Task<SupplierQueryResponse>>();

                foreach(var ingredient in ingredientsToOrder)
                {
                    supplierQueryTasks.Add(context.CallActivityAsync<SupplierQueryResponse>(Constants.SupplierFinderActivityFunctionName,
                        new SupplierQueryRequest
                        {
                            IngredientName = ingredient,
                            QueryStrategy = SupplierQueryStrategy.OptimizeDelivery
                        }));
                }

                var supplierQueryResponses = await Task.WhenAll(supplierQueryTasks);
                var groupdResults = supplierQueryResponses.GroupBy(x => x.SupplierId).ToList();

                var supplierOrderTasks = new List<Task>();
                foreach(var group in groupdResults)
                {
                    var supplierOrder = new SupplierOrder
                    {
                        SupplierId = group.Key,
                        OrderedItems = group.Select(x =>
                        {
                            return new SupplierOrderIngredientItem
                            {
                                Name = x.IngredientName,
                                Quantity = Constants.DefaultUrgentIngredientOrderQuantity
                            };
                        }).ToList()
                    };

                    supplierOrderTasks.Add(await context.CallActivityAsync(Constants.CreateSupplierOrderActivityFunctionName,));
                }

                await Task.WhenAll(supplierOrderTasks);

                // check and reserve
            }

            return true;
        }

        [FunctionName("StockCheckerOrchestrator_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")]HttpRequestMessage req,
            [OrchestrationClient]DurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("StockCheckerOrchestrator", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}