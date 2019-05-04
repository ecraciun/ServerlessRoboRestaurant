using Core;
using Core.Entities;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Restaurant
{
    public static class RecipeStepActivity
    {
        [FunctionName(Constants.RecipeStepActivityFunctionName)]
        public static async Task Run(
            [ActivityTrigger]RecipeStep recipeStep,
            ILogger log)
        {
            if (recipeStep != null && recipeStep.SecondsRequired > 0)
            {
                await Task.Delay(TimeSpan.FromSeconds(recipeStep.SecondsRequired));
            }
        }
    }
}