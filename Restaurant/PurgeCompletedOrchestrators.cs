using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Linq;
using System;
using System.Collections.Generic;
using DurableTask.Core;

namespace Restaurant
{
    public static class PurgeCompletedOrchestrators
    {
        [FunctionName("PurgeCompletedOrchestrators")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            [OrchestrationClient] DurableOrchestrationClientBase client,
            ILogger log)
        {
            var statusList = new List<OrchestrationStatus> { OrchestrationStatus.Canceled, OrchestrationStatus.Completed, OrchestrationStatus.Failed, OrchestrationStatus.Terminated };
            var result = await client.PurgeInstanceHistoryAsync(DateTime.MinValue, null, statusList.AsEnumerable());
            return new OkObjectResult($"Deleted: {result.InstancesDeleted}");
        }
    }
}