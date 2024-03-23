using System;
using System.Threading.Tasks;
using GitTrends.Shared;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json;

namespace GitTrends.Functions
{
	public static class GetNotificationHubInformation
	{
		public static string NotificationHubName { get; } = Environment.GetEnvironmentVariable("NotificationHubName") ?? string.Empty;
		public static string NotificationHubName_Debug { get; } = Environment.GetEnvironmentVariable("NotificationHubName_Debug") ?? string.Empty;

		static readonly string _notificationHubConnectionString_Debug = Environment.GetEnvironmentVariable("NotificationHubListenConnectionString_Debug") ?? string.Empty;
		static readonly string _notificationHubConnectionString = Environment.GetEnvironmentVariable("NotificationHubListenConnectionString") ?? string.Empty;

		[Function(nameof(GetNotificationHubInformation))]
		public static async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req, FunctionContext functionContext)
		{
			var response = req.CreateResponse(System.Net.HttpStatusCode.OK);

			var notificationHubInformationJson = JsonConvert.SerializeObject(new NotificationHubInformation(NotificationHubName, _notificationHubConnectionString, NotificationHubName_Debug, _notificationHubConnectionString_Debug));

			await response.WriteStringAsync(notificationHubInformationJson).ConfigureAwait(false);

			return response;
		}
	}
}