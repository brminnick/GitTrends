using System;
using System.Collections.Generic;
using System.Net;
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

        public GetTestToken(GitHubApiV3Service gitHubApiV3Service, GitHubGraphQLApiService gitHubGraphQLApiService) =>
            (_gitHubApiV3Service, _gitHubGraphQLApiService) = (gitHubApiV3Service, gitHubGraphQLApiService);

        [FunctionName(nameof(GetTestToken))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest request, ILogger log)
        {
            foreach (var testToken in _testTokenList)
            {
                var timeout = TimeSpan.FromSeconds(2);
                var cancellationTokenSource = new CancellationTokenSource(timeout);

                var gitHubRestApiResponse = await _gitHubApiV3Service.GetGitHubApiResponse(testToken, cancellationTokenSource.Token).ConfigureAwait(false);
                var gitHubGraphQLApiResponse = await _gitHubGraphQLApiService.ViewerLoginQuery(testToken, cancellationTokenSource.Token).ConfigureAwait(false);

                var restApiRequestsRemaining = GitHubApiService.GetNumberOfApiRequestsRemaining(gitHubRestApiResponse.Headers);
                var graphQLApiRequestsRemaining = GitHubApiService.GetNumberOfApiRequestsRemaining(gitHubGraphQLApiResponse.Headers);

                if (restApiRequestsRemaining > 1000
                    && graphQLApiRequestsRemaining > 1000)
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
