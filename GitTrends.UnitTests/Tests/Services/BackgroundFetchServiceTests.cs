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
        const string _gitTrendsAvatarUrl = "https://avatars3.githubusercontent.com/u/61480020?s=400&u=b1a900b5fa1ede22af9d2d9bfd6c49a072e659ba&v=4";

        [Test]
        public async Task ScheduleRetryRepositoriesViewsClonesTest_AuthenticatedUser()
        {
            //Arrange
            Repository repository_Initial, repository_Final, repository_Database;

            var scheduleRetryRepositoriesViewsClonesCompletedTCS = new TaskCompletionSource<Repository>();
            BackgroundFetchService.ScheduleRetryRepositoriesViewsClonesCompleted += HandleScheduleRetryRepositoriesViewsClonesCompleted;

            var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
            var repositoryDatabase = ServiceCollection.ServiceProvider.GetRequiredService<RepositoryDatabase>();
            var backgroundFetchService = ServiceCollection.ServiceProvider.GetRequiredService<BackgroundFetchService>();
            var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();

            await AuthenticateUser(gitHubUserService, gitHubGraphQLApiService).ConfigureAwait(false);

            repository_Initial = new Repository(GitHubConstants.GitTrendsRepoName, GitHubConstants.GitTrendsRepoName, 1, GitHubConstants.GitTrendsRepoOwner,
                                                _gitTrendsAvatarUrl, 1, 2, "https://github.com/brminnick/gittrends", false, DateTimeOffset.UtcNow, RepositoryPermission.ADMIN);

            //Act
            backgroundFetchService.ScheduleRetryRepositoriesViewsClones(repository_Initial);
            repository_Final = await scheduleRetryRepositoriesViewsClonesCompletedTCS.Task.ConfigureAwait(false);
            repository_Database = await repositoryDatabase.GetRepository(repository_Initial.Url).ConfigureAwait(false) ?? throw new NullReferenceException();
             
            //Assert
            Assert.IsFalse(repository_Initial.ContainsTrafficData);
            Assert.IsTrue(repository_Final.ContainsTrafficData);
            Assert.IsTrue(repository_Database.ContainsTrafficData);

            Assert.IsNull(repository_Initial.DailyClonesList);
            Assert.IsNull(repository_Initial.DailyViewsList);
            Assert.IsNull(repository_Initial.StarredAt);
            Assert.IsNull(repository_Initial.TotalClones);
            Assert.IsNull(repository_Initial.TotalUniqueClones);
            Assert.IsNull(repository_Initial.TotalUniqueViews);
            Assert.IsNull(repository_Initial.TotalViews);

            Assert.IsNotNull(repository_Final.DailyClonesList);
            Assert.IsNotNull(repository_Final.DailyViewsList);
            Assert.IsNotNull(repository_Final.StarredAt);
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
            Assert.AreEqual(repository_Initial.IsTrending, repository_Final.IsTrending);

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

            void HandleScheduleRetryRepositoriesViewsClonesCompleted(object? sender, Repository e)
            {
                BackgroundFetchService.ScheduleRetryRepositoriesViewsClonesCompleted -= HandleScheduleRetryRepositoriesViewsClonesCompleted;
                scheduleRetryRepositoriesViewsClonesCompletedTCS.SetResult(e);
            }
        }

        [Test]
        public async Task ScheduleRetryOrganizationsRepositoriesTest_AuthenticatedUser()
        {
            //Arrange
            string organizationName_Initial = nameof(GitTrends), organizationName_Final;
            Repository repository_Final, repository_Database;

            var scheduleRetryOrganizationsRepositoriesCompletedTCS = new TaskCompletionSource<string>();
            var scheduleRetryRepositoriesViewsClonesCompletedTCS = new TaskCompletionSource<Repository>();
            BackgroundFetchService.ScheduleRetryRepositoriesViewsClonesCompleted += HandleScheduleRetryRepositoriesViewsClonesCompleted;
            BackgroundFetchService.ScheduleRetryOrganizationsRepositoriesCompleted += HandleScheduleRetryOrganizationsRepositoriesCompleted;

            var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
            var repositoryDatabase = ServiceCollection.ServiceProvider.GetRequiredService<RepositoryDatabase>();
            var backgroundFetchService = ServiceCollection.ServiceProvider.GetRequiredService<BackgroundFetchService>();
            var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();

            await AuthenticateUser(gitHubUserService, gitHubGraphQLApiService).ConfigureAwait(false);

            //Act
            backgroundFetchService.ScheduleRetryOrganizationsRepositories(organizationName_Initial);
            organizationName_Final = await scheduleRetryOrganizationsRepositoriesCompletedTCS.Task.ConfigureAwait(false);

            repository_Final = await scheduleRetryRepositoriesViewsClonesCompletedTCS.Task.ConfigureAwait(false);
            repository_Database = await repositoryDatabase.GetRepository(repository_Final.Url).ConfigureAwait(false) ?? throw new NullReferenceException();

            //Assert
            Assert.AreEqual(organizationName_Initial, organizationName_Final);

            Assert.IsTrue(repository_Final.ContainsTrafficData);
            Assert.IsTrue(repository_Database.ContainsTrafficData);

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

            void HandleScheduleRetryRepositoriesViewsClonesCompleted(object? sender, Repository e)
            {
                BackgroundFetchService.ScheduleRetryRepositoriesViewsClonesCompleted -= HandleScheduleRetryRepositoriesViewsClonesCompleted;
                scheduleRetryRepositoriesViewsClonesCompletedTCS.SetResult(e);
            }

            void HandleScheduleRetryOrganizationsRepositoriesCompleted(object? sender, string e)
            {
                BackgroundFetchService.ScheduleRetryOrganizationsRepositoriesCompleted -= HandleScheduleRetryOrganizationsRepositoriesCompleted;
                scheduleRetryOrganizationsRepositoriesCompletedTCS.SetResult(e);
            }
        }

        [Test]
        public async Task CleanUpDatabaseTest()
        {
            //Arrange
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

            backgroundFetchService.ScheduleCleanUpDatabase();
            await databaseCleanupCompletedTCS.Task.ConfigureAwait(false);

            repositoryDatabaseCount_Final = await getRepositoryDatabaseCount(repositoryDatabase).ConfigureAwait(false);
            referringSitesDatabaseCount_Final = await getReferringSitesDatabaseCount(referringSitesDatabase, expiredRepository_Initial.Url, unexpiredRepository_Initial.Url).ConfigureAwait(false);

            var finalRepositories = await repositoryDatabase.GetRepositories().ConfigureAwait(false);
            expiredRepository_Final = finalRepositories.First(x => x.DataDownloadedAt == expiredRepository_Initial.DataDownloadedAt);
            unexpiredRepository_Final = finalRepositories.First(x => x.DataDownloadedAt == unexpiredRepository_Initial.DataDownloadedAt);

            //Assert
            Assert.AreEqual(2, repositoryDatabaseCount_Initial);
            Assert.AreEqual(2, referringSitesDatabaseCount_Initial);

            Assert.AreEqual(repositoryDatabaseCount_Initial, repositoryDatabaseCount_Final);

            Assert.Greater(expiredRepository_Initial.DailyClonesList.Sum(x => x.TotalClones), 1);
            Assert.Greater(expiredRepository_Initial.DailyClonesList.Sum(x => x.TotalUniqueClones), 1);
            Assert.Greater(expiredRepository_Initial.DailyViewsList.Sum(x => x.TotalViews), 1);
            Assert.Greater(expiredRepository_Initial.DailyViewsList.Sum(x => x.TotalUniqueViews), 1);

            Assert.AreEqual(0, expiredRepository_Final.DailyClonesList.Sum(x => x.TotalClones));
            Assert.AreEqual(0, expiredRepository_Final.DailyClonesList.Sum(x => x.TotalUniqueClones));
            Assert.AreEqual(0, expiredRepository_Final.DailyViewsList.Sum(x => x.TotalViews));
            Assert.AreEqual(0, expiredRepository_Final.DailyViewsList.Sum(x => x.TotalUniqueViews));

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
            backgroundFetchService.ScheduleNotifyTrendingRepositories(CancellationToken.None);
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
            backgroundFetchService.ScheduleNotifyTrendingRepositories(CancellationToken.None);
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
            var scheduleNotifyTrendingRepositoriesCompletedTCS = new TaskCompletionSource<bool>();
            BackgroundFetchService.ScheduleNotifyTrendingRepositoriesCompleted += HandleScheduleNotifyTrendingRepositoriesCompleted;

            var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
            var backgroundFetchService = ServiceCollection.ServiceProvider.GetRequiredService<BackgroundFetchService>();
            var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();

            await AuthenticateUser(gitHubUserService, gitHubGraphQLApiService).ConfigureAwait(false);

            //Act
            backgroundFetchService.ScheduleNotifyTrendingRepositories(CancellationToken.None);
            var result = await scheduleNotifyTrendingRepositoriesCompletedTCS.Task.ConfigureAwait(false);

            //Assert
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
                                                        DemoUserConstants.Alias, _gitTrendsAvatarUrl, DemoDataConstants.GetRandomNumber(), DemoDataConstants.GetRandomNumber(),
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
