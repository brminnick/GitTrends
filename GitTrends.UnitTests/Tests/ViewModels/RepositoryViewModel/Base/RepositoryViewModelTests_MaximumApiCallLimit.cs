using GitTrends.Common;
using GitTrends.Mobile.Common;

namespace GitTrends.UnitTests;

[NonParallelizable]
abstract class RepositoryViewModelTests_MaximumApiCallLimit : BaseTest
{
	protected async Task ExecutePullToRefreshCommandTestMaximumApiLimitTest()
	{
		//Arrange
		PullToRefreshFailedEventArgs pullToRefreshFailedEventArgs;
		IReadOnlyList<Repository> visibleRepositoryList_Initial, visibleRepositoryList_Final;

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

		emptyDataViewTitle_Initial = repositoryViewModel.EmptyDataViewTitle;
		visibleRepositoryList_Initial = new List<Repository>(repositoryViewModel.VisibleRepositoryList);
		emptyDataViewDescription_Initial = repositoryViewModel.EmptyDataViewDescription;

		await repositoryViewModel.ExecuteRefreshCommand.ExecuteAsync(null).ConfigureAwait(false);
		backgroundFetchService.CancelAllJobs();

		emptyDataViewTitle_Final = repositoryViewModel.EmptyDataViewTitle;
		visibleRepositoryList_Final = new List<Repository>(repositoryViewModel.VisibleRepositoryList);
		emptyDataViewDescription_Final = repositoryViewModel.EmptyDataViewDescription;

		pullToRefreshFailedEventArgs = await pullToRefreshFailedTCS.Task.WaitAsync(TestCancellationTokenSource.Token).ConfigureAwait(false);

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(visibleRepositoryList_Initial, Is.Empty);
			Assert.That(visibleRepositoryList_Final, Is.Empty);

			Assert.That(emptyDataViewTitle_Initial, Is.EqualTo(EmptyDataViewService.GetRepositoryTitleText(RefreshState.Uninitialized, true)));
			Assert.That(emptyDataViewTitle_Final, Is.EqualTo(EmptyDataViewService.GetRepositoryTitleText(RefreshState.MaximumApiLimit, true)));

			Assert.That(emptyDataViewDescription_Initial, Is.EqualTo(EmptyDataViewService.GetRepositoryDescriptionText(RefreshState.Uninitialized, true)));
			Assert.That(emptyDataViewDescription_Final, Is.EqualTo(EmptyDataViewService.GetRepositoryDescriptionText(RefreshState.MaximumApiLimit, true)));

			Assert.That(pullToRefreshFailedEventArgs, Is.InstanceOf<MaximumApiRequestsReachedEventArgs>());
		});

		void HandlePullToRefreshFailed(object? sender, PullToRefreshFailedEventArgs e)
		{
			RepositoryViewModel.PullToRefreshFailed -= HandlePullToRefreshFailed;
			backgroundFetchService.CancelAllJobs();
			pullToRefreshFailedTCS.SetResult(e);
		}
	}
}