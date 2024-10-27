using System.Net;
using System.Text.Json;
using GitTrends.Common;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;


namespace GitTrends.Functions;

class GenerateGitHubOAuthToken(GitHubAuthService gitHubAuthService)
{
	static readonly string _clientSecret = Environment.GetEnvironmentVariable("GitTrendsClientSecret") ?? string.Empty;
	static readonly string _clientId = Environment.GetEnvironmentVariable("GitTrendsClientId") ?? string.Empty;

	[Function(nameof(GenerateGitHubOAuthToken))]
	public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req, FunctionContext functionContext)
	{
		var logger = functionContext.GetLogger<GenerateGitHubOAuthToken>();
		logger.LogInformation("Received request for OAuth Token");

		var generateTokenDTO = await JsonSerializer.DeserializeAsync<GenerateTokenDTO>(req.Body).ConfigureAwait(false);

		if (generateTokenDTO is null)
		{
			var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
			await badRequestResponse.WriteStringAsync($"Invalid {nameof(GenerateTokenDTO)}").ConfigureAwait(false);

			return badRequestResponse;
		}

		var gitHubToken = await gitHubAuthService.GetGitHubToken(_clientId, _clientSecret, generateTokenDTO.LoginCode, generateTokenDTO.State, CancellationToken.None).ConfigureAwait(false);

		logger.LogInformation("Token Retrieved");

		var okResponse = req.CreateResponse(HttpStatusCode.OK);
		await okResponse.WriteAsJsonAsync(gitHubToken).ConfigureAwait(false);

		return okResponse;

	}
}