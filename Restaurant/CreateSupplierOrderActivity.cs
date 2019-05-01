using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Restaurant
{
    public static class CreateSupplierOrderActivity
    {
        [FunctionName("CreateSupplierOrder")]
        public static async Task<bool> Run(
            [ActivityTrigger]string orderId,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            return true;
        }
    }
}
