namespace GitTrends.UnitTests.BackgroundJobs;

class NotifyTrendingRepositoriesJobTests : BaseJobTest
{
	[Test]
	public async Task NotifyTrendingRepositoriesTest_NotLoggedIn()
	{
		//Arrange
		var scheduleNotifyTrendingRepositoriesCompletedTCS = new TaskCompletionSource<bool>();
		NotifyTrendingRepositoriesJob.JobCompleted += HandleScheduleNotifyTrendingRepositoriesCompleted;

		var backgroundFetchService = ServiceCollection.ServiceProvider.GetRequiredService<BackgroundFetchService>();

		//Act
		backgroundFetchService.TryScheduleNotifyTrendingRepositories();

		var result = await scheduleNotifyTrendingRepositoriesCompletedTCS.Task.WaitAsync(TestCancellationTokenSource.Token).ConfigureAwait(false);

		//Assert
		Assert.That(result, Is.False);

		void HandleScheduleNotifyTrendingRepositoriesCompleted(object? sender, bool e)
		{
			NotifyTrendingRepositoriesJob.JobCompleted -= HandleScheduleNotifyTrendingRepositoriesCompleted;
			scheduleNotifyTrendingRepositoriesCompletedTCS.SetResult(e);
		}
	}


	[Test]
	public async Task NotifyTrendingRepositoriesTest_DemoUser()
	{
		//Arrange
		var scheduleNotifyTrendingRepositoriesCompletedTCS = new TaskCompletionSource<bool>();
		NotifyTrendingRepositoriesJob.JobCompleted += HandleScheduleNotifyTrendingRepositoriesCompleted;

		var gitHubAuthenticationService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubAuthenticationService>();
		await gitHubAuthenticationService.ActivateDemoUser(TestCancellationTokenSource.Token).ConfigureAwait(false);

		var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
		var backgroundFetchService = ServiceCollection.ServiceProvider.GetRequiredService<BackgroundFetchService>();

		//Act
		backgroundFetchService.TryScheduleNotifyTrendingRepositories();

		var result = await scheduleNotifyTrendingRepositoriesCompletedTCS.Task.WaitAsync(TestCancellationTokenSource.Token).ConfigureAwait(false);

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(gitHubUserService.IsDemoUser);
			Assert.That(gitHubUserService.IsAuthenticated, Is.False);
			Assert.That(result, Is.False);
		});

		void HandleScheduleNotifyTrendingRepositoriesCompleted(object? sender, bool e)
		{
			NotifyTrendingRepositoriesJob.JobCompleted -= HandleScheduleNotifyTrendingRepositoriesCompleted;
			scheduleNotifyTrendingRepositoriesCompletedTCS.SetResult(e);
		}
	}

	[Test]
	public async Task NotifyTrendingRepositoriesTest_AuthenticatedUser()
	{
		//Arrange
		bool wasScheduledSuccessfully_First, wasScheduledSuccessfully_Second;
		var scheduleNotifyTrendingRepositoriesCompletedTCS = new TaskCompletionSource<bool>();
		NotifyTrendingRepositoriesJob.JobCompleted += HandleScheduleNotifyTrendingRepositoriesCompleted;

		var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
		var backgroundFetchService = ServiceCollection.ServiceProvider.GetRequiredService<BackgroundFetchService>();
		var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();

		await AuthenticateUser(gitHubUserService, gitHubGraphQLApiService, TestCancellationTokenSource.Token).ConfigureAwait(false);

		//Act
		wasScheduledSuccessfully_First = backgroundFetchService.TryScheduleNotifyTrendingRepositories();
		wasScheduledSuccessfully_Second = backgroundFetchService.TryScheduleNotifyTrendingRepositories();

		var result = await scheduleNotifyTrendingRepositoriesCompletedTCS.Task.WaitAsync(TestCancellationTokenSource.Token).ConfigureAwait(false);

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(wasScheduledSuccessfully_First);
			Assert.That(wasScheduledSuccessfully_Second, Is.False);

			Assert.That(gitHubUserService.IsDemoUser, Is.False);
			Assert.That(gitHubUserService.IsAuthenticated);
			Assert.That(result);
		});

		void HandleScheduleNotifyTrendingRepositoriesCompleted(object? sender, bool e)
		{
			NotifyTrendingRepositoriesJob.JobCompleted -= HandleScheduleNotifyTrendingRepositoriesCompleted;
			scheduleNotifyTrendingRepositoriesCompletedTCS.SetResult(e);
		}
	}
}