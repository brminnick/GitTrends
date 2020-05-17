using System;
using System.Linq;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using GitTrends.Mobile.Shared;
using GitTrends.Shared;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace GitTrends.UnitTests
{
    class BackgroundFetchServiceTests : BaseTest
    {
        public override async Task Setup()
        {
            await base.Setup().ConfigureAwait(false);

            var referringSitesDatabase = ContainerService.Container.GetService<ReferringSitesDatabase>();
            await referringSitesDatabase.DeleteAllData().ConfigureAwait(false);

            var repositoryDatabase = ContainerService.Container.GetService<RepositoryDatabase>();
            await repositoryDatabase.DeleteAllData().ConfigureAwait(false);

            var githubAuthenticationService = ContainerService.Container.GetService<GitHubAuthenticationService>();
            await githubAuthenticationService.ActivateDemoUser().ConfigureAwait(false);
        }

        [Test]
        public async Task CleanUpDatabaseTest()
        {
            //Arrange
            int repositoryDatabaseCount_Initial, repositoryDatabaseCount_Final, referringSitesDatabaseCount_Initial, referringSitesDatabaseCount_Final;
            Repository repository_Final;

            var repository_Initial = CreateExpiredRepository();
            var referringSite = CreateExpiredMobileReferringSite();

            var backgroundFetchService = ContainerService.Container.GetService<BackgroundFetchService>();
            var referringSitesDatabase = ContainerService.Container.GetService<ReferringSitesDatabase>();
            var repositoryDatabase = ContainerService.Container.GetService<RepositoryDatabase>();

            await repositoryDatabase.SaveRepository(repository_Initial).ConfigureAwait(false);
            await referringSitesDatabase.SaveReferringSite(referringSite, repository_Initial.Url).ConfigureAwait(false);

            //Act
            repositoryDatabaseCount_Initial = await getRepositoryDatabaseCount(repositoryDatabase).ConfigureAwait(false);
            referringSitesDatabaseCount_Initial = await getReferringSitesDatabaseCount(referringSitesDatabase, repository_Initial.Url).ConfigureAwait(false);

            await backgroundFetchService.CleanUpDatabase().ConfigureAwait(false);

            repositoryDatabaseCount_Final = await getRepositoryDatabaseCount(repositoryDatabase).ConfigureAwait(false);
            referringSitesDatabaseCount_Final = await getReferringSitesDatabaseCount(referringSitesDatabase, repository_Initial.Url).ConfigureAwait(false);

            var finalRepositories = await repositoryDatabase.GetRepositories().ConfigureAwait(false);
            repository_Final = finalRepositories.First();

            //Assert
            Assert.AreEqual(1, repositoryDatabaseCount_Initial);
            Assert.AreEqual(1, referringSitesDatabaseCount_Initial);

            Assert.AreEqual(repositoryDatabaseCount_Initial, repositoryDatabaseCount_Final);
            Assert.AreNotEqual(repository_Final.DailyClonesList.Count, repository_Initial.DailyClonesList.Count);
            Assert.AreNotEqual(repository_Final.DailyViewsList.Count, repository_Initial.DailyViewsList.Count);

            Assert.AreEqual(0, referringSitesDatabaseCount_Final);


            static async Task<int> getRepositoryDatabaseCount(RepositoryDatabase repositoryDatabase)
            {
                var repositories = await repositoryDatabase.GetRepositories().ConfigureAwait(false);
                return repositories.Count();
            }

            static async Task<int> getReferringSitesDatabaseCount(ReferringSitesDatabase referringSitesDatabase, string repositoryUrl)
            {
                var referringSites = await referringSitesDatabase.GetReferringSites(repositoryUrl).ConfigureAwait(false);
                return referringSites.Count();
            }
        }

        static Repository CreateExpiredRepository()
        {
            const string gitTrendsAvatarUrl = "https://avatars3.githubusercontent.com/u/61480020?s=400&u=b1a900b5fa1ede22af9d2d9bfd6c49a072e659ba&v=4";
            const string gitTrendsRepositoryUrl = "https://github.com/brminnick/GitTrends";

            return new Repository($"Repository " + DemoDataConstants.GetRandomText(), DemoDataConstants.GetRandomText(), DemoDataConstants.GetRandomNumber(),
                                                        new RepositoryOwner(DemoDataConstants.Alias, gitTrendsAvatarUrl),
                                                        new IssuesConnection(DemoDataConstants.GetRandomNumber(), Enumerable.Empty<Issue>()),
                                                        gitTrendsRepositoryUrl, new StarGazers(DemoDataConstants.GetRandomNumber()), false, DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(100)));
        }

        static MobileReferringSiteModel CreateExpiredMobileReferringSite()
        {
            return new MobileReferringSiteModel(new ReferringSiteModel(DemoDataConstants.GetRandomNumber(),
                                                DemoDataConstants.GetRandomNumber(),
                                                "Google", DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(100))));
        }
    }
}
