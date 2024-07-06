using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using GitTrends.Shared;

namespace GitTrends.UnitTests;

class BackgroundFetchServiceTests : BaseTest
{
	[Test]
	public void VerifyIdentifiers()
	{
		// Assert
		const string organizationName = "GitTrends";
		var repository = CreateRepository(false);
		var backgroundFetchService = ServiceCollection.ServiceProvider.GetRequiredService<BackgroundFetchService>();

		// Assert
		Assert.Multiple(() =>
		{
			Assert.That(backgroundFetchService.GetRetryGetReferringSitesIdentifier(repository), Is.EqualTo($"{backgroundFetchService.RetryGetReferringSitesIdentifier}.{repository.Url}"));
			Assert.That(backgroundFetchService.GetRetryRepositoriesStarsIdentifier(repository), Is.EqualTo($"{backgroundFetchService.RetryRepositoriesStarsIdentifier}.{repository.Url}"));
			Assert.That(backgroundFetchService.GetRetryOrganizationsRepositoriesIdentifier(organizationName), Is.EqualTo($"{backgroundFetchService.RetryOrganizationsReopsitoriesIdentifier}.{organizationName}"));
			Assert.That(backgroundFetchService.GetRetryRepositoriesViewsClonesStarsIdentifier(repository), Is.EqualTo($"{backgroundFetchService.RetryRepositoriesViewsClonesStarsIdentifier}.{repository.Url}"));
		});
	}

