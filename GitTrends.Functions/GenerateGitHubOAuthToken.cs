using System;
using System.Threading.Tasks;
using GitTrends.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Octokit;

namespace GitTrends.Functions
{
    public static class GenerateGitHubOAuthToken
    {
        readonly static string _clientSecret = Environment.GetEnvironmentVariable("GitTrendsClientSecret");
        readonly static string _clientId = Environment.GetEnvironmentVariable("GitTrendsClientId");

        [FunctionName(nameof(GenerateGitHubOAuthToken))]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get")] [FromBody] GitHubOAuthModel gitHubOAuthModel, ILogger log)
        {
            log.LogInformation("Received request for OAuth Token");

            var request = new OauthTokenRequest(_clientId, _clientSecret, gitHubOAuthModel.LoginCode);

            var token = await gitHubOAuthModel.GitHubClient.Oauth.CreateAccessToken(request).ConfigureAwait(false);

            return new OkObjectResult(token);
        }
    }
}
