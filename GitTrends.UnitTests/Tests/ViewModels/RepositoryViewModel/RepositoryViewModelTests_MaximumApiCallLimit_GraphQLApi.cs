using System.Net;
using System.Text.Json;
using GitHubApiStatus;
using GitTrends.Common;

using Refit;
using RichardSzalay.MockHttp;

namespace GitTrends.UnitTests;

[NonParallelizable]
class RepositoryViewModelTests_MaximumApiCallLimit_GraphQLApi : RepositoryViewModelTests_MaximumApiCallLimit
{
	[Test]
	public Task MaximumApiLimit_GraphQLApi() => ExecutePullToRefreshCommandTestMaximumApiLimitTest();

	protected override void InitializeServiceCollection()
	{
		var handler = new HttpClientHandler
		{
			AutomaticDecompression = GetDecompressionMethods()
		};

		var gitHubApiV3Client = RestService.For<IGitHubApiV3>(new HttpClient(handler)
		{
			BaseAddress = new(GitHubConstants.GitHubRestApiUrl)
		});

		var gitHubGraphQLCLient = RestService.For<IGitHubGraphQLApi>(CreateMaximumApiLimitHttpClient(GitHubConstants.GitHubGraphQLApi));

		var azureFunctionsClient = RestService.For<IAzureFunctionsApi>(new HttpClient(handler)
		{
			BaseAddress = new(AzureConstants.AzureFunctionsApiUrl)
		});

		ServiceCollection.Initialize(azureFunctionsClient, gitHubApiV3Client, gitHubGraphQLCLient);
	}

	protected static HttpClient CreateMaximumApiLimitHttpClient(string url)
	{
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
		var gitHubUserResponse = new GraphQLResponse<GitHubUserResponse>(null, [new GraphQLError(string.Empty, [])]);
		var viewerLoginResponse = new GraphQLResponse<GitHubViewerLoginResponse>(new GitHubViewerLoginResponse(new User(null, "Brandon Minnick", string.Empty, default, GitHubConstants.GitTrendsRepoOwner, new Uri(AuthenticatedGitHubUserAvatarUrl))), null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

		var errorResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
		{
			Content = new StringContent(JsonSerializer.Serialize(gitHubUserResponse))
		};
		errorResponseMessage.Headers.Add(GitHubApiStatusService.RateLimitHeader, "5000");
		errorResponseMessage.Headers.Add(GitHubApiStatusService.RateLimitRemainingHeader, "0");
		errorResponseMessage.Headers.Add(GitHubApiStatusService.RateLimitResetHeader, DateTimeOffset.UtcNow.AddMinutes(50).ToUnixTimeSeconds().ToString());

		var viewerLoginResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
		{
			Content = new StringContent(JsonSerializer.Serialize(viewerLoginResponse))
		};

		var repositoryConnectionQueryContent = new UserRepositoryConnectionQueryContent(GitHubConstants.GitTrendsRepoOwner, string.Empty);
		var viewerLoginQueryContent = new ViewerLoginQueryContent();

		var httpMessageHandler = new MockHttpMessageHandler();
		httpMessageHandler.When(url).WithContent(JsonSerializer.Serialize(repositoryConnectionQueryContent)).Respond(request => errorResponseMessage);
		httpMessageHandler.When(url).WithContent(JsonSerializer.Serialize(viewerLoginQueryContent)).Respond(request => viewerLoginResponseMessage);

		var httpClient = httpMessageHandler.ToHttpClient();
		httpClient.BaseAddress = new Uri(url);

		return httpClient;
	}
}