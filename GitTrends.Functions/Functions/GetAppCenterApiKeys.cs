using System;
using GitTrends.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace GitTrends.Functions
{
    public static class GetAppCenterApiKeys
    {
        readonly static string _iOS = Environment.GetEnvironmentVariable("AppCenterApiKey_iOS") ?? string.Empty;
        readonly static string _android = Environment.GetEnvironmentVariable("AppCenterApiKey_Android") ?? string.Empty;

        [FunctionName(nameof(GetAppCenterApiKeys))]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest request, ILogger log)
        {
            log.LogInformation("Retrieving Client Id");

            if (string.IsNullOrWhiteSpace(_iOS))
                return new NotFoundObjectResult($"{nameof(_iOS)} Not Found");

            if (string.IsNullOrWhiteSpace(_android))
                return new NotFoundObjectResult($"{nameof(_android)} Not Found");

            return new OkObjectResult(new AppCenterApiKeyDTO(_iOS, _android));
        }
    }
}
