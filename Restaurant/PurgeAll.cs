using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Restaurant
{
    public static class PurgeAll
    {
        [FunctionName("PurgeAll")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            [OrchestrationClient] DurableOrchestrationClientBase client,
            ILogger log)
        {
            var statusList = new List<OrchestrationRuntimeStatus> { OrchestrationRuntimeStatus.Running, OrchestrationRuntimeStatus.Pending, OrchestrationRuntimeStatus.ContinuedAsNew };
            var orchestrators = await client.GetStatusAsync(DateTime.MinValue, null, statusList);
            foreach(var orchestrator in orchestrators)
            {
                await client.TerminateAsync(orchestrator.InstanceId, "Force stop");
                await client.PurgeInstanceHistoryAsync(orchestrator.InstanceId);
            }
            return new OkObjectResult($"Stopped and deleted: {orchestrators.Count}");
        }
    }
}