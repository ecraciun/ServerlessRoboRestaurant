using Core;
using Core.Entities;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Restaurant
{
    public static class StockCheckerOrchestrator
    {
        [FunctionName(Constants.StockCheckerOrchestratorFunctionName)]
        public static async Task<bool> RunOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext context)
        {
            var neededIngredients = context.GetInput<List<DishIngredient>>();

            var ingredientsToOrder = await HandleNeededStock(context, neededIngredients);

            while (ingredientsToOrder.Any())
            {
                var groupdResults = await GetNeededSuppliers(context, ingredientsToOrder);

                await CreateSupplierOrders(context, groupdResults);

                var leftOverNeededIngredients = neededIngredients
                    .Where(x => ingredientsToOrder.Contains(x.Name)).ToList();

                ingredientsToOrder = await HandleNeededStock(context, leftOverNeededIngredients);
            }

            return true;
        }

        private static async Task CreateSupplierOrders(DurableOrchestrationContext context,
            List<IGrouping<string, SupplierQueryResponse>> groupdResults)
        {
            var supplierOrderTasks = new List<Task>();
            foreach (var group in groupdResults)
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

                supplierOrderTasks.Add(
                    context.CallActivityAsync(Constants.CreateSupplierOrderActivityFunctionName, supplierOrder));
            }

            await Task.WhenAll(supplierOrderTasks);
        }

        private static async Task<List<IGrouping<string, SupplierQueryResponse>>> GetNeededSuppliers(
            DurableOrchestrationContext context, List<string> ingredientsToOrder)
        {
            var supplierQueryTasks = new List<Task<SupplierQueryResponse>>();

            foreach (var ingredient in ingredientsToOrder)
            {
                supplierQueryTasks.Add(context.CallActivityAsync<SupplierQueryResponse>(
                    Constants.SupplierFinderActivityFunctionName,
                    new SupplierQueryRequest
                    {
                        IngredientName = ingredient,
                        QueryStrategy = SupplierQueryStrategy.OptimizeDelivery
                    }));
            }

            var supplierQueryResponses = await Task.WhenAll(supplierQueryTasks);
            var groupdResults = supplierQueryResponses.GroupBy(x => x.SupplierId).ToList();
            return groupdResults;
        }

        private static async Task<List<string>> HandleNeededStock(DurableOrchestrationContext context,
            List<DishIngredient> neededIngredients)
        {
            var checkTasks = new List<Task<(bool Reserved, string IngredientName)>>();

            foreach (var ingredient in neededIngredients)
            {
                checkTasks
                    .Add(context.CallActivityAsync<(bool Reserved, string IngredientName)>(
                        Constants.CheckAndReserveStockActivityFunctionName, ingredient));
            }

            var checkResult = await Task.WhenAll(checkTasks);
            var ingredientsToOrder = checkResult
                .Where(x => x.Reserved == false && !string.IsNullOrEmpty(x.IngredientName))
                .Select(x => x.IngredientName).ToList();
            return ingredientsToOrder;
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