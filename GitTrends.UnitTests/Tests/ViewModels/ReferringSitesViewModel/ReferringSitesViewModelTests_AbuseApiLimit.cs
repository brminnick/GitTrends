using System.Net;
using System.Net.Http.Headers;
using GitTrends.Mobile.Common;
using GitTrends.Shared;
using Refit;
using RichardSzalay.MockHttp;

namespace GitTrends.UnitTests;

class ReferringSitesViewModelTests_AbuseApiLimit : BaseTest
{
	[Test]
	public async Task PullToRefreshTest_AbuseApiLimit()
	{
		//Arrange
		PullToRefreshFailedEventArgs pullToRefreshFailedEventArgs;
		string emptyDataViewTitle_Initial, emptyDataViewTitle_Final;
		string emptyDataViewDescription_Initial, emptyDataViewDescription_Final;
		bool isEmptyDataViewEnabled_Initial, isEmptyDataViewEnabled_DuringRefresh, isEmptyDataViewEnabled_Final;
		IReadOnlyList<MobileReferringSiteModel> mobileReferringSites_Initial, mobileReferringSites_DuringRefresh, mobileReferringSites_Final;

		var mockGitTrendsRepository = new Repository(GitHubConstants.GitTrendsRepoName, "", 0, GitHubConstants.GitTrendsRepoOwner, AuthenticatedGitHubUserAvatarUrl, 0, 0, 0,
			$"https://github.com/{GitHubConstants.GitTrendsRepoOwner}/{GitHubConstants.GitTrendsRepoName}", false, DateTimeOffset.UtcNow, RepositoryPermission.ADMIN, false);

		var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
		var referringSitesViewModel = ServiceCollection.ServiceProvider.GetRequiredService<ReferringSitesViewModel>();
		var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();

		var pullToRefreshFailedTCS = new TaskCompletionSource<PullToRefreshFailedEventArgs>();
		ReferringSitesViewModel.PullToRefreshFailed += HandlePullToRefreshFailed;

		//Act
		await AuthenticateUser(gitHubUserService, gitHubGraphQLApiService, TestCancellationTokenSource.Token).ConfigureAwait(false);

		emptyDataViewTitle_Initial = referringSitesViewModel.EmptyDataViewTitle;
		isEmptyDataViewEnabled_Initial = referringSitesViewModel.IsEmptyDataViewEnabled;
		mobileReferringSites_Initial = referringSitesViewModel.MobileReferringSitesList;
		emptyDataViewDescription_Initial = referringSitesViewModel.EmptyDataViewDescription;

		var refreshCommandTask = referringSitesViewModel.ExecuteRefreshCommand.ExecuteAsync((mockGitTrendsRepository, CancellationToken.None));

		isEmptyDataViewEnabled_DuringRefresh = referringSitesViewModel.IsEmptyDataViewEnabled;
		mobileReferringSites_DuringRefresh = referringSitesViewModel.MobileReferringSitesList;

		await refreshCommandTask.ConfigureAwait(false);

		emptyDataViewTitle_Final = referringSitesViewModel.EmptyDataViewTitle;
		isEmptyDataViewEnabled_Final = referringSitesViewModel.IsEmptyDataViewEnabled;
		mobileReferringSites_Final = referringSitesViewModel.MobileReferringSitesList;
		emptyDataViewDescription_Final = referringSitesViewModel.EmptyDataViewDescription;

		pullToRefreshFailedEventArgs = await pullToRefreshFailedTCS.Task.ConfigureAwait(false);

		//Asset
		Assert.IsFalse(isEmptyDataViewEnabled_Initial);
		Assert.IsFalse(isEmptyDataViewEnabled_DuringRefresh);
		Assert.True(isEmptyDataViewEnabled_Final);

		Assert.IsEmpty(mobileReferringSites_Initial);
		Assert.IsEmpty(mobileReferringSites_DuringRefresh);
		Assert.IsEmpty(mobileReferringSites_Final);

		Assert.AreEqual(EmptyDataViewService.GetReferringSitesTitleText(RefreshState.AbuseLimit), emptyDataViewTitle_Final);
		Assert.AreEqual(EmptyDataViewService.GetReferringSitesTitleText(RefreshState.Uninitialized), emptyDataViewTitle_Initial);

		Assert.AreEqual(EmptyDataViewService.GetReferringSitesDescriptionText(RefreshState.AbuseLimit), emptyDataViewDescription_Final);
		Assert.AreEqual(EmptyDataViewService.GetReferringSitesDescriptionText(RefreshState.Uninitialized), emptyDataViewDescription_Initial);

		Assert.IsInstanceOf<AbuseLimitPullToRefreshEventArgs>(pullToRefreshFailedEventArgs);

		void HandlePullToRefreshFailed(object? sender, PullToRefreshFailedEventArgs e)
		{
			ReferringSitesViewModel.PullToRefreshFailed -= HandlePullToRefreshFailed;
			pullToRefreshFailedTCS.SetResult(e);
		}
	}

	protected override void InitializeServiceCollection(GitHubToken token)
	{
		var gitHubApiV3Client = RestService.For<IGitHubApiV3>(CreateAbuseApiLimitHttpClient(GitHubConstants.GitHubRestApiUrl));
		
		var gitHubGraphQLCLient = RestService.For<IGitHubGraphQLApi>(new HttpClient
		{
			BaseAddress = new(GitHubConstants.GitHubGraphQLApi)
		});

		var azureFunctionsClient = RestService.For<IAzureFunctionsApi>(new HttpClient
		{
			BaseAddress = new(AzureConstants.AzureFunctionsApiUrl)
		});

		ServiceCollection.Initialize(azureFunctionsClient, gitHubApiV3Client, gitHubGraphQLCLient, token);
	}

	protected static HttpClient CreateAbuseApiLimitHttpClient(string url)
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