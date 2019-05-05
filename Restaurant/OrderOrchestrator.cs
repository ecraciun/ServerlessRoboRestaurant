using System;
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
    public static class OrderOrchestrator
    {
        [FunctionName(Constants.OrderOrchestratorFunctionName)]
        public static async Task RunOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext context)
        {
            var order = context.GetInput<Order>();

            if( order != null && 
                (order.OrderItems?.Any() ?? false) &&
                order.LastModifiedUtc == order.TimePlacedUtc &&
                order.Status == OrderStatus.New)
            {
                bool orderUpdated = await UpdateOrder(context, order, OrderStatus.Preparing);

                var dishesToPrepare = await GetDishesToPrepare(context, order);

                var neededIngredients = GetAllNeededIngredientsForOrder(dishesToPrepare);

                var ingredientsReserved = await context.CallSubOrchestratorAsync<bool>(
                    Constants.StockCheckerOrchestratorFunctionName, neededIngredients
                    );

                if (ingredientsReserved)
                {
                    await PrepareDishes(context, dishesToPrepare);

                    orderUpdated = await UpdateOrder(context, order, OrderStatus.Ready);
                }
            }
        }

        private static async Task<bool> UpdateOrder(DurableOrchestrationContext context, Order order, OrderStatus stauts)
        {
            return await context.CallActivityAsync<bool>(
                Constants.UpdateOrderActivityFunctionName,
                new OrderDTO
                {
                    OrchestratorId = context.InstanceId,
                    OrderId = order.Id,
                    OrderStatus = stauts
                });
        }

        private static async Task PrepareDishes(DurableOrchestrationContext context, List<Dish> dishesToPrepare)
        {
            var dishOrchestratorTasks = new List<Task<bool>>();
            foreach (var dishToPrepare in dishesToPrepare)
            {
                dishOrchestratorTasks.Add(
                    context.CallSubOrchestratorAsync<bool>(
                        Constants.DishOrchestratorFunctionName, dishToPrepare));
            }

            var allPrepared = await Task.WhenAll(dishOrchestratorTasks);
            // TODO: maybe check if all were done
        }

        private static async Task<List<Dish>> GetDishesToPrepare(DurableOrchestrationContext context, Order order)
        {
            var getDishesTasks = new List<Task<Dish>>();
            foreach (var orderedItem in order.OrderItems)
            {
                getDishesTasks.Add(context.CallActivityAsync<Dish>(
                    Constants.GetDishActivityFunctionName, orderedItem.DishId));
            }

            var dishes = await Task.WhenAll(getDishesTasks);
            var dishesToPrepare = new List<Dish>();

            foreach(var dish in dishes)
            {
                var quantity = order.OrderItems.FirstOrDefault(x => x.DishId == dish.Id).Quantity;
                
                while(quantity > 0)
                {
                    dishesToPrepare.Add(dish);
                    quantity--;
                }
            }

            return dishesToPrepare;
        }

        private static List<DishIngredient> GetAllNeededIngredientsForOrder(List<Dish> dishes)
        {
            var allIngredients = dishes.SelectMany(x => x.Ingredients)
                .GroupBy(x => x.StockIngredientId)
                .Select(x => new DishIngredient
                {
                    StockIngredientId = x.Key,
                    Name = x.FirstOrDefault().Name,
                    QuantityNeeded = x.Sum(s => s.QuantityNeeded)
                })
                .ToList();
            return allIngredients;
        }
    }
}