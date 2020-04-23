using System;
using GitTrends.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace GitTrends.Functions
{
    public static class GetChartVideoUrl
    {
        readonly static string _chartVideoUrl = Environment.GetEnvironmentVariable("ChartVideoUrl") ?? string.Empty;

        [FunctionName(nameof(GetChartVideoUrl))]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest httpRequest, ILogger log)
        {
            log.LogInformation("Retrieving Chart Video");

            if (string.IsNullOrWhiteSpace(_chartVideoUrl))
                return new NotFoundObjectResult($"Chart Video Url not found");

            return new OkObjectResult(new GetChartVideoDTO(_chartVideoUrl));
        }
    }
}
