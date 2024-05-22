using System.Net;
using GitTrends.Shared;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GitTrends.Functions;

class GenerateGitHubOAuthToken(GitHubAuthService gitHubAuthService)
{
	static readonly string _clientSecret = Environment.GetEnvironmentVariable("GitTrendsClientSecret") ?? string.Empty;
	static readonly string _clientId = Environment.GetEnvironmentVariable("GitTrendsClientId") ?? string.Empty;

	static readonly JsonSerializer _serializer = new();

	[Function(nameof(GenerateGitHubOAuthToken))]
	public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req, FunctionContext functionContext)
	{
		var logger = functionContext.GetLogger<GenerateGitHubOAuthToken>();
		logger.LogInformation("Received request for OAuth Token");

		using var streamReader = new StreamReader(req.Body);
#pragma warning disable CA2007
		await using var jsonTextReader = new JsonTextReader(streamReader);
#pragma warning restore CA2007
		var generateTokenDTO = _serializer.Deserialize<GenerateTokenDTO>(jsonTextReader);

		if (generateTokenDTO is null)
		{
			var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
			await badRequestResponse.WriteStringAsync($"Invalid {nameof(GenerateTokenDTO)}").ConfigureAwait(false);

			return badRequestResponse;
		}

		var token = await gitHubAuthService.GetGitHubToken(_clientId, _clientSecret, generateTokenDTO.LoginCode, generateTokenDTO.State, CancellationToken.None).ConfigureAwait(false);

		logger.LogInformation("Token Retrieved");

		var okResponse = req.CreateResponse(HttpStatusCode.OK);

		var tokenJson = JsonConvert.SerializeObject(token);

		await okResponse.WriteStringAsync(tokenJson).ConfigureAwait(false);

		return okResponse;

	}
}