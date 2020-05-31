using System;
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
        readonly static string _uiTestToken_brminnick = Environment.GetEnvironmentVariable("UITestToken_brminnick") ?? string.Empty;
        readonly static string _uiTestToken_GitTrends = Environment.GetEnvironmentVariable("UITestToken_GitTrends") ?? string.Empty;

        readonly GitHubApiV3Service _gitHubApiV3Service;

        public GetTestToken(GitHubApiV3Service gitHubApiV3Service) => _gitHubApiV3Service = gitHubApiV3Service;

        [FunctionName(nameof(GetTestToken))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest request, ILogger log)
        {
            GitHubToken gitHubToken;

            var timeout = TimeSpan.FromSeconds(1);
            var cancellationTokenSource = new CancellationTokenSource(timeout);

            var gitHubApiResponse_brminnick = await _gitHubApiV3Service.GetGitHubApiResponse(_uiTestToken_brminnick, cancellationTokenSource.Token).ConfigureAwait(false);
            var brminnickApiRequestsRemaining = GitHubApiService.GetNumberOfApiRequestsRemaining(gitHubApiResponse_brminnick.Headers);

            if (brminnickApiRequestsRemaining > 100)
            {
                gitHubToken = new GitHubToken(_uiTestToken_brminnick, string.Empty, "Bearer");
            }
            else
            {
                gitHubToken = new GitHubToken(_uiTestToken_GitTrends, string.Empty, "Bearer");
            }

            return new ContentResult
            {
                Content = JsonConvert.SerializeObject(gitHubToken),
                StatusCode = (int)HttpStatusCode.OK,
                ContentType = "application/json"
            };
        }
    }
}
