using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Linq;
using System;
using System.Collections.Generic;

namespace Restaurant
{
    public static class GetAllOrchestratorsStatus
    {
        [FunctionName("GetAllOrchestratorsStatus")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            [OrchestrationClient] DurableOrchestrationClientBase client,
            ILogger log)
        {
            var statusList = Enum.GetValues(typeof(OrchestrationRuntimeStatus)).Cast<OrchestrationRuntimeStatus>();
            var limitToToday = req.Query.ContainsKey("today") ?
                req.Query["today"] == "1" :
                false;

            IList<DurableOrchestrationStatus> orchestrators;
            if (limitToToday)
            {
                orchestrators = await client.GetStatusAsync(DateTime.Today.AddHours(-2.0), null, statusList);
            }
            else
            {
                orchestrators = await client.GetStatusAsync();
            }

            var expandResults = req.Query.ContainsKey("expand") ?
                req.Query["expand"] == "1" :
                false;

            var result = orchestrators.OrderByDescending(o => o.CreatedTime)
                .Select(o => new
                {
                    o.InstanceId,
                    o.Name,
                    RuntimeStatus = o.RuntimeStatus.ToString(),
                    o.CustomStatus,
                    CreatedTime = o.CreatedTime.ToLongTimeString(),
                    LastUpdatedTime = o.LastUpdatedTime.ToLongTimeString()
                });

            return new OkObjectResult(
                result.GroupBy(o => o.RuntimeStatus).Select(og => new
                {
                    Status = og.Key.ToString(),
                    Count = og.Count(),
                    Orchestrators = expandResults ? og.ToList() : null
                })
            );
        }
    }
}