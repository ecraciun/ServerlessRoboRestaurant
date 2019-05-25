using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Linq;

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
            var orchestrators = await client.GetStatusAsync();

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
                    Orchestrators = og.ToList()
                })
            );
        }
    }
}