using GitTrends.Common;
namespace GitTrends.UnitTests.BackgroundJobs;

class RetryGetReferringSitesJobTests : BaseJobTest
{
	[Test]
	public void VerifyIdentifiers()
	{
		// Assert
		var repository = CreateRepository(false);
		var retryGetReferringSitesJob = ServiceCollection.ServiceProvider.GetRequiredService<RetryGetReferringSitesJob>();

		// Assert
		Assert.That(retryGetReferringSitesJob.GetJobIdentifier(repository), Is.EqualTo($"{retryGetReferringSitesJob.Identifier}.{repository.Url}"));
	}

	[Test]
	public async Task ScheduleRetryGetReferringSitesTest_AuthenticatedUser()
	{
		//Arrange
		bool wasScheduledSuccessfully_First, wasScheduledSuccessfully_Second;
		Repository repository_Final;
		MobileReferringSiteModel mobileReferringSiteModel, mobileReferringSiteModel_Database;
		IReadOnlyList<MobileReferringSiteModel> mobileReferringSitesList_Initial, mobileReferringSitesList_Final;

		Repository repository_Initial = new Repository(GitHubConstants.GitTrendsRepoName, GitHubConstants.GitTrendsRepoName, 1, GitHubConstants.GitTrendsRepoOwner,
			GitHubConstants.GitTrendsAvatarUrl, 1, 2, 3, "https://github.com/brminnick/gittrends", false, DateTimeOffset.UtcNow, RepositoryPermission.ADMIN, false);

		var mobileReferringSiteRetrievedTCS = new TaskCompletionSource<MobileReferringSiteModel>();
		var scheduleRetryGetReferringSiteCompletedTCS = new TaskCompletionSource<Repository>();

		RetryGetReferringSitesJob.MobileReferringSiteRetrieved += HandleMobileReferringSiteRetrieved;
		RetryGetReferringSitesJob.JobCompleted += HandleScheduleRetryGetReferringSitesCompleted;

		var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
		var referringSitesDatabase = ServiceCollection.ServiceProvider.GetRequiredService<ReferringSitesDatabase>();
		var backgroundFetchService = ServiceCollection.ServiceProvider.GetRequiredService<BackgroundFetchService>();
		var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();
		var referringSitesViewModel = ServiceCollection.ServiceProvider.GetRequiredService<ReferringSitesViewModel>();

		await AuthenticateUser(gitHubUserService, gitHubGraphQLApiService, TestCancellationTokenSource.Token).ConfigureAwait(false);

		//Act
		mobileReferringSitesList_Initial = referringSitesViewModel.MobileReferringSitesList;
		wasScheduledSuccessfully_First = backgroundFetchService.TryScheduleRetryGetReferringSites(repository_Initial);
		wasScheduledSuccessfully_Second = backgroundFetchService.TryScheduleRetryGetReferringSites(repository_Initial);

		mobileReferringSiteModel = await mobileReferringSiteRetrievedTCS.Task.WaitAsync(TestCancellationTokenSource.Token).ConfigureAwait(false);
		repository_Final = await scheduleRetryGetReferringSiteCompletedTCS.Task.WaitAsync(TestCancellationTokenSource.Token).ConfigureAwait(false);

		mobileReferringSitesList_Final = referringSitesViewModel.MobileReferringSitesList;

		IReadOnlyList<MobileReferringSiteModel> mobileReferringSiteModelsFromDatabase = await referringSitesDatabase.GetReferringSites(repository_Initial.Url, TestCancellationTokenSource.Token).ConfigureAwait(false);
		mobileReferringSiteModel_Database = mobileReferringSiteModelsFromDatabase.Single(x => x.ReferrerUri is not null && x.ReferrerUri == mobileReferringSiteModel.ReferrerUri);

		Assert.Multiple(() =>
		{
			// Assert
			Assert.That(wasScheduledSuccessfully_First);
			Assert.That(wasScheduledSuccessfully_Second, Is.False);

			Assert.That(mobileReferringSiteModel.FavIcon, Is.Not.Null);
			Assert.That(mobileReferringSiteModel.FavIconImageUrl, Is.Not.Null);
			if (string.Empty == mobileReferringSiteModel.FavIconImageUrl)
				Assert.That(((FileImageSource?)mobileReferringSiteModel.FavIcon)?.File, Is.EqualTo(FavIconService.DefaultFavIcon));
			else
				Assert.That(mobileReferringSiteModel.FavIconImageUrl, Is.Not.Empty);
			Assert.That(mobileReferringSiteModel.IsReferrerUriValid);
			Assert.That(mobileReferringSiteModel.Referrer, Is.Not.Null);
			Assert.That(mobileReferringSiteModel.Referrer, Is.Not.Empty);
			Assert.That(mobileReferringSiteModel.ReferrerUri, Is.Not.Null);
			Assert.That(Uri.IsWellFormedUriString(mobileReferringSiteModel.ReferrerUri?.ToString(), UriKind.Absolute));
			Assert.That(mobileReferringSiteModel.TotalCount, Is.GreaterThan(0));
			Assert.That(mobileReferringSiteModel.TotalUniqueCount, Is.GreaterThan(0));

			Assert.That(mobileReferringSiteModel_Database.ToString(), Is.EqualTo(mobileReferringSiteModel.ToString()));

			Assert.That(mobileReferringSiteModel_Database.DownloadedAt, Is.EqualTo(mobileReferringSiteModel.DownloadedAt));
			Assert.That(mobileReferringSiteModel_Database.FavIcon?.ToString(), Is.EqualTo(mobileReferringSiteModel.FavIcon?.ToString()));
			Assert.That(mobileReferringSiteModel_Database.FavIconImageUrl, Is.EqualTo(mobileReferringSiteModel.FavIconImageUrl));
			Assert.That(mobileReferringSiteModel_Database.IsReferrerUriValid, Is.EqualTo(mobileReferringSiteModel.IsReferrerUriValid));
			Assert.That(mobileReferringSiteModel_Database.Referrer, Is.EqualTo(mobileReferringSiteModel.Referrer));
			Assert.That(mobileReferringSiteModel_Database.ReferrerUri?.ToString(), Is.EqualTo(mobileReferringSiteModel.ReferrerUri?.ToString()));
			Assert.That(mobileReferringSiteModel_Database.TotalCount, Is.EqualTo(mobileReferringSiteModel.TotalCount));
			Assert.That(mobileReferringSiteModel_Database.TotalUniqueCount, Is.EqualTo(mobileReferringSiteModel.TotalUniqueCount));

			Assert.That(repository_Final.ToString(), Is.EqualTo(repository_Initial.ToString()));

			Assert.That(repository_Final.ContainsViewsClonesStarsData, Is.EqualTo(repository_Initial.ContainsViewsClonesStarsData));
			Assert.That(repository_Final.DailyClonesList?.Count, Is.EqualTo(repository_Initial.DailyClonesList?.Count));
			Assert.That(repository_Final.DailyViewsList?.Count, Is.EqualTo(repository_Initial.DailyViewsList?.Count));
			Assert.That(repository_Final.DataDownloadedAt, Is.EqualTo(repository_Initial.DataDownloadedAt));
			Assert.That(repository_Final.Description, Is.EqualTo(repository_Initial.Description));
			Assert.That(repository_Final.ForkCount, Is.EqualTo(repository_Initial.ForkCount));
			Assert.That(repository_Final.IsFavorite, Is.EqualTo(repository_Initial.IsFavorite));
			Assert.That(repository_Final.IsFork, Is.EqualTo(repository_Initial.IsFork));
			Assert.That(repository_Final.IssuesCount, Is.EqualTo(repository_Initial.IssuesCount));
			Assert.That(repository_Final.IsTrending, Is.EqualTo(repository_Initial.IsTrending));
			Assert.That(repository_Final.Name, Is.EqualTo(repository_Initial.Name));
			Assert.That(repository_Final.OwnerAvatarUrl, Is.EqualTo(repository_Initial.OwnerAvatarUrl));
			Assert.That(repository_Final.OwnerLogin, Is.EqualTo(repository_Initial.OwnerLogin));
			Assert.That(repository_Final.Permission, Is.EqualTo(repository_Initial.Permission));
			Assert.That(repository_Final.StarCount, Is.EqualTo(repository_Initial.StarCount));
			Assert.That(repository_Final.StarredAt, Is.EqualTo(repository_Initial.StarredAt));
			Assert.That(repository_Final.TotalClones, Is.EqualTo(repository_Initial.TotalClones));
			Assert.That(repository_Final.TotalUniqueClones, Is.EqualTo(repository_Initial.TotalUniqueClones));
			Assert.That(repository_Final.TotalUniqueViews, Is.EqualTo(repository_Initial.TotalUniqueViews));
			Assert.That(repository_Final.TotalViews, Is.EqualTo(repository_Initial.TotalViews));
			Assert.That(repository_Final.Url, Is.EqualTo(repository_Initial.Url));
			Assert.That(repository_Final.WatchersCount, Is.EqualTo(repository_Initial.WatchersCount));

			Assert.That(mobileReferringSitesList_Initial, Is.Empty);
			Assert.That(mobileReferringSitesList_Final, Has.Count.GreaterThan(mobileReferringSitesList_Initial.Count));
			foreach (var mobileReferringSite in mobileReferringSitesList_Final)
			{
				Assert.That(mobileReferringSite.FavIcon, Is.Not.Null);
				Assert.That(mobileReferringSite.FavIconImageUrl, Is.Not.Null);
				if (string.Empty == mobileReferringSite.FavIconImageUrl)
					Assert.That(((FileImageSource?)mobileReferringSite.FavIcon)?.File, Is.EqualTo(FavIconService.DefaultFavIcon));
				else
					Assert.That(mobileReferringSite.FavIconImageUrl, Is.Not.Empty);
				Assert.That(mobileReferringSite.IsReferrerUriValid);
				Assert.That(mobileReferringSite.Referrer, Is.Not.Null);
				Assert.That(mobileReferringSite.Referrer, Is.Not.Empty);
				Assert.That(mobileReferringSite.ReferrerUri, Is.Not.Null);
				Assert.That(Uri.IsWellFormedUriString(mobileReferringSite.ReferrerUri?.ToString(), UriKind.Absolute));
				Assert.That(mobileReferringSite.TotalCount, Is.GreaterThan(0));
				Assert.That(mobileReferringSite.TotalUniqueCount, Is.GreaterThan(0));
			}
		});

		void HandleMobileReferringSiteRetrieved(object? sender, MobileReferringSiteModel e)
		{
			RetryGetReferringSitesJob.MobileReferringSiteRetrieved -= HandleMobileReferringSiteRetrieved;
			mobileReferringSiteRetrievedTCS.TrySetResult(e);
		}

		void HandleScheduleRetryGetReferringSitesCompleted(object? sender, Repository e)
		{
			RetryGetReferringSitesJob.JobCompleted -= HandleScheduleRetryGetReferringSitesCompleted;
			scheduleRetryGetReferringSiteCompletedTCS.SetResult(e);
		}
	}
}