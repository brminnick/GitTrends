using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using GitHubApiStatus;
using GitTrends.Shared;
using Newtonsoft.Json;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace GitTrends.UnitTests
{
	[NonParallelizable]
	class RepositoryViewModelTests_AbuseApiLimit_GraphQLApi : RepositoryViewModelTests_AbuseLimit
	{
		[Test]
		public Task PullToRefreshCommandTest_MaximumApiLimit_GraphQLApi() => ExecutePullToRefreshCommandTestAbuseLimit();

		protected override void InitializeServiceCollection()
		{
			var gitHubApiV3Client = RefitExtensions.For<IGitHubApiV3>(BaseApiService.CreateHttpClient(GitHubConstants.GitHubRestApiUrl));
			var gitHubGraphQLCLient = RefitExtensions.For<IGitHubGraphQLApi>(CreateAbuseApiLimitHttpClient(GitHubConstants.GitHubGraphQLApi));
			var azureFunctionsClient = RefitExtensions.For<IAzureFunctionsApi>(BaseApiService.CreateHttpClient(AzureConstants.AzureFunctionsApiUrl));

			ServiceCollection.Initialize(azureFunctionsClient, gitHubApiV3Client, gitHubGraphQLCLient);
		}

		protected static HttpClient CreateAbuseApiLimitHttpClient(string url)
		{
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
			var gitHubUserResponse = new GraphQLResponse<GitHubUserResponse>(null, new[] { new GraphQLError(string.Empty, Array.Empty<GraphQLLocation>()) });
			var viewerLoginResponse = new GraphQLResponse<GitHubViewerLoginResponse>(new GitHubViewerLoginResponse(new User(null, "Brandon Minnick", string.Empty, default, GitHubConstants.GitTrendsRepoOwner, new Uri(AuthenticatedGitHubUserAvatarUrl))), null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

			var errorResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = new StringContent(JsonConvert.SerializeObject(gitHubUserResponse))
			};
			errorResponseMessage.Headers.Add(GitHubApiStatusService.RateLimitHeader, "5000");
			errorResponseMessage.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromMinutes(1));

			var viewerLoginResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = new StringContent(JsonConvert.SerializeObject(viewerLoginResponse))
			};

			var repositoryConnectionQueryContent = new UserRepositoryConnectionQueryContent(GitHubConstants.GitTrendsRepoOwner, string.Empty);
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