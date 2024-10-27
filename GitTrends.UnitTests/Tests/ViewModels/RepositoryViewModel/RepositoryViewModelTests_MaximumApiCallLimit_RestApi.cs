using System.Net;
using GitHubApiStatus;
using GitTrends.Common;
using Refit;
using RichardSzalay.MockHttp;

namespace GitTrends.UnitTests;

[NonParallelizable]
class RepositoryViewModelTests_MaximumApiCallLimit_RestApi : RepositoryViewModelTests_MaximumApiCallLimit
{
	[Test]
	public Task MaximumApiCallLimit_RestApi() => ExecutePullToRefreshCommandTestMaximumApiLimitTest();

	protected override void InitializeServiceCollection()
	{
		var handler = new HttpClientHandler
		{
			AutomaticDecompression = GetDecompressionMethods()
		};

		var gitHubApiV3Client = RestService.For<IGitHubApiV3>(CreateMaximumApiLimitHttpClient(GitHubConstants.GitHubRestApiUrl));

		var gitHubGraphQLCLient = RestService.For<IGitHubGraphQLApi>(new HttpClient(handler)
		{
			BaseAddress = new(GitHubConstants.GitHubGraphQLApi)
		});

		var azureFunctionsClient = RestService.For<IAzureFunctionsApi>(new HttpClient(handler)
		{
			BaseAddress = new(AzureConstants.AzureFunctionsApiUrl)
		});

		ServiceCollection.Initialize(azureFunctionsClient, gitHubApiV3Client, gitHubGraphQLCLient);
	}

	static HttpClient CreateMaximumApiLimitHttpClient(string url)
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