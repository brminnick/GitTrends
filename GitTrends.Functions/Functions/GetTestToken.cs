using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Threading;
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
        readonly GitHubGraphQLApiService _gitHubGraphQLApiService;
        readonly GitHubApiExceptionService _gitHubApiExceptionService;

        public GetTestToken(GitHubApiV3Service gitHubApiV3Service, GitHubGraphQLApiService gitHubGraphQLApiService, GitHubApiExceptionService gitHubApiExceptionService)
        {
            _gitHubApiV3Service = gitHubApiV3Service;
            _gitHubGraphQLApiService = gitHubGraphQLApiService;
            _gitHubApiExceptionService = gitHubApiExceptionService;
        }

        [FunctionName(nameof(GetTestToken))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest request, ILogger log)
        {
            foreach (var testToken in _testTokenList)
            {
                var timeout = TimeSpan.FromSeconds(2);
                var cancellationTokenSource = new CancellationTokenSource(timeout);

                var gitHubApiRateLimits = await _gitHubApiExceptionService.GetApiRateLimits(new AuthenticationHeaderValue("bearer", testToken), cancellationTokenSource.Token).ConfigureAwait(false);

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
            };

            return new NotFoundObjectResult("No Valid GitHub Token Found");
        }
    }
}
