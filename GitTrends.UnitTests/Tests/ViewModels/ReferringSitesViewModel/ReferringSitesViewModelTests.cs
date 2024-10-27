using GitTrends.Common;
using GitTrends.Mobile.Common;

namespace GitTrends.UnitTests;

[NonParallelizable]
class ReferringSitesViewModelTests : BaseTest
{
	[Test]
	public async Task PullToRefreshTest_UnauthenticatedUser()
	{
		//Arrange
		string emptyDataViewTitle_Initial, emptyDataViewTitle_Final;
		string emptyDataViewDescription_Initial, emptyDataViewDescription_Final;
		bool isEmptyDataViewEnabled_Initial, isEmptyDataViewEnabled_DuringRefresh, isEmptyDataViewEnabled_Final;
		IReadOnlyList<MobileReferringSiteModel> mobileReferringSites_Initial, mobileReferringSites_DuringRefresh, mobileReferringSites_Final;

		bool didPullToRefreshFailedFire = false;
		var pullToRefreshFailedTCS = new TaskCompletionSource<PullToRefreshFailedEventArgs>();

		var mockGitTrendsRepository = new Repository(GitHubConstants.GitTrendsRepoName, "", 0, GitHubConstants.GitTrendsRepoOwner, AuthenticatedGitHubUserAvatarUrl, 0, 0, 0,
			$"https://github.com/{GitHubConstants.GitTrendsRepoOwner}/{GitHubConstants.GitTrendsRepoName}", false, DateTimeOffset.UtcNow, RepositoryPermission.ADMIN, false);

		ReferringSitesViewModel.PullToRefreshFailed += HandlePullToRefreshFailed;

		var referringSitesViewModel = (ExtendedReferringSitesViewModel)ServiceCollection.ServiceProvider.GetRequiredService<ReferringSitesViewModel>();
		referringSitesViewModel.SetRepository(mockGitTrendsRepository);

		//Act
		emptyDataViewTitle_Initial = referringSitesViewModel.EmptyDataViewTitle;
		mobileReferringSites_Initial = referringSitesViewModel.MobileReferringSitesList;
		isEmptyDataViewEnabled_Initial = referringSitesViewModel.IsEmptyDataViewEnabled;
		emptyDataViewDescription_Initial = referringSitesViewModel.EmptyDataViewDescription;

		var refreshCommandTask = referringSitesViewModel.ExecuteRefreshCommand.ExecuteAsync(TestCancellationTokenSource.Token);

		isEmptyDataViewEnabled_DuringRefresh = referringSitesViewModel.IsEmptyDataViewEnabled;
		mobileReferringSites_DuringRefresh = referringSitesViewModel.MobileReferringSitesList;

		await refreshCommandTask.ConfigureAwait(false);
		var pullToRefreshFailedEventArgs = await pullToRefreshFailedTCS.Task.WaitAsync(TestCancellationTokenSource.Token).ConfigureAwait(false);

		emptyDataViewTitle_Final = referringSitesViewModel.EmptyDataViewTitle;
		mobileReferringSites_Final = referringSitesViewModel.MobileReferringSitesList;
		isEmptyDataViewEnabled_Final = referringSitesViewModel.IsEmptyDataViewEnabled;
		emptyDataViewDescription_Final = referringSitesViewModel.EmptyDataViewDescription;

		Assert.Multiple(() =>
		{
			//Assert
			Assert.That(didPullToRefreshFailedFire);
			Assert.That(pullToRefreshFailedEventArgs, Is.InstanceOf<PullToRefreshFailedEventArgs>());

			Assert.That(isEmptyDataViewEnabled_Initial, Is.False);
			Assert.That(isEmptyDataViewEnabled_DuringRefresh, Is.False);
			Assert.That(isEmptyDataViewEnabled_Final, Is.True);

			Assert.That(emptyDataViewTitle_Initial, Is.EqualTo(EmptyDataViewService.GetReferringSitesTitleText(RefreshState.Uninitialized)));
			Assert.That(emptyDataViewTitle_Final, Is.EqualTo(EmptyDataViewService.GetReferringSitesTitleText(RefreshState.Error)));

			Assert.That(emptyDataViewDescription_Initial, Is.EqualTo(EmptyDataViewService.GetReferringSitesDescriptionText(RefreshState.Uninitialized)));
			Assert.That(emptyDataViewDescription_Final, Is.EqualTo(EmptyDataViewService.GetReferringSitesDescriptionText(RefreshState.Error)));

			Assert.That(mobileReferringSites_Initial, Is.Empty);
			Assert.That(mobileReferringSites_DuringRefresh, Is.Empty);
			Assert.That(mobileReferringSites_Final, Is.Empty);
		});

		void HandlePullToRefreshFailed(object? sender, PullToRefreshFailedEventArgs e)
		{
			ReferringSitesViewModel.PullToRefreshFailed -= HandlePullToRefreshFailed;

			didPullToRefreshFailedFire = true;
			pullToRefreshFailedTCS.SetResult(e);
		}
	}

