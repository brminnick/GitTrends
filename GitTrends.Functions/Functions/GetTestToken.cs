using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using GitHubApiStatus;
using GitTrends.Shared;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace GitTrends.Functions
{
    class GetTestToken
    {
        readonly static IReadOnlyList<string> _testTokenList = new[]
        {
            Environment.GetEnvironmentVariable("UITestToken_brminnick") ?? string.Empty,
            Environment.GetEnvironmentVariable("UITestToken_GitTrendsApp") ?? string.Empty,
            Environment.GetEnvironmentVariable("UITestToken_TheCodeTraveler") ?? string.Empty
        };

        readonly GitHubApiV3Service _gitHubApiV3Service;
        readonly IGitHubApiStatusService _gitHubApiStatusService;
        readonly GitHubGraphQLApiService _gitHubGraphQLApiService;

        public GetTestToken(GitHubApiV3Service gitHubApiV3Service, IGitHubApiStatusService gitHubApiStatusService, GitHubGraphQLApiService gitHubGraphQLApiService)
        {
            _gitHubApiV3Service = gitHubApiV3Service;
            _gitHubApiStatusService = gitHubApiStatusService;
            _gitHubGraphQLApiService = gitHubGraphQLApiService;
        }

        [Function(nameof(GetTestToken))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req, FunctionContext functionContext)
        {
            foreach (var testToken in _testTokenList)
            {
                var timeout = TimeSpan.FromSeconds(2);
                var cancellationTokenSource = new CancellationTokenSource(timeout);

                try
                {
                    _gitHubApiStatusService.SetAuthenticationHeaderValue(new AuthenticationHeaderValue("bearer", testToken));
                    var gitHubApiRateLimits = await _gitHubApiStatusService.GetApiRateLimits(cancellationTokenSource.Token).ConfigureAwait(false);

                    if (gitHubApiRateLimits.RestApi.RemainingRequestCount > 1000
                        && gitHubApiRateLimits.GraphQLApi.RemainingRequestCount > 1000)
                    {
                        var gitHubToken = new GitHubToken(testToken, GitHubConstants.OAuthScope, "Bearer");

                        var okResponse = req.CreateResponse(HttpStatusCode.OK);
                        await okResponse.WriteAsJsonAsync(gitHubToken).ConfigureAwait(false);

                        return okResponse;
                    }
                }
                catch(Exception e)
                {
                    var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                    await errorResponse.WriteStringAsync(e.ToString()).ConfigureAwait(false);

                    return errorResponse;
                }
            };

            var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
            await notFoundResponse.WriteStringAsync("No Valid GitHub Token Found").ConfigureAwait(false);

            return notFoundResponse;
        }
    }
}
