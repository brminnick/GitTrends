using System;
using GitTrends.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace GitTrends.Functions
{
    public static class GetGitHubClientId
    {
        readonly static string _clientId = Environment.GetEnvironmentVariable("GitTrendsClientId");

        [FunctionName(nameof(GetGitHubClientId))]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest request, ILogger log) => new OkObjectResult(new GetGitHubClientIdDTO(_clientId));
    }
}
