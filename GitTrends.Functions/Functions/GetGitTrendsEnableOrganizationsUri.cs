using GitTrends.Common;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;


namespace GitTrends.Functions;

public static class GetGitTrendsEnableOrganizationsUri
{
	static readonly string _enableOrganizationsUrl = Environment.GetEnvironmentVariable("EnableOrganizationsUrl") ?? string.Empty;

	[Function(nameof(GetGitTrendsEnableOrganizationsUri))]
	public static async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req, FunctionContext functionContext)
	{
		var logger = functionContext.GetLogger(nameof(GetGitHubClientId));
		logger.LogInformation("Retrieving Organizations Url");

		if (string.IsNullOrWhiteSpace(_enableOrganizationsUrl))
		{
			var notFoundResponse = req.CreateResponse(System.Net.HttpStatusCode.NotFound);
			await notFoundResponse.WriteStringAsync("Enable Organizations Url Not Found").ConfigureAwait(false);

			return notFoundResponse;
		}

		if (string.IsNullOrWhiteSpace(GetGitHubClientId.ClientId))
		{
			var notFoundResponse = req.CreateResponse(System.Net.HttpStatusCode.NotFound);
			await notFoundResponse.WriteStringAsync("Client Id Not Found").ConfigureAwait(false);

			return notFoundResponse;
		}

		var okResponse = req.CreateResponse(System.Net.HttpStatusCode.OK);
		await okResponse.WriteAsJsonAsync(new GitTrendsEnableOrganizationsUriDTO(new Uri(_enableOrganizationsUrl + GetGitHubClientId.ClientId))).ConfigureAwait(false);

		return okResponse;
	}
}