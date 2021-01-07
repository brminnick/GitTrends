using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using GitHubApiStatus;
using GitTrends.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Pipeline;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;

namespace GitTrends.Functions
{
    class GetTestToken
    {
        readonly static IReadOnlyList<string> _testTokenList = new[]
        {
            Environment.GetEnvironmentVariable("UITestToken_brminnick") ?? string.Empty,
            Environment.GetEnvironmentVariable("UITestToken_GitTrends") ?? string.Empty,
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

        [FunctionName(nameof(GetTestToken))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req, FunctionExecutionContext executionContext)
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

                        return new ContentResult
                        {
                            Content = JsonConvert.SerializeObject(gitHubToken),
                            StatusCode = (int)HttpStatusCode.OK,
                            ContentType = "application/json"
                        };
                    }
                }
                catch(Exception e)
                {
                    return new ObjectResult(e.ToString())
                    {
                        StatusCode = (int)HttpStatusCode.InternalServerError
                    };
                }
            };

            return new NotFoundObjectResult("No Valid GitHub Token Found");
        }
    }
}
