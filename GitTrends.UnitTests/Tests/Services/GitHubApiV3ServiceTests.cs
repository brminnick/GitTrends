using System.Net;
using System.Threading;
using System.Threading.Tasks;
using GitTrends.Shared;
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
			var referringSites = await gitHubApiV3Service.GetReferringSites(GitHubConstants.GitTrendsRepoOwner, GitHubConstants.GitTrendsRepoName, CancellationToken.None).ConfigureAwait(false);

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
			var exception = Assert.ThrowsAsync<ApiException>(async () => await gitHubApiV3Service.GetReferringSites(GitHubConstants.GitTrendsRepoOwner, GitHubConstants.GitTrendsRepoName, CancellationToken.None).ConfigureAwait(false));

			//Assert
			Assert.AreEqual(HttpStatusCode.Forbidden, exception?.StatusCode);
		}

		[Test]
		public void GetReferringSitesTest_InvalidRepo()
		{
			//Arrange
			var gitHubApiV3Service = ServiceCollection.ServiceProvider.GetRequiredService<GitHubApiV3Service>();

			//Act
			var exception = Assert.ThrowsAsync<ApiException>(async () => await gitHubApiV3Service.GetReferringSites("xamarin", GitHubConstants.GitTrendsRepoName, CancellationToken.None).ConfigureAwait(false));

			//Assert
			Assert.AreEqual(HttpStatusCode.NotFound, exception?.StatusCode);
		}

		[Test]
		public async Task GetRepositoryCloneStatisticsTest_ValidRepo()
		{
			//Arrange
			var gitHubApiV3Service = ServiceCollection.ServiceProvider.GetRequiredService<GitHubApiV3Service>();

			//Act
			var clones = await gitHubApiV3Service.GetRepositoryCloneStatistics(GitHubConstants.GitTrendsRepoOwner, GitHubConstants.GitTrendsRepoName, CancellationToken.None).ConfigureAwait(false);

			//Assert
			Assert.IsNotNull(clones);
			Assert.IsNotNull(clones.DailyClonesList);
			Assert.IsNotEmpty(clones.DailyClonesList);
			Assert.AreEqual(GitHubConstants.GitTrendsRepoName, clones.RepositoryName);
			Assert.AreEqual(GitHubConstants.GitTrendsRepoOwner, clones.RepositoryOwner);
		}

		[Test]
		public void GetRepositoryCloneStatisticsTest_InvalidRepo()
		{
			//Arrange
			var gitHubApiV3Service = ServiceCollection.ServiceProvider.GetRequiredService<GitHubApiV3Service>();

			//Act
			var exception = Assert.ThrowsAsync<ApiException>(async () => await gitHubApiV3Service.GetRepositoryCloneStatistics("xamarin", GitHubConstants.GitTrendsRepoName, CancellationToken.None).ConfigureAwait(false));

			//Assert
			Assert.AreEqual(HttpStatusCode.NotFound, exception?.StatusCode);
		}

		[Test]
		public void GetRepositoryCloneStatisticsTest_ValidRepo_Unauthenticated()
		{
			//Arrange
			var gitHubApiV3Service = ServiceCollection.ServiceProvider.GetRequiredService<GitHubApiV3Service>();
			var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();

			gitHubUserService.InvalidateToken();

			//Act
			var exception = Assert.ThrowsAsync<ApiException>(async () => await gitHubApiV3Service.GetRepositoryCloneStatistics(gitHubUserService.Alias, GitHubConstants.GitTrendsRepoName, CancellationToken.None).ConfigureAwait(false));

			//Assert
			Assert.AreEqual(HttpStatusCode.Forbidden, exception?.StatusCode);
		}

		[Test]
		public async Task GetRepositoryViewStatisticsTest_ValidRepo()
		{
			//Arrange
			var gitHubApiV3Service = ServiceCollection.ServiceProvider.GetRequiredService<GitHubApiV3Service>();

			//Act
			var views = await gitHubApiV3Service.GetRepositoryViewStatistics(GitHubConstants.GitTrendsRepoOwner, GitHubConstants.GitTrendsRepoName, CancellationToken.None).ConfigureAwait(false);

			//Assert
			Assert.IsNotNull(views);
			Assert.IsNotNull(views.DailyViewsList);
			Assert.IsNotEmpty(views.DailyViewsList);
			Assert.AreEqual(GitHubConstants.GitTrendsRepoName, views.RepositoryName);
			Assert.AreEqual(GitHubConstants.GitTrendsRepoOwner, views.RepositoryOwner);
		}

		[Test]
		public void GetRepositoryViewStatisticsTest_InvalidRepo()
		{
			//Arrange
			var gitHubApiV3Service = ServiceCollection.ServiceProvider.GetRequiredService<GitHubApiV3Service>();

			//Act
			var exception = Assert.ThrowsAsync<ApiException>(async () => await gitHubApiV3Service.GetRepositoryViewStatistics("xamarin", GitHubConstants.GitTrendsRepoName, CancellationToken.None).ConfigureAwait(false));

			//Assert
			Assert.AreEqual(HttpStatusCode.NotFound, exception?.StatusCode);
		}

		[Test]
		public void GetRepositoryViewStatisticsTest_ValidRepo_Unauthenticated()
		{
			//Arrange
			var gitHubApiV3Service = ServiceCollection.ServiceProvider.GetRequiredService<GitHubApiV3Service>();
			var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();

			gitHubUserService.InvalidateToken();

			//Act
			var exception = Assert.ThrowsAsync<ApiException>(async () => await gitHubApiV3Service.GetRepositoryViewStatistics(GitHubConstants.GitTrendsRepoOwner, GitHubConstants.GitTrendsRepoName, CancellationToken.None).ConfigureAwait(false));

			//Assert
			Assert.AreEqual(HttpStatusCode.Forbidden, exception?.StatusCode);
		}
	}
}