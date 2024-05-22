using GitTrends.Shared;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GitTrends.Functions;

public static class GetStreamingManifests
{
	static readonly string _chartVideoManifestUrl = Environment.GetEnvironmentVariable("ChartVideoManifestUrl") ?? string.Empty;
	static readonly string _enableOrganizationsVideoManifestUrl = Environment.GetEnvironmentVariable("EnableOrganizationsVideoManifestUrl") ?? string.Empty;

	[Function(nameof(GetStreamingManifests))]
	public static async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req, FunctionContext functionContext)
	{
		var logger = functionContext.GetLogger(nameof(GetStreamingManifests));
		logger.LogInformation("Retrieving Chart Video");

		if (string.IsNullOrWhiteSpace(_chartVideoManifestUrl))
		{
			var notFoundResponse = req.CreateResponse(System.Net.HttpStatusCode.NotFound);
			await notFoundResponse.WriteStringAsync("Chart Video Url Not Found").ConfigureAwait(false);

			return notFoundResponse;
		}

		if (string.IsNullOrWhiteSpace(_enableOrganizationsVideoManifestUrl))
		{
			var notFoundResponse = req.CreateResponse(System.Net.HttpStatusCode.NotFound);
			await notFoundResponse.WriteStringAsync("Enable Organizations Video Url Not Found").ConfigureAwait(false);

			return notFoundResponse;
		}

		IReadOnlyDictionary<string, StreamingManifest> videoModels = new Dictionary<string, StreamingManifest>
		{
			{ StreamingConstants.Chart, new StreamingManifest(_chartVideoManifestUrl) },
			{ StreamingConstants.EnableOrganizations, new StreamingManifest(_enableOrganizationsVideoManifestUrl) }
		};

		var okResponse = req.CreateResponse(System.Net.HttpStatusCode.OK);

		var streamingManifestJson = JsonConvert.SerializeObject(videoModels);
		await okResponse.WriteStringAsync(streamingManifestJson).ConfigureAwait(false);

		return okResponse;
	}
}