using System.Net;
using GitTrends.Shared;
using Refit;

namespace GitTrends.UnitTests;

class GitHubApiV3ServiceTests : BaseTest
{
	public override async Task Setup()
	{
		await base.Setup().ConfigureAwait(false);

		var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
		var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();

		await AuthenticateUser(gitHubUserService, gitHubGraphQLApiService, TestCancellationTokenSource.Token).ConfigureAwait(false);
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

		//Act //Assert
		Assert.ThrowsAsync<InvalidOperationException>(async () => await gitHubApiV3Service.GetReferringSites(GitHubConstants.GitTrendsRepoOwner, GitHubConstants.GitTrendsRepoName, CancellationToken.None).ConfigureAwait(false));
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

		//Act //Assert
		Assert.ThrowsAsync<InvalidOperationException>(async () => await gitHubApiV3Service.GetRepositoryCloneStatistics(gitHubUserService.Alias, GitHubConstants.GitTrendsRepoName, CancellationToken.None).ConfigureAwait(false));
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

		//Act //Assert
		Assert.ThrowsAsync<InvalidOperationException>(async () => await gitHubApiV3Service.GetRepositoryViewStatistics(GitHubConstants.GitTrendsRepoOwner, GitHubConstants.GitTrendsRepoName, CancellationToken.None).ConfigureAwait(false));
	}

	[Test]
	public async Task GetStarGazersTest_DemoUser()
	{
		//Arrange
		StarGazers starGazers;

		var gitHubApiV3Service = ServiceCollection.ServiceProvider.GetRequiredService<GitHubApiV3Service>();
		var gitHubAuthenticationService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubAuthenticationService>();

		await gitHubAuthenticationService.ActivateDemoUser(TestCancellationTokenSource.Token).ConfigureAwait(false);

		//Act
		starGazers = await gitHubApiV3Service.GetStarGazers(GitHubConstants.GitTrendsRepoOwner, GitHubConstants.GitTrendsRepoName, CancellationToken.None).ConfigureAwait(false);

		//Assert
		Assert.IsNotEmpty(starGazers.StarredAt);
		Assert.Greater(starGazers.StarredAt.Count, 0);
		Assert.AreEqual(starGazers.TotalCount, starGazers.StarredAt.Count);
	}

	[Test]
	public async Task GetStarGazersTest_AuthenticatedUser()
	{
		//Arrange
		StarGazers starGazers;

		var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
		var gitHubApiV3Service = ServiceCollection.ServiceProvider.GetRequiredService<GitHubApiV3Service>();
		var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();

		await AuthenticateUser(gitHubUserService, gitHubGraphQLApiService, TestCancellationTokenSource.Token).ConfigureAwait(false);

		//Act
		starGazers = await gitHubApiV3Service.GetStarGazers(GitHubConstants.GitTrendsRepoOwner, GitHubConstants.GitTrendsRepoName, CancellationToken.None).ConfigureAwait(false);

		//Assert
		Assert.IsNotEmpty(starGazers.StarredAt);
		Assert.Greater(starGazers.StarredAt.Count, 400);
		Assert.AreEqual(starGazers.TotalCount, starGazers.StarredAt.Count);
	}

	[Test]
	public async Task GetStarGazers_ValidRepo()
	{
		//Arrange
		StarGazers starGazers;

		var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
		var gitHubApiV3Service = ServiceCollection.ServiceProvider.GetRequiredService<GitHubApiV3Service>();
		var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();

		await AuthenticateUser(gitHubUserService, gitHubGraphQLApiService, TestCancellationTokenSource.Token).ConfigureAwait(false);

		//Act
		starGazers = await gitHubApiV3Service.GetStarGazers(GitHubConstants.GitTrendsRepoOwner, GitHubConstants.GitTrendsRepoName, CancellationToken.None).ConfigureAwait(false); ;

		//Assert
		Assert.NotNull(starGazers);
		Assert.Greater(starGazers.TotalCount, 500);
		Assert.IsNotEmpty(starGazers.StarredAt);
		Assert.AreEqual(starGazers.TotalCount, starGazers.StarredAt.Count);
	}

	[Test]
	public async Task GetStarGazers_InvalidRepo()
	{
		//Arrange
		const string fakeRepoName = "abc123321";
		const string fakeRepoOwner = "zxcvbnmlkjhgfdsa1234567890";

		var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
		var gitHubApiV3Service = ServiceCollection.ServiceProvider.GetRequiredService<GitHubApiV3Service>();
		var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();

		await AuthenticateUser(gitHubUserService, gitHubGraphQLApiService, TestCancellationTokenSource.Token).ConfigureAwait(false);

		//Act
		var exception = Assert.ThrowsAsync<ApiException>(() => gitHubApiV3Service.GetStarGazers(fakeRepoOwner, fakeRepoName, CancellationToken.None));

		//Assert
		Assert.AreEqual(HttpStatusCode.NotFound, exception?.StatusCode);

		//"Could not resolve to a Repository with the name 'zxcvbnmlkjhgfdsa1234567890/abc123321'."
	}

	[Test, Ignore("Test Fails When GitHub API Rate Limit Exceeded")]
	public async Task GetStarGazers_Unauthenticated()
	{
		//Arrange
		StarGazers starGazers;
		var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
		var gitHubApiV3Service = ServiceCollection.ServiceProvider.GetRequiredService<GitHubApiV3Service>();

		//Act
		gitHubUserService.InvalidateToken();

		starGazers = await gitHubApiV3Service.GetStarGazers(GitHubConstants.GitTrendsRepoOwner, GitHubConstants.GitTrendsRepoName, CancellationToken.None).ConfigureAwait(false);

		//Assert
		Assert.NotNull(starGazers);
		Assert.Greater(starGazers.TotalCount, 500);
		Assert.IsNotEmpty(starGazers.StarredAt);
		Assert.AreEqual(starGazers.TotalCount, starGazers.StarredAt.Count);
	}
}