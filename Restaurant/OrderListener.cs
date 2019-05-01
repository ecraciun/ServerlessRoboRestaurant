using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace Restaurant
{
    public static class OrderListener
    {
        [FunctionName("OrderListener")]
        public static void Run(
            [QueueTrigger("orders", Connection = "AzureWebJobsStorage")]string myQueueItem,
            [OrchestrationClient] DurableOrchestrationClient starter,
            ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");
        }
    }
}