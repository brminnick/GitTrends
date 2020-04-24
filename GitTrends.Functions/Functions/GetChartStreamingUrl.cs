using System;
using GitTrends.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace GitTrends.Functions
{
    public static class GetChartStreamingUrl
    {
        readonly static string _chartVideoManifestUrl = Environment.GetEnvironmentVariable("ChartVideoManifestUrl") ?? string.Empty;

        [FunctionName(nameof(GetChartStreamingUrl))]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest httpRequest, ILogger log)
        {
            log.LogInformation("Retrieving Chart Video");

            if (string.IsNullOrWhiteSpace(_chartVideoManifestUrl))
                return new NotFoundObjectResult($"Chart Video Url not found");

            return new OkObjectResult(new StreamingManifest(_chartVideoManifestUrl));
        }
    }
}
