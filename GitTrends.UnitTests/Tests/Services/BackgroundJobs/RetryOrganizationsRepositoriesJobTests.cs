using GitTrends.Common;
namespace GitTrends.UnitTests.BackgroundJobs;

class RetryOrganizationsRepositoriesJobTests : BaseJobTest
{
	[Test]
	public void VerifyIdentifiers()
	{
		// Assert
		const string organizationName = "GitTrends";
		var retryOrganizationsRepositoriesJob = ServiceCollection.ServiceProvider.GetRequiredService<RetryOrganizationsRepositoriesJob>();

		// Assert
		Assert.That(retryOrganizationsRepositoriesJob.GetJobIdentifier(organizationName), Is.EqualTo($"{retryOrganizationsRepositoriesJob.Identifier}.{organizationName}"));

	}

	[Test]
	public async Task ScheduleRetryOrganizationsRepositoriesTest_AuthenticatedUser()
	{
		//Arrange
		bool wasScheduledSuccessfully_First, wasScheduledSuccessfully_Second;
		string organizationName_Initial = nameof(GitTrends), organizationName_Final;
		Repository repository_Final, repository_Database;

		var scheduleRetryOrganizationsRepositoriesCompletedTCS = new TaskCompletionSource<string>();
		var scheduleRetryRepositoriesViewsClonesCompletedTCS = new TaskCompletionSource<Repository>();
		RetryOrganizationsRepositoriesJob.JobCompleted += HandleScheduleRetryOrganizationsRepositoriesCompleted;
		RetryRepositoriesViewsClonesStarsJob.JobCompleted += HandleScheduleRetryRepositoriesViewsClonesStarsCompleted;

		var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
		var repositoryDatabase = ServiceCollection.ServiceProvider.GetRequiredService<RepositoryDatabase>();
		var backgroundFetchService = ServiceCollection.ServiceProvider.GetRequiredService<BackgroundFetchService>();
		var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();

		await AuthenticateUser(gitHubUserService, gitHubGraphQLApiService, TestCancellationTokenSource.Token).ConfigureAwait(false);

		//Act
		wasScheduledSuccessfully_First = backgroundFetchService.TryScheduleRetryOrganizationsRepositories(organizationName_Initial);
		wasScheduledSuccessfully_Second = backgroundFetchService.TryScheduleRetryOrganizationsRepositories(organizationName_Initial);

		organizationName_Final = await scheduleRetryOrganizationsRepositoriesCompletedTCS.Task.WaitAsync(TestCancellationTokenSource.Token).ConfigureAwait(false);

		repository_Final = await scheduleRetryRepositoriesViewsClonesCompletedTCS.Task.WaitAsync(TestCancellationTokenSource.Token).ConfigureAwait(false);
		repository_Database = await repositoryDatabase.GetRepository(repository_Final.Url, TestCancellationTokenSource.Token).ConfigureAwait(false) ?? throw new NullReferenceException();

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(wasScheduledSuccessfully_First);
			Assert.That(wasScheduledSuccessfully_Second, Is.False);

			Assert.That(organizationName_Final, Is.EqualTo(organizationName_Initial));

			Assert.That(repository_Final.ContainsViewsClonesStarsData);
			Assert.That(repository_Database.ContainsViewsClonesStarsData);

			Assert.That(repository_Final.DailyClonesList, Is.Not.Null);
			Assert.That(repository_Final.DailyViewsList, Is.Not.Null);
			Assert.That(repository_Final.StarredAt, Is.Not.Null);
			Assert.That(repository_Final.TotalClones, Is.Not.Null);
			Assert.That(repository_Final.TotalUniqueClones, Is.Not.Null);
			Assert.That(repository_Final.TotalUniqueViews, Is.Not.Null);
			Assert.That(repository_Final.TotalViews, Is.Not.Null);


			Assert.That(repository_Database.DailyClonesList, Is.Not.Null);
			Assert.That(repository_Database.DailyViewsList, Is.Not.Null);
			Assert.That(repository_Database.StarredAt, Is.Not.Null);
			Assert.That(repository_Database.TotalClones, Is.Not.Null);
			Assert.That(repository_Database.TotalUniqueClones, Is.Not.Null);
			Assert.That(repository_Database.TotalUniqueViews, Is.Not.Null);
			Assert.That(repository_Database.TotalViews, Is.Not.Null);

			Assert.That(repository_Database.Name, Is.EqualTo(repository_Final.Name));
			Assert.That(repository_Database.Description, Is.EqualTo(repository_Final.Description));
			Assert.That(repository_Database.ForkCount, Is.EqualTo(repository_Final.ForkCount));
			Assert.That(repository_Database.IsFavorite, Is.EqualTo(repository_Final.IsFavorite));
			Assert.That(repository_Database.IsFork, Is.EqualTo(repository_Final.IsFork));
			Assert.That(repository_Database.IssuesCount, Is.EqualTo(repository_Final.IssuesCount));
			Assert.That(repository_Database.IsTrending, Is.EqualTo(repository_Final.IsTrending));
		});

		void HandleScheduleRetryRepositoriesViewsClonesStarsCompleted(object? sender, Repository e)
		{
			RetryRepositoriesViewsClonesStarsJob.JobCompleted -= HandleScheduleRetryRepositoriesViewsClonesStarsCompleted;
			scheduleRetryRepositoriesViewsClonesCompletedTCS.SetResult(e);
		}

		void HandleScheduleRetryOrganizationsRepositoriesCompleted(object? sender, string e)
		{
			RetryOrganizationsRepositoriesJob.JobCompleted -= HandleScheduleRetryOrganizationsRepositoriesCompleted;
			scheduleRetryOrganizationsRepositoriesCompletedTCS.SetResult(e);
		}
	}
}