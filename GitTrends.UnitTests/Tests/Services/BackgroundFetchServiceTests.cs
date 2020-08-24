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
        public async Task CleanUpDatabaseTest()
        {
            //Arrange
            int repositoryDatabaseCount_Initial, repositoryDatabaseCount_Final, referringSitesDatabaseCount_Initial, referringSitesDatabaseCount_Final;
            MobileReferringSiteModel expiredReferringSite_Initial, unexpiredReferringSite_Initial;
            Repository expiredRepository_Initial, unexpiredRepository_Initial, expiredRepository_Final, unexpiredRepository_Final;

            var backgroundFetchService = ServiceCollection.ServiceProvider.GetRequiredService<BackgroundFetchService>();
            var referringSitesDatabase = ServiceCollection.ServiceProvider.GetRequiredService<ReferringSitesDatabase>();
            var repositoryDatabase = ServiceCollection.ServiceProvider.GetRequiredService<RepositoryDatabase>();

            expiredRepository_Initial = CreateExpiredRepository(DateTimeOffset.UtcNow.Subtract(repositoryDatabase.ExpiresAt), "https://github.com/brminnick/gittrends");
            unexpiredRepository_Initial = CreateExpiredRepository(DateTimeOffset.UtcNow, "https://github.com/brminnick/gitstatus");

            expiredReferringSite_Initial = CreateExpiredMobileReferringSite(DateTimeOffset.UtcNow.Subtract(referringSitesDatabase.ExpiresAt), "Google");
            unexpiredReferringSite_Initial = CreateExpiredMobileReferringSite(DateTimeOffset.UtcNow, "codetraveler.io");

            await repositoryDatabase.SaveRepository(expiredRepository_Initial).ConfigureAwait(false);
            await repositoryDatabase.SaveRepository(unexpiredRepository_Initial).ConfigureAwait(false);

            await referringSitesDatabase.SaveReferringSite(expiredReferringSite_Initial, expiredRepository_Initial.Url).ConfigureAwait(false);
            await referringSitesDatabase.SaveReferringSite(unexpiredReferringSite_Initial, unexpiredRepository_Initial.Url).ConfigureAwait(false);

            //Act
            repositoryDatabaseCount_Initial = await getRepositoryDatabaseCount(repositoryDatabase).ConfigureAwait(false);
            referringSitesDatabaseCount_Initial = await getReferringSitesDatabaseCount(referringSitesDatabase, expiredRepository_Initial.Url, unexpiredRepository_Initial.Url).ConfigureAwait(false);

            await backgroundFetchService.CleanUpDatabase().ConfigureAwait(false);

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

            Assert.AreEqual(1, referringSitesDatabaseCount_Final);

            static async Task<int> getRepositoryDatabaseCount(RepositoryDatabase repositoryDatabase)
            {
                var repositories = await repositoryDatabase.GetRepositories().ConfigureAwait(false);
                return repositories.Count();
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
        }

        [Test]
        public async Task NotifyTrendingRepositoriesTest_NotLoggedIn()
        {
            //Arrange
            var backgroundFetchService = ServiceCollection.ServiceProvider.GetRequiredService<BackgroundFetchService>();

            //Act
            var result = await backgroundFetchService.NotifyTrendingRepositories(CancellationToken.None).ConfigureAwait(false);

            //Assert
            Assert.IsFalse(result);
        }

        [Test]
        public async Task NotifyTrendingRepositoriesTest_DemoUser()
        {
            //Arrange
            var gitHubAuthenticationService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubAuthenticationService>();
            await gitHubAuthenticationService.ActivateDemoUser().ConfigureAwait(false);

            var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
            var backgroundFetchService = ServiceCollection.ServiceProvider.GetRequiredService<BackgroundFetchService>();

            //Act
            var result = await backgroundFetchService.NotifyTrendingRepositories(CancellationToken.None).ConfigureAwait(false);

            //Assert
            Assert.IsTrue(gitHubUserService.IsDemoUser);
            Assert.IsTrue(gitHubUserService.IsAuthenticated);
            Assert.IsFalse(result);
        }

        [Test]
        public async Task NotifyTrendingRepositoriesTest_AuthenticatedUser()
        {
            //Arrange
            var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
            var backgroundFetchService = ServiceCollection.ServiceProvider.GetRequiredService<BackgroundFetchService>();
            var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();

            await AuthenticateUser(gitHubUserService, gitHubGraphQLApiService).ConfigureAwait(false);

            //Act
            var result = await backgroundFetchService.NotifyTrendingRepositories(CancellationToken.None).ConfigureAwait(false);

            //Assert
            Assert.IsFalse(gitHubUserService.IsDemoUser);
            Assert.IsTrue(gitHubUserService.IsAuthenticated);
            Assert.IsTrue(result);
        }

        static Repository CreateExpiredRepository(DateTimeOffset downloadedAt, string repositoryUrl)
        {
            const string gitTrendsAvatarUrl = "https://avatars3.githubusercontent.com/u/61480020?s=400&u=b1a900b5fa1ede22af9d2d9bfd6c49a072e659ba&v=4";

            var dailyViewsList = new List<DailyViewsModel>();
            var dailyClonesList = new List<DailyClonesModel>();

            for (int i = 0; i < 14; i++)
            {
                var count = DemoDataConstants.GetRandomNumber();
                var uniqeCount = count / 2; //Ensures uniqueCount is always less than count

                dailyViewsList.Add(new DailyViewsModel(downloadedAt.Subtract(TimeSpan.FromDays(i)), count, uniqeCount));
                dailyClonesList.Add(new DailyClonesModel(downloadedAt.Subtract(TimeSpan.FromDays(i)), count, uniqeCount));
            }

            return new Repository($"Repository " + DemoDataConstants.GetRandomText(), DemoDataConstants.GetRandomText(), DemoDataConstants.GetRandomNumber(),
                                                        new RepositoryOwner(DemoUserConstants.Alias, gitTrendsAvatarUrl),
                                                        new IssuesConnection(DemoDataConstants.GetRandomNumber(), Enumerable.Empty<Issue>()),
                                                        repositoryUrl, new StarGazers(DemoDataConstants.GetRandomNumber()), false, downloadedAt, false, dailyViewsList, dailyClonesList);
        }

        static MobileReferringSiteModel CreateExpiredMobileReferringSite(DateTimeOffset downloadedAt, string referrer)
        {
            return new MobileReferringSiteModel(new ReferringSiteModel(DemoDataConstants.GetRandomNumber(),
                                                DemoDataConstants.GetRandomNumber(),
                                                referrer, downloadedAt));
        }
    }
}
