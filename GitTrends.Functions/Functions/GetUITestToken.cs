using System;
using System.Net;
using GitTrends.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GitTrends.Functions
{
    public class GetUITestToken
    {
        readonly static string _token = Environment.GetEnvironmentVariable("UITestToken") ?? string.Empty;

        [FunctionName(nameof(GetUITestToken))]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest request, ILogger log)
        {
            var token = new GitHubToken(_token, string.Empty, "Bearer");

            return new ContentResult
            {
                Content = JsonConvert.SerializeObject(token),
                StatusCode = (int)HttpStatusCode.OK,
                ContentType = "application/json"
            };
        }
    }
}
