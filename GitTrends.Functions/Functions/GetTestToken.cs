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
        readonly static Lazy<List<string>> _testTokenListHolder = new Lazy<List<string>>(() => new List<string>
        {
            { Environment.GetEnvironmentVariable("UITestToken_brminnick") ?? string.Empty },
            { Environment.GetEnvironmentVariable("UITestToken_GitTrends") ?? string.Empty },
            { Environment.GetEnvironmentVariable("UITestToken_GitTrendsApp") ?? string.Empty },
            { Environment.GetEnvironmentVariable("UITestToken_TheCodeTraveler") ?? string.Empty }
        });

        readonly GitHubApiV3Service _gitHubApiV3Service;

        public GetTestToken(GitHubApiV3Service gitHubApiV3Service) => _gitHubApiV3Service = gitHubApiV3Service;

        List<string> TestTokenList => _testTokenListHolder.Value;

        [FunctionName(nameof(GetTestToken))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest request, ILogger log)
        {
            foreach (var testToken in TestTokenList)
            {
                var timeout = TimeSpan.FromSeconds(1);
                var cancellationTokenSource = new CancellationTokenSource(timeout);

                var gitHubApiResponse = await _gitHubApiV3Service.GetGitHubApiResponse(testToken, cancellationTokenSource.Token).ConfigureAwait(false);
                var apiRequestsRemaining = GitHubApiService.GetNumberOfApiRequestsRemaining(gitHubApiResponse.Headers);

                if (apiRequestsRemaining > 1000)
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
