using Core;
using Core.Entities;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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
            [OrchestrationTrigger] DurableOrchestrationContextBase context)
        {
            var neededIngredients = context.GetInput<List<DishIngredient>>();

            var ingredientsToOrder = await HandleNeededStock(context, neededIngredients);

            while (ingredientsToOrder.Any()) // possible infinite loop!!!
            {
                var groupedSupplierResults = await GetNeededSuppliers(context, ingredientsToOrder);

                await CreateSupplierOrders(context, groupedSupplierResults);

                var leftOverNeededIngredients = neededIngredients
                    .Where(x => ingredientsToOrder.Contains(x.Name)).ToList();

                var currentStockIngredients = await context.CallActivityAsync<IList<StockIngredient>>(
                Constants.GetStockActivityFunctionName, null);

                await UpdateStockQuantities(context,
                    currentStockIngredients.Where(x => ingredientsToOrder.Contains(x.Name)).ToList());

                ingredientsToOrder = await HandleNeededStock(context, leftOverNeededIngredients);
            }

            return true;
        }

        private static async Task UpdateStockQuantities(DurableOrchestrationContextBase context,
            List<StockIngredient> ingredientsThatNeedReplenishing)
        {
            var updateStockTasks = new List<Task<bool>>();
            foreach (var ingredient in ingredientsThatNeedReplenishing)
            {
                updateStockTasks.Add(context.CallActivityAsync<bool>(
                    Constants.UpdateStockActivityFunctionName,
                    (ingredient.Id, Constants.DefaultUrgentIngredientOrderQuantity)));
            }
            await Task.WhenAll(updateStockTasks);
        }

        private static async Task CreateSupplierOrders(DurableOrchestrationContextBase context,
            List<IGrouping<string, SupplierQueryResponse>> groupdResults)
        {
            var supplierOrderTasks = new List<Task<bool>>();
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
                    context.CallActivityAsync<bool>(Constants.CreateSupplierOrderActivityFunctionName, supplierOrder));
            }

            await Task.WhenAll(supplierOrderTasks);
        }

        private static async Task<List<IGrouping<string, SupplierQueryResponse>>> GetNeededSuppliers(
            DurableOrchestrationContextBase context, List<string> ingredientsToOrder)
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
            var groupedResults = supplierQueryResponses.GroupBy(x => x.SupplierId).ToList();
            return groupedResults;
        }

        private static async Task<List<string>> HandleNeededStock(DurableOrchestrationContextBase context,
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

#if DEBUG

        [FunctionName("StockCheckerOrchestrator_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")]HttpRequestMessage req,
            [OrchestrationClient]DurableOrchestrationClientBase starter,
            ILogger log)
        {
            string requestBody = await req.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(requestBody))
            {
                var ingredients = JsonConvert.DeserializeObject<List<DishIngredient>>(requestBody);
                // Function input comes from the request content.
                string instanceId = await starter.StartNewAsync(Constants.StockCheckerOrchestratorFunctionName, ingredients);

                return starter.CreateCheckStatusResponse(req, instanceId);
            }

            return new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.BadRequest
            };
        }
#endif
    }
}