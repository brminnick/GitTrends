using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using GitHubApiStatus;
using GitTrends.Shared;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace GitTrends.UnitTests;

[NonParallelizable]
class RepositoryViewModelTests_MaximumApiCallLimit_RestApi : RepositoryViewModelTests_MaximumApiCallLimit
{
	[Test]
	public Task PullToRefreshCommandTest_MaximumApiLimit_RestLApi() => ExecutePullToRefreshCommandTestMaximumApiLimitTest();

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
		responseMessage.Headers.Add(GitHubApiStatusService.RateLimitHeader, "5000");
		responseMessage.Headers.Add(GitHubApiStatusService.RateLimitRemainingHeader, "0");
		responseMessage.Headers.Add(GitHubApiStatusService.RateLimitResetHeader, DateTimeOffset.UtcNow.AddMinutes(50).ToUnixTimeSeconds().ToString());

		var httpMessageHandler = new MockHttpMessageHandler();
		httpMessageHandler.When($"{url}/*").Respond(request => responseMessage);

		var httpClient = httpMessageHandler.ToHttpClient();
		httpClient.BaseAddress = new Uri(url);

		return httpClient;
	}
}