	[Test]
	public async Task ScheduleRetryRepositoriesViewsClonesStarsTest_AuthenticatedUser()
	{
		//Arrange
		bool wasScheduledSuccessfully_First, wasScheduledSuccessfully_Second;
		Repository repository_Initial, repository_Final, repository_Database;

		repository_Initial = new Repository(GitHubConstants.GitTrendsRepoName, GitHubConstants.GitTrendsRepoName, 1, GitHubConstants.GitTrendsRepoOwner,
			GitHubConstants.GitTrendsAvatarUrl, 1, 2, 3, "https://github.com/brminnick/gittrends", false, DateTimeOffset.UtcNow, RepositoryPermission.ADMIN, false);

		var scheduleRetryRepositoriesViewsClonesStarsCompletedTCS = new TaskCompletionSource<Repository>();
		BackgroundFetchService.ScheduleRetryRepositoriesViewsClonesStarsCompleted += HandleScheduleRetryRepositoriesViewsClonesStarsCompleted;


		var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
		var repositoryDatabase = ServiceCollection.ServiceProvider.GetRequiredService<RepositoryDatabase>();
		var backgroundFetchService = ServiceCollection.ServiceProvider.GetRequiredService<BackgroundFetchService>();
		var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();

		await AuthenticateUser(gitHubUserService, gitHubGraphQLApiService, TestCancellationTokenSource.Token).ConfigureAwait(false);


		//Act
		wasScheduledSuccessfully_First = backgroundFetchService.TryScheduleRetryRepositoriesViewsClonesStars(repository_Initial);
		wasScheduledSuccessfully_Second = backgroundFetchService.TryScheduleRetryRepositoriesViewsClonesStars(repository_Initial);

		repository_Final = await scheduleRetryRepositoriesViewsClonesStarsCompletedTCS.Task.ConfigureAwait(false);
		repository_Database = await repositoryDatabase.GetRepository(repository_Initial.Url, TestCancellationTokenSource.Token).ConfigureAwait(false) ?? throw new NullReferenceException();

		Assert.Multiple(() =>
		{
			//Assert
			Assert.That(wasScheduledSuccessfully_First);
			Assert.That(wasScheduledSuccessfully_Second, Is.False);

			Assert.That(repository_Initial.StarredAt, Is.Null);
			Assert.That(repository_Final.StarredAt, Is.Not.Null);
			Assert.That(repository_Database.StarredAt, Is.Not.Null);

			Assert.That(repository_Initial.DailyClonesList, Is.Null);
			Assert.That(repository_Initial.DailyViewsList, Is.Null);
			Assert.That(repository_Initial.StarredAt, Is.Null);
			Assert.That(repository_Initial.TotalClones, Is.Null);
			Assert.That(repository_Initial.TotalUniqueClones, Is.Null);
			Assert.That(repository_Initial.TotalUniqueViews, Is.Null);
			Assert.That(repository_Initial.TotalViews, Is.Null);

			Assert.That(repository_Final.StarredAt, Is.Not.Null);
			Assert.That(repository_Final.DailyClonesList, Is.Not.Null);
			Assert.That(repository_Final.DailyViewsList, Is.Not.Null);
			Assert.That(repository_Final.TotalClones, Is.Not.Null);
			Assert.That(repository_Final.TotalUniqueClones, Is.Not.Null);
			Assert.That(repository_Final.TotalUniqueViews, Is.Not.Null);
			Assert.That(repository_Final.TotalViews, Is.Not.Null);

			Assert.That(repository_Final.Name, Is.EqualTo(repository_Initial.Name));
			Assert.That(repository_Final.Description, Is.EqualTo(repository_Initial.Description));
			Assert.That(repository_Final.ForkCount, Is.EqualTo(repository_Initial.ForkCount));
			Assert.That(repository_Final.IsFavorite, Is.EqualTo(repository_Initial.IsFavorite));
			Assert.That(repository_Final.IsFork, Is.EqualTo(repository_Initial.IsFork));
			Assert.That(repository_Final.IssuesCount, Is.EqualTo(repository_Initial.IssuesCount));

			Assert.That(repository_Database.StarredAt, Is.Not.Null);
			Assert.That(repository_Database.DailyClonesList, Is.Not.Null);
			Assert.That(repository_Database.DailyViewsList, Is.Not.Null);
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
			if (e.Url == repository_Initial.Url)
			{
				BackgroundFetchService.ScheduleRetryRepositoriesViewsClonesStarsCompleted -= HandleScheduleRetryRepositoriesViewsClonesStarsCompleted;
				scheduleRetryRepositoriesViewsClonesStarsCompletedTCS.SetResult(e);
			}
		}
	}

	[Test]
	public async Task ScheduleRetryRepositoriesStarsTest_AuthenticatedUser()
	{
		//Arrange
		bool wasScheduledSuccessfully_First, wasScheduledSuccessfully_Second;
		Repository repository_Initial, repository_Final, repository_Database;

		var scheduleRetryRepositoriesStarsCompletedTCS = new TaskCompletionSource<Repository>();
		BackgroundFetchService.ScheduleRetryRepositoriesStarsCompleted += HandleScheduleRetryRepositoriesStarsCompleted;

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

		repository_Final = await scheduleRetryRepositoriesStarsCompletedTCS.Task.ConfigureAwait(false);
		repository_Database = await repositoryDatabase.GetRepository(repository_Initial.Url, TestCancellationTokenSource.Token).ConfigureAwait(false) ?? throw new NullReferenceException();

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

		void HandleScheduleRetryRepositoriesStarsCompleted(object? sender, Repository e)
		{
			BackgroundFetchService.ScheduleRetryRepositoriesStarsCompleted -= HandleScheduleRetryRepositoriesStarsCompleted;
			scheduleRetryRepositoriesStarsCompletedTCS.SetResult(e);
		}
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
		BackgroundFetchService.ScheduleRetryOrganizationsRepositoriesCompleted += HandleScheduleRetryOrganizationsRepositoriesCompleted;
		BackgroundFetchService.ScheduleRetryRepositoriesViewsClonesStarsCompleted += HandleScheduleRetryRepositoriesViewsClonesStarsCompleted;

		var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
		var repositoryDatabase = ServiceCollection.ServiceProvider.GetRequiredService<RepositoryDatabase>();
		var backgroundFetchService = ServiceCollection.ServiceProvider.GetRequiredService<BackgroundFetchService>();
		var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();

		await AuthenticateUser(gitHubUserService, gitHubGraphQLApiService, TestCancellationTokenSource.Token).ConfigureAwait(false);

		//Act
		wasScheduledSuccessfully_First = backgroundFetchService.TryScheduleRetryOrganizationsRepositories(organizationName_Initial);
		wasScheduledSuccessfully_Second = backgroundFetchService.TryScheduleRetryOrganizationsRepositories(organizationName_Initial);

		organizationName_Final = await scheduleRetryOrganizationsRepositoriesCompletedTCS.Task.ConfigureAwait(false);

		repository_Final = await scheduleRetryRepositoriesViewsClonesCompletedTCS.Task.ConfigureAwait(false);
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
			BackgroundFetchService.ScheduleRetryRepositoriesViewsClonesStarsCompleted -= HandleScheduleRetryRepositoriesViewsClonesStarsCompleted;
			scheduleRetryRepositoriesViewsClonesCompletedTCS.SetResult(e);
		}

		void HandleScheduleRetryOrganizationsRepositoriesCompleted(object? sender, string e)
		{
			BackgroundFetchService.ScheduleRetryOrganizationsRepositoriesCompleted -= HandleScheduleRetryOrganizationsRepositoriesCompleted;
			scheduleRetryOrganizationsRepositoriesCompletedTCS.SetResult(e);
		}
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

		BackgroundFetchService.MobileReferringSiteRetrieved += HandleMobileReferringSiteRetrieved;
		BackgroundFetchService.ScheduleRetryGetReferringSitesCompleted += HandleScheduleRetryGetReferringSitesCompleted;

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

		mobileReferringSiteModel = await mobileReferringSiteRetrievedTCS.Task.ConfigureAwait(false);
		repository_Final = await scheduleRetryGetReferringSiteCompletedTCS.Task.ConfigureAwait(false);

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
			BackgroundFetchService.MobileReferringSiteRetrieved -= HandleMobileReferringSiteRetrieved;
			mobileReferringSiteRetrievedTCS.TrySetResult(e);
		}

