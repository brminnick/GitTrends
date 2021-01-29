using System;
using GitTrends.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Pipeline;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace GitTrends.Functions
{
    public static class GetSyncfusionInformation
    {
        [FunctionName(nameof(GetSyncfusionInformation))]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = nameof(GetSyncfusionInformation) + "/{licenseVersion:long}")] HttpRequestData req, FunctionExecutionContext executionContext, long licenseVersion)
        {
            var logger = executionContext.Logger;
            logger.LogInformation("Received request for Syncfusion Information");

            var licenseKey = Environment.GetEnvironmentVariable($"SyncfusionLicense{licenseVersion}", EnvironmentVariableTarget.Process);

            if (string.IsNullOrWhiteSpace(licenseKey))
                return new BadRequestObjectResult($"Key for {nameof(licenseVersion)}{licenseVersion} not found");

            return new OkObjectResult(new SyncFusionDTO(licenseKey, licenseVersion));
        }
    }
}
