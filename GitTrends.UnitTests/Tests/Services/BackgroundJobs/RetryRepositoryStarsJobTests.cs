using GitTrends.Common;

namespace GitTrends.UnitTests.BackgroundJobs;

class RetryRepositoryStarsJobTests : BaseJobTest
{
	[Test]
	public void VerifyIdentifiers()
	{
		// Assert
		var repository = CreateRepository(false);
		var retryRepositoryStarsJob = ServiceCollection.ServiceProvider.GetRequiredService<RetryRepositoryStarsJob>();

		// Assert
		Assert.That(retryRepositoryStarsJob.GetJobIdentifier(repository), Is.EqualTo($"{retryRepositoryStarsJob.Identifier}.{repository.Url}"));
	}

	[Test]
	public async Task ScheduleRetryRepositoriesStarsTest_AuthenticatedUser()
	{
		//Arrange
		bool wasScheduledSuccessfully_First, wasScheduledSuccessfully_Second;
		Repository repository_Initial, repository_Final, repository_Database;

		var scheduleRetryRepositoriesStarsCompletedTCS = new TaskCompletionSource<Repository>();
		RetryRepositoryStarsJob.UpdatedRepositorySavedToDatabase += HandleUpdatedRepositorySavedToDatabase;

		var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
		var repositoryDatabase = ServiceCollection.ServiceProvider.GetRequiredService<RepositoryDatabase>();
		var backgroundFetchService = ServiceCollection.ServiceProvider.GetRequiredService<BackgroundFetchService>();
		var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();

		await AuthenticateUser(gitHubUserService, gitHubGraphQLApiService, TestCancellationTokenSource.Token).ConfigureAwait(false);

		repository_Initial = new Repository(GitHubConstants.GitTrendsRepoName, GitHubConstants.GitTrendsRepoName, 1, GitHubConstants.GitTrendsRepoOwner,
			GitHubConstants.GitTrendsAvatarUrl, 1, 2, 3, "https://github.com/brminnick/gittrends", false, DateTimeOffset.UtcNow, RepositoryPermission.ADMIN, false);

		//Act
		wasScheduledSuccessfully_First = backgroundFetchService.TryScheduleRetryRepositoriesStars(repository_Initial);
		wasScheduledSuccessfully_Second = backgroundFetchService.TryScheduleRetryRepositoriesStars(repository_Initial);

		var cancellationToken = new CancellationTokenSource(TimeSpan.FromMinutes(6)).Token;
		repository_Final = await scheduleRetryRepositoriesStarsCompletedTCS.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
		repository_Database = await repositoryDatabase.GetRepository(repository_Initial.Url, cancellationToken).ConfigureAwait(false) ?? throw new NullReferenceException();

		Assert.Multiple(() =>
		{
			//Assert
			Assert.That(wasScheduledSuccessfully_First);
			Assert.That(wasScheduledSuccessfully_Second, Is.False);

			Assert.That(repository_Initial.ContainsViewsClonesData, Is.False);
			Assert.That(repository_Initial.ContainsViewsClonesStarsData, Is.False);
			Assert.That(repository_Final.ContainsViewsClonesData, Is.False);
			Assert.That(repository_Final.ContainsViewsClonesStarsData, Is.False);
			Assert.That(repository_Database.ContainsViewsClonesData, Is.False);
			Assert.That(repository_Database.ContainsViewsClonesStarsData, Is.False);

			Assert.That(repository_Initial.DailyClonesList, Is.Null);
			Assert.That(repository_Initial.DailyViewsList, Is.Null);
			Assert.That(repository_Initial.StarredAt, Is.Null);
			Assert.That(repository_Initial.TotalClones, Is.Null);
			Assert.That(repository_Initial.TotalUniqueClones, Is.Null);
			Assert.That(repository_Initial.TotalUniqueViews, Is.Null);
			Assert.That(repository_Initial.TotalViews, Is.Null);

			Assert.That(repository_Final.StarredAt, Is.Not.Null);
			Assert.That(repository_Final.StarredAt ?? [], Is.Not.Empty);
			Assert.That(repository_Final.DailyClonesList, Is.Null);
			Assert.That(repository_Final.DailyViewsList, Is.Null);
			Assert.That(repository_Final.TotalClones, Is.Null);
			Assert.That(repository_Final.TotalUniqueClones, Is.Null);
			Assert.That(repository_Final.TotalUniqueViews, Is.Null);
			Assert.That(repository_Final.TotalViews, Is.Null);

			Assert.That(repository_Final.Name, Is.EqualTo(repository_Initial.Name));
			Assert.That(repository_Final.Description, Is.EqualTo(repository_Initial.Description));
			Assert.That(repository_Final.ForkCount, Is.EqualTo(repository_Initial.ForkCount));
			Assert.That(repository_Final.IsFavorite, Is.EqualTo(repository_Initial.IsFavorite));
			Assert.That(repository_Final.IsFork, Is.EqualTo(repository_Initial.IsFork));
			Assert.That(repository_Final.IssuesCount, Is.EqualTo(repository_Initial.IssuesCount));

			Assert.That(repository_Database.StarredAt, Is.Not.Null);
			Assert.That(repository_Database.StarredAt ?? [], Is.Not.Empty);
			Assert.That(repository_Database.DailyClonesList, Is.Null);
			Assert.That(repository_Database.DailyViewsList, Is.Null);
			Assert.That(repository_Database.TotalClones, Is.Null);
			Assert.That(repository_Database.TotalUniqueClones, Is.Null);
			Assert.That(repository_Database.TotalUniqueViews, Is.Null);
			Assert.That(repository_Database.TotalViews, Is.Null);

			Assert.That(repository_Database.Name, Is.EqualTo(repository_Final.Name));
			Assert.That(repository_Database.Description, Is.EqualTo(repository_Final.Description));
			Assert.That(repository_Database.ForkCount, Is.EqualTo(repository_Final.ForkCount));
			Assert.That(repository_Database.IsFavorite, Is.EqualTo(repository_Final.IsFavorite));
			Assert.That(repository_Database.IsFork, Is.EqualTo(repository_Final.IsFork));
			Assert.That(repository_Database.IssuesCount, Is.EqualTo(repository_Final.IssuesCount));
			Assert.That(repository_Database.IsTrending, Is.EqualTo(repository_Final.IsTrending));
		});

		void HandleUpdatedRepositorySavedToDatabase(object? sender, Repository e)
		{
			RetryRepositoriesViewsClonesStarsJob.JobCompleted -= HandleUpdatedRepositorySavedToDatabase;
			scheduleRetryRepositoriesStarsCompletedTCS.SetResult(e);
		}
	}
}