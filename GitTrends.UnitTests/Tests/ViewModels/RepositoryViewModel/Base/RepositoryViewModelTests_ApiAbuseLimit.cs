using GitTrends.Common;
using GitTrends.Mobile.Common;

namespace GitTrends.UnitTests;

[NonParallelizable]
abstract class RepositoryViewModelTests_AbuseLimit : BaseTest
{
	protected async Task ExecutePullToRefreshCommandTestAbuseLimit()
	{
		//Arrange
		PullToRefreshFailedEventArgs pullToRefreshFailedEventArgs;
		IReadOnlyList<Repository> visibleRepositoryList_Initial, visibleRepositoryList_Final;

		int gitHubApiAbuseLimitCount_Initial, gitHubApiAbuseLimitCount_Final;

		string emptyDataViewTitle_Initial, emptyDataViewTitle_Final;
		string emptyDataViewDescription_Initial, emptyDataViewDescription_Final;

		var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
		var repositoryViewModel = ServiceCollection.ServiceProvider.GetRequiredService<RepositoryViewModel>();
		var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();
		var backgroundFetchService = (ExtendedBackgroundFetchService)ServiceCollection.ServiceProvider.GetRequiredService<BackgroundFetchService>();

		var pullToRefreshFailedTCS = new TaskCompletionSource<PullToRefreshFailedEventArgs>();

		RepositoryViewModel.PullToRefreshFailed += HandlePullToRefreshFailed;

		//Act
		await AuthenticateUser(gitHubUserService, gitHubGraphQLApiService, TestCancellationTokenSource.Token).ConfigureAwait(false);

		gitHubApiAbuseLimitCount_Initial = gitHubUserService.GitHubApiAbuseLimitCount;

		emptyDataViewTitle_Initial = repositoryViewModel.EmptyDataViewTitle;
		visibleRepositoryList_Initial = new List<Repository>(repositoryViewModel.VisibleRepositoryList);
		emptyDataViewDescription_Initial = repositoryViewModel.EmptyDataViewDescription;

		await repositoryViewModel.ExecuteRefreshCommand.ExecuteAsync(null).ConfigureAwait(false);
		backgroundFetchService.CancelAllJobs();

		emptyDataViewTitle_Final = repositoryViewModel.EmptyDataViewTitle;
		visibleRepositoryList_Final = new List<Repository>(repositoryViewModel.VisibleRepositoryList);
		emptyDataViewDescription_Final = repositoryViewModel.EmptyDataViewDescription;

		pullToRefreshFailedEventArgs = await pullToRefreshFailedTCS.Task.WaitAsync(TestCancellationTokenSource.Token).ConfigureAwait(false);

		gitHubApiAbuseLimitCount_Final = gitHubUserService.GitHubApiAbuseLimitCount;

		Assert.Multiple(() =>
		{
			//Assert
			Assert.That(visibleRepositoryList_Initial, Is.Empty);

			Assert.That(emptyDataViewTitle_Initial, Is.EqualTo(EmptyDataViewService.GetRepositoryTitleText(RefreshState.Uninitialized, true)));
			Assert.That(emptyDataViewTitle_Final, Is.EqualTo(EmptyDataViewService.GetRepositoryTitleText(RefreshState.AbuseLimit, !visibleRepositoryList_Final.Any())));

			Assert.That(emptyDataViewDescription_Initial, Is.EqualTo(EmptyDataViewService.GetRepositoryDescriptionText(RefreshState.Uninitialized, true)));
			Assert.That(emptyDataViewDescription_Final, Is.EqualTo(EmptyDataViewService.GetRepositoryDescriptionText(RefreshState.AbuseLimit, !visibleRepositoryList_Final.Any())));

			Assert.That(pullToRefreshFailedEventArgs, Is.InstanceOf<AbuseLimitPullToRefreshEventArgs>());
			Assert.That(gitHubApiAbuseLimitCount_Initial, Is.EqualTo(0));
			Assert.That(gitHubApiAbuseLimitCount_Final, Is.GreaterThan(0));
		});

		void HandlePullToRefreshFailed(object? sender, PullToRefreshFailedEventArgs e)
		{
			RepositoryViewModel.PullToRefreshFailed -= HandlePullToRefreshFailed;
			pullToRefreshFailedTCS.SetResult(e);
		}
	}
}