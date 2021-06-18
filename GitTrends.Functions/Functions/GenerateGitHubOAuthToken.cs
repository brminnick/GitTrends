using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using GitTrends.Shared;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GitTrends.Functions
{
    class GenerateGitHubOAuthToken
    {
        readonly static string _clientSecret = Environment.GetEnvironmentVariable("GitTrendsClientSecret") ?? string.Empty;
        readonly static string _clientId = Environment.GetEnvironmentVariable("GitTrendsClientId") ?? string.Empty;

        readonly static JsonSerializer _serializer = new();

        readonly GitHubAuthService _gitHubAuthService;

        public GenerateGitHubOAuthToken(GitHubAuthService gitHubAuthService) => _gitHubAuthService = gitHubAuthService;

        [Function(nameof(GenerateGitHubOAuthToken))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req, FunctionContext functionContext)
        {
            var logger = functionContext.GetLogger<GenerateGitHubOAuthToken>();
            logger.LogInformation("Received request for OAuth Token");

            using var streamReader = new StreamReader(req.Body);
            using var jsonTextReader = new JsonTextReader(streamReader);
            var generateTokenDTO = _serializer.Deserialize<GenerateTokenDTO>(jsonTextReader);

            if (generateTokenDTO is null)
            {
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteStringAsync($"Invalid {nameof(GenerateTokenDTO)}").ConfigureAwait(false);

                return badRequestResponse;
            }

            var token = await _gitHubAuthService.GetGitHubToken(_clientId, _clientSecret, generateTokenDTO.LoginCode, generateTokenDTO.State).ConfigureAwait(false);

            logger.LogInformation("Token Retrieved");

            var okResponse = req.CreateResponse(HttpStatusCode.OK);

            var tokenJson = JsonConvert.SerializeObject(token);

            await okResponse.WriteStringAsync(tokenJson).ConfigureAwait(false);

            return okResponse;
        }
    }
}
