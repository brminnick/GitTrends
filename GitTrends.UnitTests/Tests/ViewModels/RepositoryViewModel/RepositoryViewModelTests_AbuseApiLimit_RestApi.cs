using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using GitTrends.Shared;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace GitTrends.UnitTests
{
    class RepositoryViewModelTests_AbuseLimit_RestApi : RepositoryViewModelTests_AbuseLimit
    {
        [Test]
        public Task PullToRefreshCommandTest_MaximumApiLimit_RestLApi() =>
            ExecutePullToRefreshCommandTestMaximumApiLimitTest(new TaskCompletionSource<Mobile.Common.PullToRefreshFailedEventArgs>());

        protected override void InitializeServiceCollection()
        {
            var gitHubApiV3Client = RefitExtensions.For<IGitHubApiV3>(CreateMaximumApiLimitHttpClient(GitHubConstants.GitHubRestApiUrl));
            var gitHubGraphQLCLient = RefitExtensions.For<IGitHubGraphQLApi>(BaseApiService.CreateHttpClient(GitHubConstants.GitHubGraphQLApi));
            var azureFunctionsClient = RefitExtensions.For<IAzureFunctionsApi>(BaseApiService.CreateHttpClient(AzureConstants.AzureFunctionsApiUrl));

            ServiceCollection.Initialize(azureFunctionsClient, gitHubApiV3Client, gitHubGraphQLCLient);
        }

        protected static HttpClient CreateMaximumApiLimitHttpClient(string url)
        {
            var responseMessage = new HttpResponseMessage(HttpStatusCode.Forbidden);
            responseMessage.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromMinutes(1));

            var httpMessageHandler = new MockHttpMessageHandler();
            httpMessageHandler.When($"{url}/*").Respond(request => responseMessage);

            var httpClient = httpMessageHandler.ToHttpClient();
            httpClient.BaseAddress = new Uri(url);

            return httpClient;
        }
    }
}
