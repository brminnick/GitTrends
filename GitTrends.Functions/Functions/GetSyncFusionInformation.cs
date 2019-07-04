using System;
using GitTrends.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace GitTrends.Functions
{
    public static class GetSyncFusionInformation
    {
        readonly static string _license = Environment.GetEnvironmentVariable("SyncFusionLicense");

        [FunctionName(nameof(GetSyncFusionInformation))]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest request, ILogger log) => new OkObjectResult(new SyncFusionDTO(_license));
    }
}
