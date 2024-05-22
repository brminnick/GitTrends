using GitTrends.Shared;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GitTrends.Functions;

public static class GetSyncfusionInformation
{
	[Function(nameof(GetSyncfusionInformation))]
	public static async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = nameof(GetSyncfusionInformation) + "/{licenseVersion:long}")] HttpRequestData req, FunctionContext functionContext, long licenseVersion)
	{
		var logger = functionContext.GetLogger(nameof(GetSyncfusionInformation));
		logger.LogInformation("Received request for Syncfusion Information");

		var licenseKey = Environment.GetEnvironmentVariable($"SyncfusionLicense{licenseVersion}", EnvironmentVariableTarget.Process);

		if (string.IsNullOrWhiteSpace(licenseKey))
		{
			var badRequestResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
			await badRequestResponse.WriteStringAsync($"Key for {nameof(licenseVersion)}{licenseVersion} not found").ConfigureAwait(false);

			return badRequestResponse;
		}

		var okResponse = req.CreateResponse(System.Net.HttpStatusCode.OK);

		var syncFusionDtoJson = JsonConvert.SerializeObject(new SyncFusionDTO(licenseKey, licenseVersion));

		await okResponse.WriteStringAsync(syncFusionDtoJson).ConfigureAwait(false);

		return okResponse;
	}
}