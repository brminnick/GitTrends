using System;
using GitTrends.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace GitTrends.Functions
{
    public static class GetNotificationHubInformation
    {
        public static string NotificationHubName { get; } = Environment.GetEnvironmentVariable("NotificationHubName") ?? string.Empty;
        public static string NotificationHubName_Debug { get; } = Environment.GetEnvironmentVariable("NotificationHubName_Debug") ?? string.Empty;

        readonly static string _notificationHubConnectionString_Debug = Environment.GetEnvironmentVariable("NotificationHubListenConnectionString_Debug") ?? string.Empty;
        readonly static string _notificationHubConnectionString = Environment.GetEnvironmentVariable("NotificationHubListenConnectionString") ?? string.Empty;

        [FunctionName(nameof(GetNotificationHubInformation))]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req, FunctionContext functionContext) =>
           new OkObjectResult(new NotificationHubInformation(NotificationHubName, _notificationHubConnectionString, NotificationHubName_Debug, _notificationHubConnectionString_Debug));
    }
}
