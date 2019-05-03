using System.Collections.Generic;
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

            // Replace "hello" with the name of your Durable Activity Function.
            await context.CallActivityAsync<string>("OrderOrchestrator_Hello", "London")
        }

        [FunctionName("OrderOrchestrator_Hello")]
        public static string SayHello([ActivityTrigger] string name, ILogger log)
        {
            log.LogInformation($"Saying hello to {name}.");
            return $"Hello {name}!";
        }
    }
}