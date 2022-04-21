using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using GitTrends.Shared;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace GitTrends.UnitTests
{
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
			Assert.AreEqual($"{backgroundFetchService.RetryGetReferringSitesIdentifier}.{repository.Url}", backgroundFetchService.GetRetryGetReferringSitesIdentifier(repository));
			Assert.AreEqual($"{backgroundFetchService.RetryRepositoriesStarsIdentifier}.{repository.Url}", backgroundFetchService.GetRetryRepositoriesStarsIdentifier(repository));
			Assert.AreEqual($"{backgroundFetchService.RetryOrganizationsReopsitoriesIdentifier}.{organizationName}", backgroundFetchService.GetRetryOrganizationsRepositoriesIdentifier(organizationName));
			Assert.AreEqual($"{backgroundFetchService.RetryRepositoriesViewsClonesStarsIdentifier}.{repository.Url}", backgroundFetchService.GetRetryRepositoriesViewsClonesStarsIdentifier(repository));
		}

		[Test]
		public async Task ScheduleRetryRepositoriesViewsClonesStarsTest_AuthenticatedUser()
		{
			//Arrange
			bool wasScheduledSuccessfully_First, wasScheduledSuccessfully_Second;
			Repository repository_Initial, repository_Final, repository_Database;

			var scheduleRetryRepositoriesViewsClonesStarsCompletedTCS = new TaskCompletionSource<Repository>();
			BackgroundFetchService.ScheduleRetryRepositoriesViewsClonesStarsCompleted += HandleScheduleRetryRepositoriesViewsClonesStarsCompleted;

			var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
			var repositoryDatabase = ServiceCollection.ServiceProvider.GetRequiredService<RepositoryDatabase>();
			var backgroundFetchService = ServiceCollection.ServiceProvider.GetRequiredService<BackgroundFetchService>();
			var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();

			await AuthenticateUser(gitHubUserService, gitHubGraphQLApiService).ConfigureAwait(false);

			repository_Initial = new Repository(GitHubConstants.GitTrendsRepoName, GitHubConstants.GitTrendsRepoName, 1, GitHubConstants.GitTrendsRepoOwner,
												GitHubConstants.GitTrendsAvatarUrl, 1, 2, 3, "https://github.com/brminnick/gittrends", false, DateTimeOffset.UtcNow, RepositoryPermission.ADMIN);

			//Act
			wasScheduledSuccessfully_First = backgroundFetchService.TryScheduleRetryRepositoriesViewsClonesStars(repository_Initial);
			wasScheduledSuccessfully_Second = backgroundFetchService.TryScheduleRetryRepositoriesViewsClonesStars(repository_Initial);

			repository_Final = await scheduleRetryRepositoriesViewsClonesStarsCompletedTCS.Task.ConfigureAwait(false);
			repository_Database = await repositoryDatabase.GetRepository(repository_Initial.Url).ConfigureAwait(false) ?? throw new NullReferenceException();

			//Assert
			Assert.IsTrue(wasScheduledSuccessfully_First);
			Assert.IsFalse(wasScheduledSuccessfully_Second);

			Assert.IsFalse(repository_Initial.ContainsViewsClonesStarsData);
			Assert.IsTrue(repository_Final.ContainsViewsClonesStarsData);
			Assert.IsTrue(repository_Database.ContainsViewsClonesStarsData);

			Assert.IsNull(repository_Initial.DailyClonesList);
			Assert.IsNull(repository_Initial.DailyViewsList);
			Assert.IsNull(repository_Initial.StarredAt);
			Assert.IsNull(repository_Initial.TotalClones);
			Assert.IsNull(repository_Initial.TotalUniqueClones);
			Assert.IsNull(repository_Initial.TotalUniqueViews);
			Assert.IsNull(repository_Initial.TotalViews);

			Assert.IsNotNull(repository_Final.StarredAt);
			Assert.IsNotNull(repository_Final.DailyClonesList);
			Assert.IsNotNull(repository_Final.DailyViewsList);
			Assert.IsNotNull(repository_Final.TotalClones);
			Assert.IsNotNull(repository_Final.TotalUniqueClones);
			Assert.IsNotNull(repository_Final.TotalUniqueViews);
			Assert.IsNotNull(repository_Final.TotalViews);

			Assert.AreEqual(repository_Initial.Name, repository_Final.Name);
			Assert.AreEqual(repository_Initial.Description, repository_Final.Description);
			Assert.AreEqual(repository_Initial.ForkCount, repository_Final.ForkCount);
			Assert.AreEqual(repository_Initial.IsFavorite, repository_Final.IsFavorite);
			Assert.AreEqual(repository_Initial.IsFork, repository_Final.IsFork);
			Assert.AreEqual(repository_Initial.IssuesCount, repository_Final.IssuesCount);

			Assert.IsNotNull(repository_Database.StarredAt);
			Assert.IsNotNull(repository_Database.DailyClonesList);
			Assert.IsNotNull(repository_Database.DailyViewsList);
			Assert.IsNotNull(repository_Database.TotalClones);
			Assert.IsNotNull(repository_Database.TotalUniqueClones);
			Assert.IsNotNull(repository_Database.TotalUniqueViews);
			Assert.IsNotNull(repository_Database.TotalViews);

			Assert.AreEqual(repository_Final.Name, repository_Database.Name);
			Assert.AreEqual(repository_Final.Description, repository_Database.Description);
			Assert.AreEqual(repository_Final.ForkCount, repository_Database.ForkCount);
			Assert.AreEqual(repository_Final.IsFavorite, repository_Database.IsFavorite);
			Assert.AreEqual(repository_Final.IsFork, repository_Database.IsFork);
			Assert.AreEqual(repository_Final.IssuesCount, repository_Database.IssuesCount);
			Assert.AreEqual(repository_Final.IsTrending, repository_Database.IsTrending);

			void HandleScheduleRetryRepositoriesViewsClonesStarsCompleted(object? sender, Repository e)
			{
				BackgroundFetchService.ScheduleRetryRepositoriesViewsClonesStarsCompleted -= HandleScheduleRetryRepositoriesViewsClonesStarsCompleted;
				scheduleRetryRepositoriesViewsClonesStarsCompletedTCS.SetResult(e);
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

			await AuthenticateUser(gitHubUserService, gitHubGraphQLApiService).ConfigureAwait(false);

			repository_Initial = new Repository(GitHubConstants.GitTrendsRepoName, GitHubConstants.GitTrendsRepoName, 1, GitHubConstants.GitTrendsRepoOwner,
												GitHubConstants.GitTrendsAvatarUrl, 1, 2, 3, "https://github.com/brminnick/gittrends", false, DateTimeOffset.UtcNow, RepositoryPermission.ADMIN);

			//Act
			wasScheduledSuccessfully_First = backgroundFetchService.TryScheduleRetryRepositoriesStars(repository_Initial);
			wasScheduledSuccessfully_Second = backgroundFetchService.TryScheduleRetryRepositoriesStars(repository_Initial);

			repository_Final = await scheduleRetryRepositoriesStarsCompletedTCS.Task.ConfigureAwait(false);
			repository_Database = await repositoryDatabase.GetRepository(repository_Initial.Url).ConfigureAwait(false) ?? throw new NullReferenceException();

			//Assert
			Assert.IsTrue(wasScheduledSuccessfully_First);
			Assert.IsFalse(wasScheduledSuccessfully_Second);

			Assert.IsFalse(repository_Initial.ContainsViewsClonesData);
			Assert.IsFalse(repository_Initial.ContainsViewsClonesStarsData);
			Assert.IsFalse(repository_Final.ContainsViewsClonesData);
			Assert.IsFalse(repository_Final.ContainsViewsClonesStarsData);
			Assert.IsFalse(repository_Database.ContainsViewsClonesData);
			Assert.IsFalse(repository_Database.ContainsViewsClonesStarsData);

			Assert.IsNull(repository_Initial.DailyClonesList);
			Assert.IsNull(repository_Initial.DailyViewsList);
			Assert.IsNull(repository_Initial.StarredAt);
			Assert.IsNull(repository_Initial.TotalClones);
			Assert.IsNull(repository_Initial.TotalUniqueClones);
			Assert.IsNull(repository_Initial.TotalUniqueViews);
			Assert.IsNull(repository_Initial.TotalViews);

			Assert.IsNotNull(repository_Final.StarredAt);
			Assert.IsNotEmpty(repository_Final.StarredAt ?? Array.Empty<DateTimeOffset>());
			Assert.IsNull(repository_Final.DailyClonesList);
			Assert.IsNull(repository_Final.DailyViewsList);
			Assert.IsNull(repository_Final.TotalClones);
			Assert.IsNull(repository_Final.TotalUniqueClones);
			Assert.IsNull(repository_Final.TotalUniqueViews);
			Assert.IsNull(repository_Final.TotalViews);

			Assert.AreEqual(repository_Initial.Name, repository_Final.Name);
			Assert.AreEqual(repository_Initial.Description, repository_Final.Description);
			Assert.AreEqual(repository_Initial.ForkCount, repository_Final.ForkCount);
			Assert.AreEqual(repository_Initial.IsFavorite, repository_Final.IsFavorite);
			Assert.AreEqual(repository_Initial.IsFork, repository_Final.IsFork);
			Assert.AreEqual(repository_Initial.IssuesCount, repository_Final.IssuesCount);

			Assert.IsNotNull(repository_Database.StarredAt);
			Assert.IsNotEmpty(repository_Database.StarredAt ?? Array.Empty<DateTimeOffset>());
			Assert.IsNull(repository_Database.DailyClonesList);
			Assert.IsNull(repository_Database.DailyViewsList);
			Assert.IsNull(repository_Database.TotalClones);
			Assert.IsNull(repository_Database.TotalUniqueClones);
			Assert.IsNull(repository_Database.TotalUniqueViews);
			Assert.IsNull(repository_Database.TotalViews);

			Assert.AreEqual(repository_Final.Name, repository_Database.Name);
			Assert.AreEqual(repository_Final.Description, repository_Database.Description);
			Assert.AreEqual(repository_Final.ForkCount, repository_Database.ForkCount);
			Assert.AreEqual(repository_Final.IsFavorite, repository_Database.IsFavorite);
			Assert.AreEqual(repository_Final.IsFork, repository_Database.IsFork);
			Assert.AreEqual(repository_Final.IssuesCount, repository_Database.IssuesCount);
			Assert.AreEqual(repository_Final.IsTrending, repository_Database.IsTrending);

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

			await AuthenticateUser(gitHubUserService, gitHubGraphQLApiService).ConfigureAwait(false);

			//Act
			wasScheduledSuccessfully_First = backgroundFetchService.TryScheduleRetryOrganizationsRepositories(organizationName_Initial);
			wasScheduledSuccessfully_Second = backgroundFetchService.TryScheduleRetryOrganizationsRepositories(organizationName_Initial);

			organizationName_Final = await scheduleRetryOrganizationsRepositoriesCompletedTCS.Task.ConfigureAwait(false);

			repository_Final = await scheduleRetryRepositoriesViewsClonesCompletedTCS.Task.ConfigureAwait(false);
			repository_Database = await repositoryDatabase.GetRepository(repository_Final.Url).ConfigureAwait(false) ?? throw new NullReferenceException();

			//Assert
			Assert.IsTrue(wasScheduledSuccessfully_First);
			Assert.IsFalse(wasScheduledSuccessfully_Second);

			Assert.AreEqual(organizationName_Initial, organizationName_Final);

			Assert.IsTrue(repository_Final.ContainsViewsClonesStarsData);
			Assert.IsTrue(repository_Database.ContainsViewsClonesStarsData);

			Assert.IsNotNull(repository_Final.DailyClonesList);
			Assert.IsNotNull(repository_Final.DailyViewsList);
			Assert.IsNotNull(repository_Final.StarredAt);
			Assert.IsNotNull(repository_Final.TotalClones);
			Assert.IsNotNull(repository_Final.TotalUniqueClones);
			Assert.IsNotNull(repository_Final.TotalUniqueViews);
			Assert.IsNotNull(repository_Final.TotalViews);

			Assert.IsNotNull(repository_Database.DailyClonesList);
			Assert.IsNotNull(repository_Database.DailyViewsList);
			Assert.IsNotNull(repository_Database.StarredAt);
			Assert.IsNotNull(repository_Database.TotalClones);
			Assert.IsNotNull(repository_Database.TotalUniqueClones);
			Assert.IsNotNull(repository_Database.TotalUniqueViews);
			Assert.IsNotNull(repository_Database.TotalViews);

			Assert.AreEqual(repository_Final.Name, repository_Database.Name);
			Assert.AreEqual(repository_Final.Description, repository_Database.Description);
			Assert.AreEqual(repository_Final.ForkCount, repository_Database.ForkCount);
			Assert.AreEqual(repository_Final.IsFavorite, repository_Database.IsFavorite);
			Assert.AreEqual(repository_Final.IsFork, repository_Database.IsFork);
			Assert.AreEqual(repository_Final.IssuesCount, repository_Database.IssuesCount);
			Assert.AreEqual(repository_Final.IsTrending, repository_Database.IsTrending);

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
												GitHubConstants.GitTrendsAvatarUrl, 1, 2, 3, "https://github.com/brminnick/gittrends", false, DateTimeOffset.UtcNow, RepositoryPermission.ADMIN);

			var mobileReferringSiteRetrievedTCS = new TaskCompletionSource<MobileReferringSiteModel>();
			var scheduleRetryGetReferringSiteCompletedTCS = new TaskCompletionSource<Repository>();

			BackgroundFetchService.MobileReferringSiteRetrieved += HandleMobileReferringSiteRetrieved;
			BackgroundFetchService.ScheduleRetryGetReferringSitesCompleted += HandleScheduleRetryGetReferringSitesCompleted;

			var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
			var referringSitesDatabase = ServiceCollection.ServiceProvider.GetRequiredService<ReferringSitesDatabase>();
			var backgroundFetchService = ServiceCollection.ServiceProvider.GetRequiredService<BackgroundFetchService>();
			var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();
			var referringSitesViewModel = ServiceCollection.ServiceProvider.GetRequiredService<ReferringSitesViewModel>();

			await AuthenticateUser(gitHubUserService, gitHubGraphQLApiService).ConfigureAwait(false);

			//Act
			mobileReferringSitesList_Initial = referringSitesViewModel.MobileReferringSitesList;
			wasScheduledSuccessfully_First = backgroundFetchService.TryScheduleRetryGetReferringSites(repository_Initial);
			wasScheduledSuccessfully_Second = backgroundFetchService.TryScheduleRetryGetReferringSites(repository_Initial);

			mobileReferringSiteModel = await mobileReferringSiteRetrievedTCS.Task.ConfigureAwait(false);
			repository_Final = await scheduleRetryGetReferringSiteCompletedTCS.Task.ConfigureAwait(false);

			mobileReferringSitesList_Final = referringSitesViewModel.MobileReferringSitesList;

			IReadOnlyList<MobileReferringSiteModel> mobileReferringSiteModelsFromDatabase = await referringSitesDatabase.GetReferringSites(repository_Initial.Url.ToString()).ConfigureAwait(false);
			mobileReferringSiteModel_Database = mobileReferringSiteModelsFromDatabase.Single(x => x.ReferrerUri is not null && x.ReferrerUri == mobileReferringSiteModel.ReferrerUri);

			// Assert
			Assert.IsTrue(wasScheduledSuccessfully_First);
			Assert.IsFalse(wasScheduledSuccessfully_Second);

			Assert.IsNotNull(mobileReferringSiteModel.FavIcon);
			Assert.IsNotNull(mobileReferringSiteModel.FavIconImageUrl);
			if (string.Empty == mobileReferringSiteModel.FavIconImageUrl)
				Assert.AreEqual(FavIconService.DefaultFavIcon, ((Xamarin.Forms.FileImageSource?)mobileReferringSiteModel.FavIcon)?.File);
			else
				Assert.IsNotEmpty(mobileReferringSiteModel.FavIconImageUrl);
			Assert.IsTrue(mobileReferringSiteModel.IsReferrerUriValid);
			Assert.IsNotNull(mobileReferringSiteModel.Referrer);
			Assert.IsNotEmpty(mobileReferringSiteModel.Referrer);
			Assert.IsNotNull(mobileReferringSiteModel.ReferrerUri);
			Assert.IsTrue(Uri.IsWellFormedUriString(mobileReferringSiteModel.ReferrerUri?.ToString(), UriKind.Absolute));
			Assert.Greater(mobileReferringSiteModel.TotalCount, 0);
			Assert.Greater(mobileReferringSiteModel.TotalUniqueCount, 0);

			Assert.AreEqual(mobileReferringSiteModel.ToString(), mobileReferringSiteModel_Database.ToString());

			Assert.AreEqual(mobileReferringSiteModel.DownloadedAt, mobileReferringSiteModel_Database.DownloadedAt);
			Assert.AreEqual(mobileReferringSiteModel.FavIcon?.ToString(), mobileReferringSiteModel_Database.FavIcon?.ToString());
			Assert.AreEqual(mobileReferringSiteModel.FavIconImageUrl, mobileReferringSiteModel_Database.FavIconImageUrl);
			Assert.AreEqual(mobileReferringSiteModel.IsReferrerUriValid, mobileReferringSiteModel_Database.IsReferrerUriValid);
			Assert.AreEqual(mobileReferringSiteModel.Referrer, mobileReferringSiteModel_Database.Referrer);
			Assert.AreEqual(mobileReferringSiteModel.ReferrerUri?.ToString(), mobileReferringSiteModel_Database.ReferrerUri?.ToString());
			Assert.AreEqual(mobileReferringSiteModel.TotalCount, mobileReferringSiteModel_Database.TotalCount);
			Assert.AreEqual(mobileReferringSiteModel.TotalUniqueCount, mobileReferringSiteModel_Database.TotalUniqueCount);

			Assert.AreEqual(repository_Initial.ToString(), repository_Final.ToString());

			Assert.AreEqual(repository_Initial.ContainsViewsClonesStarsData, repository_Final.ContainsViewsClonesStarsData);
			Assert.AreEqual(repository_Initial.DailyClonesList?.Count, repository_Final.DailyClonesList?.Count);
			Assert.AreEqual(repository_Initial.DailyViewsList?.Count, repository_Final.DailyViewsList?.Count);
			Assert.AreEqual(repository_Initial.DataDownloadedAt, repository_Final.DataDownloadedAt);
			Assert.AreEqual(repository_Initial.Description, repository_Final.Description);
			Assert.AreEqual(repository_Initial.ForkCount, repository_Final.ForkCount);
			Assert.AreEqual(repository_Initial.IsFavorite, repository_Final.IsFavorite);
			Assert.AreEqual(repository_Initial.IsFork, repository_Final.IsFork);
			Assert.AreEqual(repository_Initial.IssuesCount, repository_Final.IssuesCount);
			Assert.AreEqual(repository_Initial.IsTrending, repository_Final.IsTrending);
			Assert.AreEqual(repository_Initial.Name, repository_Final.Name);
			Assert.AreEqual(repository_Initial.OwnerAvatarUrl, repository_Final.OwnerAvatarUrl);
			Assert.AreEqual(repository_Initial.OwnerLogin, repository_Final.OwnerLogin);
			Assert.AreEqual(repository_Initial.Permission, repository_Final.Permission);
			Assert.AreEqual(repository_Initial.StarCount, repository_Final.StarCount);
			Assert.AreEqual(repository_Initial.StarredAt, repository_Final.StarredAt);
			Assert.AreEqual(repository_Initial.TotalClones, repository_Final.TotalClones);
			Assert.AreEqual(repository_Initial.TotalUniqueClones, repository_Final.TotalUniqueClones);
			Assert.AreEqual(repository_Initial.TotalUniqueViews, repository_Final.TotalUniqueViews);
			Assert.AreEqual(repository_Initial.TotalViews, repository_Final.TotalViews);
			Assert.AreEqual(repository_Initial.Url, repository_Final.Url);
			Assert.AreEqual(repository_Initial.WatchersCount, repository_Final.WatchersCount);

			Assert.AreEqual(0, mobileReferringSitesList_Initial.Count);
			Assert.Greater(mobileReferringSitesList_Final.Count, mobileReferringSitesList_Initial.Count);
			foreach (var mobileReferringSite in mobileReferringSitesList_Final)
			{
				Assert.IsNotNull(mobileReferringSite.FavIcon);
				Assert.IsNotNull(mobileReferringSite.FavIconImageUrl);
				if (string.Empty == mobileReferringSite.FavIconImageUrl)
					Assert.AreEqual(FavIconService.DefaultFavIcon, ((Xamarin.Forms.FileImageSource?)mobileReferringSite.FavIcon)?.File);
				else
					Assert.IsNotEmpty(mobileReferringSite.FavIconImageUrl);
				Assert.IsTrue(mobileReferringSite.IsReferrerUriValid);
				Assert.IsNotNull(mobileReferringSite.Referrer);
				Assert.IsNotEmpty(mobileReferringSite.Referrer);
				Assert.IsNotNull(mobileReferringSite.ReferrerUri);
				Assert.IsTrue(Uri.IsWellFormedUriString(mobileReferringSite.ReferrerUri?.ToString(), UriKind.Absolute));
				Assert.Greater(mobileReferringSite.TotalCount, 0);
				Assert.Greater(mobileReferringSite.TotalUniqueCount, 0);
			}

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

			await repositoryDatabase.SaveRepository(expiredRepository_Initial).ConfigureAwait(false);
			await repositoryDatabase.SaveRepository(unexpiredRepository_Initial).ConfigureAwait(false);

			await referringSitesDatabase.SaveReferringSite(expiredReferringSite_Initial, expiredRepository_Initial.Url).ConfigureAwait(false);
			await referringSitesDatabase.SaveReferringSite(unexpiredReferringSite_Initial, unexpiredRepository_Initial.Url).ConfigureAwait(false);

			//Act
			repositoryDatabaseCount_Initial = await getRepositoryDatabaseCount(repositoryDatabase).ConfigureAwait(false);
			referringSitesDatabaseCount_Initial = await getReferringSitesDatabaseCount(referringSitesDatabase, expiredRepository_Initial.Url, unexpiredRepository_Initial.Url).ConfigureAwait(false);

			wasScheduledSuccessfully_First = backgroundFetchService.TryScheduleCleanUpDatabase();
			wasScheduledSuccessfully_Second = backgroundFetchService.TryScheduleCleanUpDatabase();

			await databaseCleanupCompletedTCS.Task.ConfigureAwait(false);

			repositoryDatabaseCount_Final = await getRepositoryDatabaseCount(repositoryDatabase).ConfigureAwait(false);
			referringSitesDatabaseCount_Final = await getReferringSitesDatabaseCount(referringSitesDatabase, expiredRepository_Initial.Url, unexpiredRepository_Initial.Url).ConfigureAwait(false);

			var finalRepositories = await repositoryDatabase.GetRepositories().ConfigureAwait(false);
			expiredRepository_Final = finalRepositories.First(x => x.DataDownloadedAt == expiredRepository_Initial.DataDownloadedAt);
			unexpiredRepository_Final = finalRepositories.First(x => x.DataDownloadedAt == unexpiredRepository_Initial.DataDownloadedAt);

			//Assert
			Assert.IsTrue(wasScheduledSuccessfully_First);
			Assert.IsFalse(wasScheduledSuccessfully_Second);

			Assert.AreEqual(2, repositoryDatabaseCount_Initial);
			Assert.AreEqual(2, referringSitesDatabaseCount_Initial);

			Assert.AreEqual(repositoryDatabaseCount_Initial, repositoryDatabaseCount_Final);

			Assert.Greater(expiredRepository_Initial.DailyClonesList.Sum(x => x.TotalClones), 1);
			Assert.Greater(expiredRepository_Initial.DailyClonesList.Sum(x => x.TotalUniqueClones), 1);
			Assert.Greater(expiredRepository_Initial.DailyViewsList.Sum(x => x.TotalViews), 1);
			Assert.Greater(expiredRepository_Initial.DailyViewsList.Sum(x => x.TotalUniqueViews), 1);

			Assert.IsNull(expiredRepository_Final.DailyClonesList);
			Assert.IsNull(expiredRepository_Final.DailyViewsList);

			Assert.AreEqual(unexpiredRepository_Initial.DailyClonesList.Sum(x => x.TotalClones), unexpiredRepository_Final.DailyClonesList.Sum(x => x.TotalClones));
			Assert.AreEqual(unexpiredRepository_Initial.DailyClonesList.Sum(x => x.TotalUniqueClones), unexpiredRepository_Final.DailyClonesList.Sum(x => x.TotalUniqueClones));
			Assert.AreEqual(unexpiredRepository_Initial.DailyViewsList.Sum(x => x.TotalViews), unexpiredRepository_Final.DailyViewsList.Sum(x => x.TotalViews));
			Assert.AreEqual(unexpiredRepository_Initial.DailyViewsList.Sum(x => x.TotalUniqueViews), unexpiredRepository_Final.DailyViewsList.Sum(x => x.TotalUniqueViews));

			Assert.AreEqual(2, repositoryDatabaseCount_Final);
			Assert.AreEqual(1, referringSitesDatabaseCount_Final);

			Assert.IsTrue(unexpiredRepository_Initial.IsFavorite);
			Assert.IsTrue(unexpiredRepository_Final.IsFavorite);

			static async Task<int> getRepositoryDatabaseCount(RepositoryDatabase repositoryDatabase)
			{
				var repositories = await repositoryDatabase.GetRepositories().ConfigureAwait(false);
				return repositories.Count;
			}

			static async Task<int> getReferringSitesDatabaseCount(ReferringSitesDatabase referringSitesDatabase, params string[] repositoryUrl)
			{
				var referringSitesCount = 0;

				foreach (var url in repositoryUrl)
				{
					var referringSites = await referringSitesDatabase.GetReferringSites(url).ConfigureAwait(false);
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
			Assert.IsFalse(result);

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
			await gitHubAuthenticationService.ActivateDemoUser().ConfigureAwait(false);

			var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
			var backgroundFetchService = ServiceCollection.ServiceProvider.GetRequiredService<BackgroundFetchService>();

			//Act
			backgroundFetchService.TryScheduleNotifyTrendingRepositories(CancellationToken.None);

			var result = await scheduleNotifyTrendingRepositoriesCompletedTCS.Task.ConfigureAwait(false);

			//Assert
			Assert.IsTrue(gitHubUserService.IsDemoUser);
			Assert.IsFalse(gitHubUserService.IsAuthenticated);
			Assert.IsFalse(result);

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

			await AuthenticateUser(gitHubUserService, gitHubGraphQLApiService).ConfigureAwait(false);

			//Act
			wasScheduledSuccessfully_First = backgroundFetchService.TryScheduleNotifyTrendingRepositories(CancellationToken.None);
			wasScheduledSuccessfully_Second = backgroundFetchService.TryScheduleNotifyTrendingRepositories(CancellationToken.None);

			var result = await scheduleNotifyTrendingRepositoriesCompletedTCS.Task.ConfigureAwait(false);

			//Assert
			Assert.IsTrue(wasScheduledSuccessfully_First);
			Assert.IsFalse(wasScheduledSuccessfully_Second);

			Assert.IsFalse(gitHubUserService.IsDemoUser);
			Assert.IsTrue(gitHubUserService.IsAuthenticated);
			Assert.IsTrue(result);

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
				var uniqeCount = count / 2; //Ensures uniqueCount is always less than count

				starredAtList.Add(DemoDataConstants.GetRandomDate());
				dailyViewsList.Add(new DailyViewsModel(downloadedAt.Subtract(TimeSpan.FromDays(i)), count, uniqeCount));
				dailyClonesList.Add(new DailyClonesModel(downloadedAt.Subtract(TimeSpan.FromDays(i)), count, uniqeCount));
			}

			return new Repository($"Repository " + DemoDataConstants.GetRandomText(), DemoDataConstants.GetRandomText(), DemoDataConstants.GetRandomNumber(),
														DemoUserConstants.Alias, GitHubConstants.GitTrendsAvatarUrl, DemoDataConstants.GetRandomNumber(), DemoDataConstants.GetRandomNumber(), starredAtList.Count,
														repositoryUrl, false, downloadedAt, RepositoryPermission.ADMIN, true, dailyViewsList, dailyClonesList, starredAtList);
		}

		static MobileReferringSiteModel CreateMobileReferringSite(DateTimeOffset downloadedAt, string referrer)
		{
			return new MobileReferringSiteModel(new ReferringSiteModel(DemoDataConstants.GetRandomNumber(),
												DemoDataConstants.GetRandomNumber(),
												referrer, downloadedAt));
		}
	}
}