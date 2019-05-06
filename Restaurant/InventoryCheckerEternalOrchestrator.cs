using Core;
using Core.Entities;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Restaurant
{
    public static class InventoryCheckerEternalOrchestrator
    {
        [FunctionName(Constants.InventoryCheckerEternalOrchestratorFunctionName)]
        public static async Task RunOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext context)
        {
            context.SetCustomStatus("Starting new run");
            var currentStockIngredients = await context.CallActivityAsync<IList<StockIngredient>>(
                Constants.GetStockActivityFunctionName, null);

            var ingredientsThatNeedReplenishing = currentStockIngredients
                .Where(x => x.StockQuantity < Constants.RegularInventoryCheckMinimumThreshold).ToList();

            context.SetCustomStatus("Finding appropriate suppliers");
            var groupedSupplierResults = await GetNeededSuppliers(context,
                ingredientsThatNeedReplenishing.Select(x => x.Name).ToList());

            context.SetCustomStatus("Creating and waiting for supplier orders");
            await CreateSupplierOrders(context, groupedSupplierResults);

            context.SetCustomStatus("Updating stock");
            await UpdateStockQuantities(context, ingredientsThatNeedReplenishing);

            context.SetCustomStatus("Sleeping");
            DateTime nextCheck = context.CurrentUtcDateTime.AddSeconds(Constants.RegularInventoryCheckSleepTimeInSeconds);
            await context.CreateTimer(nextCheck, CancellationToken.None);

            context.ContinueAsNew(null);
        }

        // TODO: copy pasta not nice!
        private static async Task UpdateStockQuantities(DurableOrchestrationContext context, 
            List<StockIngredient> ingredientsThatNeedReplenishing)
        {
            var updateStockTasks = new List<Task<bool>>();
            foreach(var ingredient in ingredientsThatNeedReplenishing)
            {
                updateStockTasks.Add(context.CallActivityAsync<bool>(
                    Constants.UpdateStockActivityFunctionName, 
                    (ingredient.Id, Constants.DefaultRegularIngredientOrderQuantity)));
            }
            await Task.WhenAll(updateStockTasks);
        }

        // TODO: copy pasta not nice!
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
                        QueryStrategy = SupplierQueryStrategy.OptimizeCost
                    }));
            }

            var supplierQueryResponses = await Task.WhenAll(supplierQueryTasks);
            var groupdResults = supplierQueryResponses.GroupBy(x => x.SupplierId).ToList();
            return groupdResults;
        }

        // TODO: copy pasta not nice!
        private static async Task CreateSupplierOrders(DurableOrchestrationContext context,
            List<IGrouping<string, SupplierQueryResponse>> groupdResults)
        {
            var supplierOrderTasks = new List<Task>();
            foreach (var group in groupdResults)
            {
                var supplierOrder = new SupplierOrder
                {
                    SupplierId = group.Key,
                    DeliveryETAInSeconds = group.First().TimeToDelivery,
                    OrderedItems = group.Select(x =>
                    {
                        return new SupplierOrderIngredientItem
                        {
                            Name = x.IngredientName,
                            Quantity = Constants.DefaultRegularIngredientOrderQuantity
                        };
                    }).ToList()
                };

                supplierOrderTasks.Add(
                    context.CallActivityAsync(Constants.CreateSupplierOrderActivityFunctionName, supplierOrder));
            }

            await Task.WhenAll(supplierOrderTasks);
        }

#if DEBUG
        [FunctionName("InventoryCheckerEternalOrchestrator_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")]HttpRequestMessage req,
            [OrchestrationClient]DurableOrchestrationClient starter,
            ILogger log)
        {
            // Check if an instance with the specified ID already exists.
            var existingInstance = await starter.GetStatusAsync(Constants.InventoryCheckerOrchestratorId);
            if (existingInstance == null || existingInstance.RuntimeStatus == OrchestrationRuntimeStatus.Failed)
            {
                if(existingInstance != null)
                {
                    await starter.PurgeInstanceHistoryAsync(Constants.InventoryCheckerEternalOrchestratorFunctionName);
                    //await starter.TerminateAsync(Constants.InventoryCheckerEternalOrchestratorFunctionName, "Cleanup failed run");
                }
                // An instance with the specified ID doesn't exist, create one.
                await starter.StartNewAsync(Constants.InventoryCheckerEternalOrchestratorFunctionName,
                    Constants.InventoryCheckerOrchestratorId, null);
                return starter.CreateCheckStatusResponse(req, Constants.InventoryCheckerOrchestratorId);
            }
            else
            {
                await starter.TerminateAsync(Constants.InventoryCheckerEternalOrchestratorFunctionName, "Force start new");
                await starter.PurgeInstanceHistoryAsync(Constants.InventoryCheckerEternalOrchestratorFunctionName);

                // An instance with the specified ID exists, don't create one.
                return req.CreateErrorResponse(
                    HttpStatusCode.Conflict,
                    $"An instance with ID '{Constants.InventoryCheckerOrchestratorId}' already exists.");
            }
        }
#endif
    }
}