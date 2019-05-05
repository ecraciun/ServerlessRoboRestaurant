using Core;
using Core.Entities;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
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

#if DEBUG

        [FunctionName("DishOrchestrator_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")]HttpRequestMessage req,
            [OrchestrationClient]DurableOrchestrationClient starter,
            ILogger log)
        {
            string requestBody = await req.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(requestBody))
            {
                var dish = JsonConvert.DeserializeObject<Dish>(requestBody);
                // Function input comes from the request content.
                string instanceId = await starter.StartNewAsync(Constants.DishOrchestratorFunctionName, dish);

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