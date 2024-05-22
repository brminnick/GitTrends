using System.Net;
using GitTrends.Shared;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GitTrends.Functions;

public static class GetAppCenterApiKeys
{
	static readonly string _iOS = Environment.GetEnvironmentVariable("AppCenterApiKey_iOS") ?? string.Empty;
	static readonly string _android = Environment.GetEnvironmentVariable("AppCenterApiKey_Android") ?? string.Empty;

	[Function(nameof(GetAppCenterApiKeys))]
	public static async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req, FunctionContext context)
	{
		var log = context.GetLogger(nameof(GetAppCenterApiKeys));

		log.LogInformation("Retrieving Client Id");

		if (string.IsNullOrWhiteSpace(_iOS))
		{
			var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
			await notFoundResponse.WriteStringAsync($"{nameof(_iOS)} Not Found").ConfigureAwait(false);

			return notFoundResponse;
		}
		else if (string.IsNullOrWhiteSpace(_android))
		{
			var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
			await notFoundResponse.WriteStringAsync($"{nameof(_android)} Not Found").ConfigureAwait(false);

			return notFoundResponse;
		}
		else
		{
			var response = req.CreateResponse(HttpStatusCode.OK);

			var appCenterApiKeyDtoJson = JsonConvert.SerializeObject(new AppCenterApiKeyDTO(_iOS, _android));

			await response.WriteStringAsync(appCenterApiKeyDtoJson).ConfigureAwait(false);

			return response;
		}
	}
}