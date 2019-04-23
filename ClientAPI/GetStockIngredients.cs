using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Table;
using Core;
using Core.Entities;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;

namespace ClientAPI
{
    public static class GetStockIngredients
    {
        [FunctionName("GetStockIngredients")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            //[Table(Constants.StockTableName)] CloudTable stockTable,
            ILogger log, 
            ExecutionContext context)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var cloudStorageAccount = CloudStorageAccount.Parse(config["AzureWebJobsStorage"]);
            var cloudTableClient = cloudStorageAccount.CreateCloudTableClient();
            var stockTable = cloudTableClient.GetTableReference(Constants.StockTableName);

            TableQuery<StockIngredient> query = new TableQuery<StockIngredient>()
                .Where(TableQuery.GenerateFilterCondition(nameof(StockIngredient.PartitionKey), QueryComparisons.Equal, Constants.DefaultPartitionName));
            TableContinuationToken token = null;
            List<StockIngredient> result = new List<StockIngredient>();
            do
            {
                TableQuerySegment<StockIngredient> resultSegment = await stockTable.ExecuteQuerySegmentedAsync(query, token);
                token = resultSegment.ContinuationToken;
                result.AddRange(resultSegment.Results);
            }
            while (token != null);

            return new JsonResult(result);

            //string name = req.Query["name"];

            //string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            //dynamic data = JsonConvert.DeserializeObject(requestBody);
            //name = name ?? data?.name;

            //return name != null
            //    ? (ActionResult)new OkObjectResult($"Hello, {name}")
            //    : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
        }
    }
}
