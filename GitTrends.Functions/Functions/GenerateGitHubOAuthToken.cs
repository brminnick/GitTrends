using System;
using System.IO;
using System.Threading.Tasks;
using GitTrends.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GitTrends.Functions
{
    public static class GenerateGitHubOAuthToken
    {
        readonly static string _clientSecret = Environment.GetEnvironmentVariable("GitTrendsClientSecret");
        readonly static string _clientId = Environment.GetEnvironmentVariable("GitTrendsClientId");

        [FunctionName(nameof(GenerateGitHubOAuthToken))]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest httpRequest, ILogger log)
        {
            log.LogInformation("Received request for OAuth Token");

            using (var reader = new StreamReader(httpRequest.Body))
            {
                var body = await reader.ReadToEndAsync().ConfigureAwait(false);
                var generateTokenDTO = JsonConvert.DeserializeObject<GenerateTokenDTO>(body);

                var token = await GitHubAuthService.GetGitHubToken(_clientId, _clientSecret, generateTokenDTO.LoginCode, generateTokenDTO.State).ConfigureAwait(false);

                log.LogInformation("Token Retrived");

                return new OkObjectResult(token);
            }
        }
    }
}