	[Test]
	public async Task PullToRefreshTest_AuthenticatedUser()
	{
		//Arrange
		string emptyDataViewTitle_Initial, emptyDataViewTitle_Final;
		string emptyDataViewDescription_Initial, emptyDataViewDescription_Final;
		DateTimeOffset currentTime_Initial, currentTime_Final;
		bool isEmptyDataViewEnabled_Initial, isEmptyDataViewEnabled_DuringRefresh, isEmptyDataViewEnabled_Final;
		IReadOnlyList<MobileReferringSiteModel> mobileReferringSites_Initial, mobileReferringSites_DuringRefresh, mobileReferringSites_Final;

		var mockGitTrendsRepository = new Repository(GitHubConstants.GitTrendsRepoName, "", 0, GitHubConstants.GitTrendsRepoOwner, AuthenticatedGitHubUserAvatarUrl, 0, 0, 0,
			$"https://github.com/{GitHubConstants.GitTrendsRepoOwner}/{GitHubConstants.GitTrendsRepoName}", false, DateTimeOffset.UtcNow, RepositoryPermission.ADMIN, false);

		var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
		var referringSitesViewModel = (ExtendedReferringSitesViewModel)ServiceCollection.ServiceProvider.GetRequiredService<ReferringSitesViewModel>();
		var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();

		referringSitesViewModel.SetRepository(mockGitTrendsRepository);

		//Act
		await AuthenticateUser(gitHubUserService, gitHubGraphQLApiService, TestCancellationTokenSource.Token).ConfigureAwait(false);

		currentTime_Initial = DateTimeOffset.UtcNow;
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

		currentTime_Final = DateTimeOffset.UtcNow;

		Assert.Multiple(() =>
		{
			//Assert
			Assert.That(isEmptyDataViewEnabled_Initial, Is.False);
			Assert.That(isEmptyDataViewEnabled_DuringRefresh, Is.False);
			Assert.That(isEmptyDataViewEnabled_Final);

			Assert.That(mobileReferringSites_Initial, Is.Empty);
			Assert.That(mobileReferringSites_DuringRefresh, Is.Empty);
			Assert.That(mobileReferringSites_Final, Is.Not.Empty);

			Assert.That(emptyDataViewTitle_Final, Is.EqualTo(EmptyDataViewService.GetReferringSitesTitleText(RefreshState.Succeeded)));
			Assert.That(emptyDataViewTitle_Initial, Is.EqualTo(EmptyDataViewService.GetReferringSitesTitleText(RefreshState.Uninitialized)));

			Assert.That(emptyDataViewDescription_Final, Is.EqualTo(EmptyDataViewService.GetReferringSitesDescriptionText(RefreshState.Succeeded)));
			Assert.That(emptyDataViewDescription_Initial, Is.EqualTo(EmptyDataViewService.GetReferringSitesDescriptionText(RefreshState.Uninitialized)));

			foreach (var referringSite in mobileReferringSites_Final)
			{
				Assert.That(currentTime_Initial, Is.LessThan(referringSite.DownloadedAt));
				Assert.That(currentTime_Final, Is.GreaterThan(referringSite.DownloadedAt));
			}
		});
	}
}