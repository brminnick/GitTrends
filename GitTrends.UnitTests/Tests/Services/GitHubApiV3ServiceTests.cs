using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Refit;

namespace GitTrends.UnitTests
{
    class GitHubApiV3ServiceTests : BaseTest
    {
        public override async Task Setup()
        {
            await base.Setup().ConfigureAwait(false);

            var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
            var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();

            await AuthenticateUser(gitHubUserService, gitHubGraphQLApiService).ConfigureAwait(false);
        }

        [Test]
        public async Task GetReferringSitesTest_ValidRepo()
        {
            //Arrange
            var gitHubApiV3Service = ServiceCollection.ServiceProvider.GetRequiredService<GitHubApiV3Service>();

            //Act
            var referringSites = await gitHubApiV3Service.GetReferringSites(GitTrendsRepoOwner, GitTrendsRepoName, CancellationToken.None).ConfigureAwait(false);

            //Assert
            Assert.IsNotNull(referringSites);
            Assert.IsNotEmpty(referringSites);
        }

        [Test]
        public void GetReferringSitesTest_ValidRepo_Unauthenticated()
        {
            //Arrange
            var gitHubApiV3Service = ServiceCollection.ServiceProvider.GetRequiredService<GitHubApiV3Service>();
            var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();

            gitHubUserService.InvalidateToken();

            //Act
            var exception = Assert.ThrowsAsync<ApiException>(async () => await gitHubApiV3Service.GetReferringSites(GitTrendsRepoOwner, GitTrendsRepoName, CancellationToken.None).ConfigureAwait(false));

            //Assert
            Assert.AreEqual(exception.StatusCode, HttpStatusCode.Forbidden);
        }

        [Test]
        public void GetReferringSitesTest_InvalidRepo()
        {
            //Arrange
            var gitHubApiV3Service = ServiceCollection.ServiceProvider.GetRequiredService<GitHubApiV3Service>();

            //Act
            var exception = Assert.ThrowsAsync<ApiException>(async () => await gitHubApiV3Service.GetReferringSites("xamarin", GitTrendsRepoName, CancellationToken.None).ConfigureAwait(false));

            //Assert
            Assert.AreEqual(HttpStatusCode.NotFound, exception.StatusCode);
        }

        [Test]
        public async Task GetRepositoryCloneStatisticsTest_ValidRepo()
        {
            //Arrange
            var gitHubApiV3Service = ServiceCollection.ServiceProvider.GetRequiredService<GitHubApiV3Service>();

            //Act
            var clones = await gitHubApiV3Service.GetRepositoryCloneStatistics(GitTrendsRepoOwner, GitTrendsRepoName, CancellationToken.None).ConfigureAwait(false);

            //Assert
            Assert.IsNotNull(clones);
            Assert.IsNotNull(clones.DailyClonesList);
            Assert.IsNotEmpty(clones.DailyClonesList);
            Assert.AreEqual(GitTrendsRepoName, clones.RepositoryName);
            Assert.AreEqual(GitTrendsRepoOwner, clones.RepositoryOwner);
        }

        [Test]
        public void GetRepositoryCloneStatisticsTest_InvalidRepo()
        {
            //Arrange
            var gitHubApiV3Service = ServiceCollection.ServiceProvider.GetRequiredService<GitHubApiV3Service>();

            //Act
            var exception = Assert.ThrowsAsync<ApiException>(async () => await gitHubApiV3Service.GetRepositoryCloneStatistics("xamarin", GitTrendsRepoName, CancellationToken.None).ConfigureAwait(false));

            //Assert
            Assert.AreEqual(exception.StatusCode, HttpStatusCode.NotFound);
        }

        [Test]
        public void GetRepositoryCloneStatisticsTest_ValidRepo_Unauthenticated()
        {
            //Arrange
            var gitHubApiV3Service = ServiceCollection.ServiceProvider.GetRequiredService<GitHubApiV3Service>();
            var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();

            gitHubUserService.InvalidateToken();

            //Act
            var exception = Assert.ThrowsAsync<ApiException>(async () => await gitHubApiV3Service.GetRepositoryCloneStatistics(gitHubUserService.Alias, GitTrendsRepoName, CancellationToken.None).ConfigureAwait(false));

            //Assert
            Assert.AreEqual(exception.StatusCode, HttpStatusCode.Forbidden);
        }

        [Test]
        public async Task GetRepositoryViewStatisticsTest_ValidRepo()
        {
            //Arrange
            var gitHubApiV3Service = ServiceCollection.ServiceProvider.GetRequiredService<GitHubApiV3Service>();
            var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();

            //Act
            var views = await gitHubApiV3Service.GetRepositoryViewStatistics(GitTrendsRepoOwner, GitTrendsRepoName, CancellationToken.None).ConfigureAwait(false);

            //Assert
            Assert.IsNotNull(views);
            Assert.IsNotNull(views.DailyViewsList);
            Assert.IsNotEmpty(views.DailyViewsList);
            Assert.AreEqual(GitTrendsRepoName, views.RepositoryName);
            Assert.AreEqual(GitTrendsRepoOwner, views.RepositoryOwner);
        }

        [Test]
        public void GetRepositoryViewStatisticsTest_InvalidRepo()
        {
            //Arrange
            var gitHubApiV3Service = ServiceCollection.ServiceProvider.GetRequiredService<GitHubApiV3Service>();

            //Act
            var exception = Assert.ThrowsAsync<ApiException>(async () => await gitHubApiV3Service.GetRepositoryViewStatistics("xamarin", GitTrendsRepoName, CancellationToken.None).ConfigureAwait(false));

            //Assert
            Assert.AreEqual(HttpStatusCode.NotFound, exception.StatusCode);
        }

        [Test]
        public void GetRepositoryViewStatisticsTest_ValidRepo_Unauthenticated()
        {
            //Arrange
            var gitHubApiV3Service = ServiceCollection.ServiceProvider.GetRequiredService<GitHubApiV3Service>();
            var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();

            gitHubUserService.InvalidateToken();

            //Act
            var exception = Assert.ThrowsAsync<ApiException>(async () => await gitHubApiV3Service.GetRepositoryViewStatistics(GitTrendsRepoOwner, GitTrendsRepoName, CancellationToken.None).ConfigureAwait(false));

            //Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, exception.StatusCode);
        }
    }
}
