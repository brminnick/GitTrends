using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using GitTrends.Shared;
using Newtonsoft.Json;
using NUnit.Framework;
using Refit;
using RichardSzalay.MockHttp;

namespace GitTrends.UnitTests
{
    class RepositoryViewModelTests_MaximimApiCallLimit_GraphQLApi : RepositoryViewModelTests_MaximimApiCallLimit
    {
        [Test]
        public Task PullToRefreshCommandTest_MaximumApiLimit_GraphQLApi() => ExecutePullToRefreshCommandTestMaximumApiLimitTest();

        protected override void InitializeServiceCollection()
        {
            var gitHubApiV3Client = RestService.For<IGitHubApiV3>(BaseApiService.CreateHttpClient(GitHubConstants.GitHubRestApiUrl));
            var gitHubGraphQLCLient = RestService.For<IGitHubGraphQLApi>(CreateMaximumApiLimitHttpClient(GitHubConstants.GitHubGraphQLApi));
            var azureFunctionsClient = RestService.For<IAzureFunctionsApi>(BaseApiService.CreateHttpClient(AzureConstants.AzureFunctionsApiUrl));

            ServiceCollection.Initialize(azureFunctionsClient, gitHubApiV3Client, gitHubGraphQLCLient);
        }

        protected static HttpClient CreateMaximumApiLimitHttpClient(string url)
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            var repositoryConnectionResponse = new GraphQLResponse<RepositoryConnectionResponse>(null, new[] { new GraphQLError(string.Empty, Array.Empty<GraphQLLocation>()) });
            var viewerLoginResponse = new GraphQLResponse<GitHubViewerResponse>(new GitHubViewerResponse(new User(null, "Brandon Minnick", string.Empty, default, "brminnick", new Uri(AuthenticatedGitHubUserAvatarUrl))), null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            var errorResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(repositoryConnectionResponse))
            };
            errorResponseMessage.Headers.Add(GitHubApiExceptionService.RateLimitRemainingHeader, "0");
            errorResponseMessage.Headers.Add(GitHubApiExceptionService.RateLimitResetHeader, DateTimeOffset.UtcNow.AddMinutes(50).ToUnixTimeSeconds().ToString());

            var viewerLoginResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(viewerLoginResponse))
            };

            var repositoryConnectionQueryContent = new RepositoryConnectionQueryContent(GitTrendsRepoOwner, string.Empty);
            var viewerLoginQueryContent = new ViewerLoginQueryContent();

            var httpMessageHandler = new MockHttpMessageHandler();
            httpMessageHandler.When(url).WithContent(JsonConvert.SerializeObject(repositoryConnectionQueryContent)).Respond(request => errorResponseMessage);
            httpMessageHandler.When(url).WithContent(JsonConvert.SerializeObject(viewerLoginQueryContent)).Respond(request => viewerLoginResponseMessage);

            var httpClient = httpMessageHandler.ToHttpClient();
            httpClient.BaseAddress = new Uri(url);

            return httpClient;
        }
    }
}
