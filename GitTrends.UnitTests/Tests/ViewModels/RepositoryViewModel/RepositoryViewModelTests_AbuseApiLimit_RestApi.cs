using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using GitHubApiStatus;
using GitTrends.Shared;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace GitTrends.UnitTests;

class RepositoryViewModelTests_AbuseLimit_RestApi : RepositoryViewModelTests_AbuseLimit
{
	[Test]
	public Task PullToRefreshCommandTest_MaximumApiLimit_RestLApi() => ExecutePullToRefreshCommandTestAbuseLimit();

	protected override void InitializeServiceCollection()
	{
		var gitHubApiV3Client = RefitExtensions.For<IGitHubApiV3>(CreateAbuseApiLimitHttpClient(GitHubConstants.GitHubRestApiUrl));
		var gitHubGraphQLCLient = RefitExtensions.For<IGitHubGraphQLApi>(BaseApiService.CreateHttpClient(GitHubConstants.GitHubGraphQLApi));
		var azureFunctionsClient = RefitExtensions.For<IAzureFunctionsApi>(BaseApiService.CreateHttpClient(AzureConstants.AzureFunctionsApiUrl));

		ServiceCollection.Initialize(azureFunctionsClient, gitHubApiV3Client, gitHubGraphQLCLient);
	}

	protected static HttpClient CreateAbuseApiLimitHttpClient(string url)
	{
		var errorResponseMessage = new HttpResponseMessage(HttpStatusCode.Forbidden);
		errorResponseMessage.Headers.Add(GitHubApiStatusService.RateLimitHeader, "5000");
		errorResponseMessage.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromMinutes(1));

		var httpMessageHandler = new MockHttpMessageHandler();
		httpMessageHandler.When($"{url}/*").Respond(request => errorResponseMessage);

		var httpClient = httpMessageHandler.ToHttpClient();
		httpClient.BaseAddress = new Uri(url);

		return httpClient;
	}
}