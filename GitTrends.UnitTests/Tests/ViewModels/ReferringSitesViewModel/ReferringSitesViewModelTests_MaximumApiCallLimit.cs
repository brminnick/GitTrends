using System.Net;
using GitHubApiStatus;
using GitTrends.Common;
using GitTrends.Mobile.Common;
using Refit;
using RichardSzalay.MockHttp;

namespace GitTrends.UnitTests;

[NonParallelizable]
class ReferringSitesViewModelTests_MaximumApiCallLimit : BaseTest
{
	[Test]
	public async Task PullToRefreshTest_MaximumApiCallLimit()
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
		var referringSitesViewModel = (ExtendedReferringSitesViewModel)ServiceCollection.ServiceProvider.GetRequiredService<ReferringSitesViewModel>();
		var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();

		var pullToRefreshFailedTCS = new TaskCompletionSource<PullToRefreshFailedEventArgs>();
		ReferringSitesViewModel.PullToRefreshFailed += HandlePullToRefreshFailed;

		referringSitesViewModel.SetRepository(mockGitTrendsRepository);

		//Act
		await AuthenticateUser(gitHubUserService, gitHubGraphQLApiService, TestCancellationTokenSource.Token).ConfigureAwait(false);

		emptyDataViewTitle_Initial = referringSitesViewModel.EmptyDataViewTitle;
		isEmptyDataViewEnabled_Initial = referringSitesViewModel.IsEmptyDataViewEnabled;
		mobileReferringSites_Initial = referringSitesViewModel.MobileReferringSitesList;
		emptyDataViewDescription_Initial = referringSitesViewModel.EmptyDataViewDescription;

		var refreshCommandTask = referringSitesViewModel.ExecuteRefreshCommand.ExecuteAsync(TestCancellationTokenSource.Token);

		isEmptyDataViewEnabled_DuringRefresh = referringSitesViewModel.IsEmptyDataViewEnabled;
		mobileReferringSites_DuringRefresh = referringSitesViewModel.MobileReferringSitesList;

		await refreshCommandTask.ConfigureAwait(false);

		emptyDataViewTitle_Final = referringSitesViewModel.EmptyDataViewTitle;
		isEmptyDataViewEnabled_Final = referringSitesViewModel.IsEmptyDataViewEnabled;
		mobileReferringSites_Final = referringSitesViewModel.MobileReferringSitesList;
		emptyDataViewDescription_Final = referringSitesViewModel.EmptyDataViewDescription;

		pullToRefreshFailedEventArgs = await pullToRefreshFailedTCS.Task.WaitAsync(TestCancellationTokenSource.Token).ConfigureAwait(false);

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(isEmptyDataViewEnabled_Initial, Is.False);
			Assert.That(isEmptyDataViewEnabled_DuringRefresh, Is.False);
			Assert.That(isEmptyDataViewEnabled_Final);

			Assert.That(mobileReferringSites_Initial, Is.Empty);
			Assert.That(mobileReferringSites_DuringRefresh, Is.Empty);
			Assert.That(mobileReferringSites_Final, Is.Empty);

			Assert.That(emptyDataViewTitle_Initial, Is.EqualTo(EmptyDataViewService.GetReferringSitesTitleText(RefreshState.Uninitialized)));
			Assert.That(emptyDataViewTitle_Final, Is.EqualTo(EmptyDataViewService.GetReferringSitesTitleText(RefreshState.MaximumApiLimit)));

			Assert.That(emptyDataViewDescription_Final, Is.EqualTo(EmptyDataViewService.GetReferringSitesDescriptionText(RefreshState.MaximumApiLimit)));
			Assert.That(emptyDataViewDescription_Initial, Is.EqualTo(EmptyDataViewService.GetReferringSitesDescriptionText(RefreshState.Uninitialized)));

			Assert.That(pullToRefreshFailedEventArgs, Is.InstanceOf<MaximumApiRequestsReachedEventArgs>());
		});

		void HandlePullToRefreshFailed(object? sender, PullToRefreshFailedEventArgs e)
		{
			ReferringSitesViewModel.PullToRefreshFailed -= HandlePullToRefreshFailed;
			pullToRefreshFailedTCS.SetResult(e);
		}
	}

	protected override void InitializeServiceCollection()
	{
		var gitHubApiV3Client = RestService.For<IGitHubApiV3>(CreateMaximumApiLimitHttpClient(GitHubConstants.GitHubRestApiUrl));

		var handler = new HttpClientHandler
		{
			AutomaticDecompression = GetDecompressionMethods()
		};

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
		responseMessage.Headers.Add(GitHubApiStatusService.RateLimitRemainingHeader, "0");
		responseMessage.Headers.Add(GitHubApiStatusService.RateLimitResetHeader, DateTimeOffset.UtcNow.AddMinutes(50).ToUnixTimeSeconds().ToString());

		var httpMessageHandler = new MockHttpMessageHandler();
		httpMessageHandler.When($"{url}/*").Respond(request => responseMessage);

		var httpClient = httpMessageHandler.ToHttpClient();
		httpClient.BaseAddress = new Uri(url);

		return httpClient;
	}
}