using Core;
using Core.Entities;
using Microsoft.Azure.WebJobs;
using System.Threading.Tasks;

namespace Restaurant
{
    public static class DishOrchestrator
    {
        [FunctionName(Constants.DishOrchestratorFunctionName)]
        public static async Task<bool> RunOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext context)
        {
            var dish = context.GetInput<Dish>();
            if (dish != null && dish.Recipe != null && dish.Recipe.Steps != null)
            {
                foreach (var step in dish.Recipe.Steps)
                {
                    await context.CallActivityAsync(Constants.RecipeStepActivityFunctionName, step);
                }
                return true;
            }

            return false;
        }
    }
}