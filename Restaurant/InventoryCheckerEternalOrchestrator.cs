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
            [OrchestrationTrigger] DurableOrchestrationContextBase context,
            [OrchestrationClient]DurableOrchestrationClientBase client)
        {
            context.SetCustomStatus("Starting new run");
            IList<StockIngredient> currentStockIngredients = await GetCurrentStockWithTimeout(context);

            if(currentStockIngredients != null)
            {
                var ingredientsThatNeedReplenishing = currentStockIngredients
                .Where(x => x.StockQuantity < Constants.RegularInventoryCheckMinimumThreshold).ToList();

                if (ingredientsThatNeedReplenishing.Any())
                {
                    context.SetCustomStatus("Finding appropriate suppliers");
                    var groupedSupplierResults = await GetNeededSuppliers(context,
                        ingredientsThatNeedReplenishing.Select(x => x.Name).ToList());

                    context.SetCustomStatus("Creating and waiting for supplier orders");
                    var orderIds = await CreateSupplierOrders(context, groupedSupplierResults);

                    context.SetCustomStatus("Starting monitor");
                    await client.StartNewAsync(Constants.SupplierOrderMonitorOrchestratorFunctionName, orderIds); // fire and forget
                }
            }

            context.SetCustomStatus("Sleeping");
            DateTime nextCheck = context.CurrentUtcDateTime.AddSeconds(Constants.RegularInventoryCheckSleepTimeInSeconds);
            await context.CreateTimer(nextCheck, CancellationToken.None);

            context.ContinueAsNew(null);
        }

        private static async Task<IList<StockIngredient>> GetCurrentStockWithTimeout(DurableOrchestrationContextBase context)
        {
            var deadline = context.CurrentUtcDateTime.AddSeconds(30);
            using (var cts = new CancellationTokenSource())
            {
                var activityTask = context.CallActivityAsync<IList<StockIngredient>>(Constants.GetStockActivityFunctionName, null);
                var timeoutTask = context.CreateTimer(deadline, cts.Token);

                var winner = await Task.WhenAny(activityTask, timeoutTask);
                if (winner == activityTask)
                {
                    cts.Cancel();
                    return activityTask.Result;
                }
                else
                {
                    return null;
                }
            }
        }

        private static async Task UpdateStockQuantities(DurableOrchestrationContextBase context, 
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
                        QueryStrategy = SupplierQueryStrategy.OptimizeCost
                    }));
            }

            var supplierQueryResponses = await Task.WhenAll(supplierQueryTasks);
            var groupdResults = supplierQueryResponses.GroupBy(x => x.SupplierId).ToList();
            return groupdResults;
        }

        
        private static async Task<List<string>> CreateSupplierOrders(DurableOrchestrationContextBase context,
            List<IGrouping<string, SupplierQueryResponse>> groupdResults)
        {
            var supplierOrderTasks = new List<Task<string>>();
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
                    context.CallActivityAsync<string>(Constants.CreateSupplierOrderActivityFunctionName, supplierOrder));
            }

            var orderIds = await Task.WhenAll(supplierOrderTasks);
            return orderIds.ToList();
        }

#if DEBUG
        [FunctionName("InventoryCheckerEternalOrchestrator_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")]HttpRequestMessage req,
            [OrchestrationClient]DurableOrchestrationClientBase starter,
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
                await starter.TerminateAsync(Constants.InventoryCheckerOrchestratorId, "Force start new");
                await starter.PurgeInstanceHistoryAsync(Constants.InventoryCheckerOrchestratorId);

                // An instance with the specified ID exists, don't create one.
                return req.CreateErrorResponse(
                    HttpStatusCode.Conflict,
                    $"An instance with ID '{Constants.InventoryCheckerOrchestratorId}' already exists.");
            }
        }
#endif
    }
}