		void HandleScheduleRetryGetReferringSitesCompleted(object? sender, Repository e)
		{
			BackgroundFetchService.ScheduleRetryGetReferringSitesCompleted -= HandleScheduleRetryGetReferringSitesCompleted;
			scheduleRetryGetReferringSiteCompletedTCS.SetResult(e);
		}
	}

	[Test]
	public async Task CleanUpDatabaseTest()
	{
		//Arrange
		bool wasScheduledSuccessfully_First, wasScheduledSuccessfully_Second;
		var databaseCleanupCompletedTCS = new TaskCompletionSource<object?>();
		BackgroundFetchService.DatabaseCleanupCompleted += HandleDatabaseCompleted;

		int repositoryDatabaseCount_Initial, repositoryDatabaseCount_Final, referringSitesDatabaseCount_Initial, referringSitesDatabaseCount_Final;
		MobileReferringSiteModel expiredReferringSite_Initial, unexpiredReferringSite_Initial;
		Repository expiredRepository_Initial, unexpiredRepository_Initial, expiredRepository_Final, unexpiredRepository_Final;

		var repositoryDatabase = ServiceCollection.ServiceProvider.GetRequiredService<RepositoryDatabase>();
		var backgroundFetchService = ServiceCollection.ServiceProvider.GetRequiredService<BackgroundFetchService>();
		var referringSitesDatabase = ServiceCollection.ServiceProvider.GetRequiredService<ReferringSitesDatabase>();

		expiredRepository_Initial = CreateRepository(DateTimeOffset.UtcNow.Subtract(repositoryDatabase.ExpiresAt), "https://github.com/brminnick/gittrends");
		unexpiredRepository_Initial = CreateRepository(DateTimeOffset.UtcNow, "https://github.com/brminnick/gitstatus");

		expiredReferringSite_Initial = CreateMobileReferringSite(DateTimeOffset.UtcNow.Subtract(referringSitesDatabase.ExpiresAt), "Google");
		unexpiredReferringSite_Initial = CreateMobileReferringSite(DateTimeOffset.UtcNow, "codetraveler.io");

		await repositoryDatabase.SaveRepository(expiredRepository_Initial, TestCancellationTokenSource.Token).ConfigureAwait(false);
		await repositoryDatabase.SaveRepository(unexpiredRepository_Initial, TestCancellationTokenSource.Token).ConfigureAwait(false);

		await referringSitesDatabase.SaveReferringSite(expiredReferringSite_Initial, expiredRepository_Initial.Url, TestCancellationTokenSource.Token).ConfigureAwait(false);
		await referringSitesDatabase.SaveReferringSite(unexpiredReferringSite_Initial, unexpiredRepository_Initial.Url, TestCancellationTokenSource.Token).ConfigureAwait(false);

		//Act
		repositoryDatabaseCount_Initial = await getRepositoryDatabaseCount(repositoryDatabase).ConfigureAwait(false);
		referringSitesDatabaseCount_Initial = await getReferringSitesDatabaseCount(referringSitesDatabase, expiredRepository_Initial.Url, unexpiredRepository_Initial.Url).ConfigureAwait(false);

		wasScheduledSuccessfully_First = backgroundFetchService.TryScheduleCleanUpDatabase();
		wasScheduledSuccessfully_Second = backgroundFetchService.TryScheduleCleanUpDatabase();

		await databaseCleanupCompletedTCS.Task.ConfigureAwait(false);

		repositoryDatabaseCount_Final = await getRepositoryDatabaseCount(repositoryDatabase).ConfigureAwait(false);
		referringSitesDatabaseCount_Final = await getReferringSitesDatabaseCount(referringSitesDatabase, expiredRepository_Initial.Url, unexpiredRepository_Initial.Url).ConfigureAwait(false);

		var finalRepositories = await repositoryDatabase.GetRepositories(TestCancellationTokenSource.Token).ConfigureAwait(false);
		expiredRepository_Final = finalRepositories.First(x => x.DataDownloadedAt == expiredRepository_Initial.DataDownloadedAt);
		unexpiredRepository_Final = finalRepositories.First(x => x.DataDownloadedAt == unexpiredRepository_Initial.DataDownloadedAt);

		Assert.Multiple(() =>
		{
			//Assert
			Assert.That(wasScheduledSuccessfully_First);
			Assert.That(wasScheduledSuccessfully_Second, Is.False);

			Assert.That(repositoryDatabaseCount_Initial, Is.EqualTo(2));
			Assert.That(referringSitesDatabaseCount_Initial, Is.EqualTo(2));

			Assert.That(repositoryDatabaseCount_Final, Is.EqualTo(repositoryDatabaseCount_Initial));

			Assert.That(expiredRepository_Initial.DailyClonesList?.Sum(static x => x.TotalClones), Is.EqualTo(0));
			Assert.That(expiredRepository_Initial.DailyClonesList?.Sum(static x => x.TotalUniqueClones), Is.EqualTo(0));
			Assert.That(expiredRepository_Initial.DailyViewsList?.Sum(static x => x.TotalViews), Is.EqualTo(0));
			Assert.That(expiredRepository_Initial.DailyViewsList?.Sum(static x => x.TotalUniqueViews), Is.EqualTo(0));

			Assert.That(expiredRepository_Final.DailyClonesList, Is.Null);
			Assert.That(expiredRepository_Final.DailyViewsList, Is.Null);

			Assert.That(unexpiredRepository_Final.DailyClonesList?.Sum(static x => x.TotalClones), Is.EqualTo(unexpiredRepository_Initial.DailyClonesList?.Sum(static x => x.TotalClones)));
			Assert.That(unexpiredRepository_Final.DailyClonesList?.Sum(static x => x.TotalUniqueClones), Is.EqualTo(unexpiredRepository_Initial.DailyClonesList?.Sum(static x => x.TotalUniqueClones)));
			Assert.That(unexpiredRepository_Final.DailyViewsList?.Sum(static x => x.TotalViews), Is.EqualTo(unexpiredRepository_Initial.DailyViewsList?.Sum(static x => x.TotalViews)));
			Assert.That(unexpiredRepository_Final.DailyViewsList?.Sum(static x => x.TotalUniqueViews), Is.EqualTo(unexpiredRepository_Initial.DailyViewsList?.Sum(static x => x.TotalUniqueViews)));

			Assert.That(repositoryDatabaseCount_Final, Is.EqualTo(2));
			Assert.That(referringSitesDatabaseCount_Final, Is.EqualTo(1));

			Assert.That(unexpiredRepository_Initial.IsFavorite, Is.True);
			Assert.That(unexpiredRepository_Final.IsFavorite, Is.True);
		});

		async Task<int> getRepositoryDatabaseCount(RepositoryDatabase repositoryDatabase)
		{
			var repositories = await repositoryDatabase.GetRepositories(TestCancellationTokenSource.Token).ConfigureAwait(false);
			return repositories.Count;
		}

		async Task<int> getReferringSitesDatabaseCount(ReferringSitesDatabase referringSitesDatabase, params string[] repositoryUrl)
		{
			var referringSitesCount = 0;

			foreach (var url in repositoryUrl)
			{
				var referringSites = await referringSitesDatabase.GetReferringSites(url, TestCancellationTokenSource.Token).ConfigureAwait(false);
				referringSitesCount += referringSites.Count();
			}
			return referringSitesCount;
		}

		void HandleDatabaseCompleted(object? sender, EventArgs e)
		{
			BackgroundFetchService.DatabaseCleanupCompleted -= HandleDatabaseCompleted;
			databaseCleanupCompletedTCS.SetResult(null);
		}
	}

	[Test]
	public async Task NotifyTrendingRepositoriesTest_NotLoggedIn()
	{
		//Arrange
		var scheduleNotifyTrendingRepositoriesCompletedTCS = new TaskCompletionSource<bool>();
		BackgroundFetchService.ScheduleNotifyTrendingRepositoriesCompleted += HandleScheduleNotifyTrendingRepositoriesCompleted;

		var backgroundFetchService = ServiceCollection.ServiceProvider.GetRequiredService<BackgroundFetchService>();

		//Act
		backgroundFetchService.TryScheduleNotifyTrendingRepositories(CancellationToken.None);

		var result = await scheduleNotifyTrendingRepositoriesCompletedTCS.Task.ConfigureAwait(false);

		//Assert
		Assert.That(result, Is.False);

		void HandleScheduleNotifyTrendingRepositoriesCompleted(object? sender, bool e)
		{
			BackgroundFetchService.ScheduleNotifyTrendingRepositoriesCompleted -= HandleScheduleNotifyTrendingRepositoriesCompleted;
			scheduleNotifyTrendingRepositoriesCompletedTCS.SetResult(e);
		}
	}


	[Test]
	public async Task NotifyTrendingRepositoriesTest_DemoUser()
	{
		//Arrange
		var scheduleNotifyTrendingRepositoriesCompletedTCS = new TaskCompletionSource<bool>();
		BackgroundFetchService.ScheduleNotifyTrendingRepositoriesCompleted += HandleScheduleNotifyTrendingRepositoriesCompleted;

		var gitHubAuthenticationService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubAuthenticationService>();
		await gitHubAuthenticationService.ActivateDemoUser(TestCancellationTokenSource.Token).ConfigureAwait(false);

		var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
		var backgroundFetchService = ServiceCollection.ServiceProvider.GetRequiredService<BackgroundFetchService>();

		//Act
		backgroundFetchService.TryScheduleNotifyTrendingRepositories(CancellationToken.None);

		var result = await scheduleNotifyTrendingRepositoriesCompletedTCS.Task.ConfigureAwait(false);

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(gitHubUserService.IsDemoUser);
			Assert.That(gitHubUserService.IsAuthenticated, Is.False);
			Assert.That(result, Is.False);
		});

		void HandleScheduleNotifyTrendingRepositoriesCompleted(object? sender, bool e)
		{
			BackgroundFetchService.ScheduleNotifyTrendingRepositoriesCompleted -= HandleScheduleNotifyTrendingRepositoriesCompleted;
			scheduleNotifyTrendingRepositoriesCompletedTCS.SetResult(e);
		}
	}

	[Test]
	public async Task NotifyTrendingRepositoriesTest_AuthenticatedUser()
	{
		//Arrange
		bool wasScheduledSuccessfully_First, wasScheduledSuccessfully_Second;
		var scheduleNotifyTrendingRepositoriesCompletedTCS = new TaskCompletionSource<bool>();
		BackgroundFetchService.ScheduleNotifyTrendingRepositoriesCompleted += HandleScheduleNotifyTrendingRepositoriesCompleted;

		var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
		var backgroundFetchService = ServiceCollection.ServiceProvider.GetRequiredService<BackgroundFetchService>();
		var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();

		await AuthenticateUser(gitHubUserService, gitHubGraphQLApiService, TestCancellationTokenSource.Token).ConfigureAwait(false);

		//Act
		wasScheduledSuccessfully_First = backgroundFetchService.TryScheduleNotifyTrendingRepositories(CancellationToken.None);
		wasScheduledSuccessfully_Second = backgroundFetchService.TryScheduleNotifyTrendingRepositories(CancellationToken.None);

		var result = await scheduleNotifyTrendingRepositoriesCompletedTCS.Task.ConfigureAwait(false);

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
			BackgroundFetchService.ScheduleNotifyTrendingRepositoriesCompleted -= HandleScheduleNotifyTrendingRepositoriesCompleted;
			scheduleNotifyTrendingRepositoriesCompletedTCS.SetResult(e);
		}
	}

	static Repository CreateRepository(DateTimeOffset downloadedAt, string repositoryUrl)
	{
		var starredAtList = new List<DateTimeOffset>();
		var dailyViewsList = new List<DailyViewsModel>();
		var dailyClonesList = new List<DailyClonesModel>();

		for (int i = 0; i < 14; i++)
		{
			var count = DemoDataConstants.GetRandomNumber();
			var uniqueCount = count / 2; //Ensures uniqueCount is always less than count

			starredAtList.Add(DemoDataConstants.GetRandomDate());
			dailyViewsList.Add(new DailyViewsModel(downloadedAt.Subtract(TimeSpan.FromDays(i)), count, uniqueCount));
			dailyClonesList.Add(new DailyClonesModel(downloadedAt.Subtract(TimeSpan.FromDays(i)), count, uniqueCount));
		}

		return new Repository($"Repository " + DemoDataConstants.GetRandomText(), DemoDataConstants.GetRandomText(), DemoDataConstants.GetRandomNumber(),
			DemoUserConstants.Alias, GitHubConstants.GitTrendsAvatarUrl, DemoDataConstants.GetRandomNumber(), DemoDataConstants.GetRandomNumber(), starredAtList.Count,
			repositoryUrl, false, downloadedAt, RepositoryPermission.ADMIN, false, true, dailyViewsList, dailyClonesList, starredAtList);
	}

	static MobileReferringSiteModel CreateMobileReferringSite(DateTimeOffset downloadedAt, string referrer)
	{
		return new MobileReferringSiteModel(new ReferringSiteModel(DemoDataConstants.GetRandomNumber(),
			DemoDataConstants.GetRandomNumber(),
			referrer))
		{
			DownloadedAt = downloadedAt
		};
	}
}