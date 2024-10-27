using System.Net;
using System.Net.Http.Headers;
using GitHubApiStatus;
using GitTrends.Common;
using Refit;
using RichardSzalay.MockHttp;

namespace GitTrends.UnitTests;

class RepositoryViewModelTests_AbuseLimit_RestApi : RepositoryViewModelTests_AbuseLimit
{
	[Test]
	public Task AbuseLimit_RestApi() => ExecutePullToRefreshCommandTestAbuseLimit();

	protected override void InitializeServiceCollection()
	{
		var handler = new HttpClientHandler
		{
			AutomaticDecompression = GetDecompressionMethods()
		};

		var gitHubApiV3Client = RestService.For<IGitHubApiV3>(CreateAbuseApiLimitHttpClient(GitHubConstants.GitHubRestApiUrl));
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