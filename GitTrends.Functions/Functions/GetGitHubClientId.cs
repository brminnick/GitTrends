using System;
using System.Threading.Tasks;
using GitTrends.Shared;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GitTrends.Functions
{
    public static class GetGitHubClientId
    {
        readonly static string _clientId = Environment.GetEnvironmentVariable("GitTrendsClientId") ?? string.Empty;

        [Function(nameof(GetGitHubClientId))]
        public static async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req, FunctionContext functionContext)
        {
            var logger = functionContext.GetLogger(nameof(GetGitHubClientId));
            logger.LogInformation("Retrieving Client Id");

            if (string.IsNullOrWhiteSpace(_clientId))
            {
                var notFoundResponse = req.CreateResponse(System.Net.HttpStatusCode.NotFound);
                await notFoundResponse.WriteStringAsync("Client ID Not Found").ConfigureAwait(false);

                return notFoundResponse;
            }

            var okResponse = req.CreateResponse(System.Net.HttpStatusCode.OK);

            var getGitHubClientDtoJson = JsonConvert.SerializeObject(new GetGitHubClientIdDTO(_clientId));

            await okResponse.WriteStringAsync(getGitHubClientDtoJson).ConfigureAwait(false);

            return okResponse;
        }
    }
}
