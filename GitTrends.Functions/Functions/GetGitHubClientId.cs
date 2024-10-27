using GitTrends.Common;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;


namespace GitTrends.Functions;

public static class GetGitHubClientId
{
	public static string ClientId { get; } = Environment.GetEnvironmentVariable("GitTrendsClientId") ?? string.Empty;

	[Function(nameof(GetGitHubClientId))]
	public static async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req, FunctionContext functionContext)
	{
		var logger = functionContext.GetLogger(nameof(GetGitHubClientId));
		logger.LogInformation("Retrieving Client Id");

		if (string.IsNullOrWhiteSpace(ClientId))
		{
			var notFoundResponse = req.CreateResponse(System.Net.HttpStatusCode.NotFound);
			await notFoundResponse.WriteStringAsync("Client ID Not Found").ConfigureAwait(false);

			return notFoundResponse;
		}

		var okResponse = req.CreateResponse(System.Net.HttpStatusCode.OK);
		await okResponse.WriteAsJsonAsync(new GetGitHubClientIdDTO(ClientId)).ConfigureAwait(false);

		return okResponse;
	}
}