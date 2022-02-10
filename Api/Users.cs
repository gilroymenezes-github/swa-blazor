using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;

namespace BlazorApp.Api
{
    public class Users
    {
        private readonly ILogger<Users> _logger;

        public Users(ILogger<Users> log)
        {
            _logger = log;
        }

        [FunctionName(nameof(Users))]
        [OpenApiOperation(operationId: "Run", tags: new[] { "name" })]
        [OpenApiParameter(name: "name", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **Name** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            [Table("names", Connection = "AzureWebJobsStorage")] CloudTable namesTable)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            try
            {
                string name = req.Query["name"];

                var filter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "HttpTrigger");
                var query = new TableQuery<FieldEntity>().Where(filter);
                var result = await namesTable.ExecuteQuerySegmentedAsync(query, new TableContinuationToken());

                return new OkObjectResult(result);
            }
            catch (Exception ex)
            {
                return new NotFoundObjectResult(ex);
            }
        }
    }

    internal class FieldEntity: TableEntity
    {
        public string? Field { get; set; }
        public string? Id { get; set; }
    }
}

