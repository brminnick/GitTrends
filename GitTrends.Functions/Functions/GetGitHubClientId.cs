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
    public static class GetGitHubClientId
    {
        readonly static string _clientId = Environment.GetEnvironmentVariable("GitTrendsClientId") ?? string.Empty;

        [FunctionName(nameof(GetGitHubClientId))]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req, FunctionExecutionContext executionContext)
        {
            var logger = executionContext.Logger;
            logger.LogInformation("Retrieving Client Id");

            if (string.IsNullOrWhiteSpace(_clientId))
                return new NotFoundObjectResult("Client ID Not Found");

            return new OkObjectResult(new GetGitHubClientIdDTO(_clientId));
        }
    }
}
