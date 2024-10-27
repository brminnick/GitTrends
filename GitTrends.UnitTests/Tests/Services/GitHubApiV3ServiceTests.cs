using System.Net;
using GitTrends.Common;
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
		Assert.That(referringSites, Is.Not.Null);
		Assert.That(referringSites, Is.Not.Empty);
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
		Assert.That(exception?.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
	}

	[Test]
	public void GetReferringSitesTest_InvalidRepo()
	{
		//Arrange
		var gitHubApiV3Service = ServiceCollection.ServiceProvider.GetRequiredService<GitHubApiV3Service>();

		//Act
		var exception = Assert.ThrowsAsync<ApiException>(async () => await gitHubApiV3Service.GetReferringSites("xamarin", GitHubConstants.GitTrendsRepoName, CancellationToken.None).ConfigureAwait(false));

		//Assert
		Assert.That(exception?.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
	}

	[Test]
	public async Task GetRepositoryCloneStatisticsTest_ValidRepo()
	{
		//Arrange
		var gitHubApiV3Service = ServiceCollection.ServiceProvider.GetRequiredService<GitHubApiV3Service>();

		//Act
		var clones = await gitHubApiV3Service.GetRepositoryCloneStatistics(GitHubConstants.GitTrendsRepoOwner, GitHubConstants.GitTrendsRepoName, CancellationToken.None).ConfigureAwait(false);

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(clones, Is.Not.Null);
			Assert.That(clones.DailyClonesList, Is.Not.Null);
			Assert.That(clones.DailyClonesList, Is.Not.Empty);
			Assert.That(clones.RepositoryName, Is.EqualTo(GitHubConstants.GitTrendsRepoName));
			Assert.That(clones.RepositoryOwner, Is.EqualTo(GitHubConstants.GitTrendsRepoOwner));
		});
	}

	[Test]
	public void GetRepositoryCloneStatisticsTest_InvalidRepo()
	{
		//Arrange
		var gitHubApiV3Service = ServiceCollection.ServiceProvider.GetRequiredService<GitHubApiV3Service>();

		//Act
		var exception = Assert.ThrowsAsync<ApiException>(async () => await gitHubApiV3Service.GetRepositoryCloneStatistics("xamarin", GitHubConstants.GitTrendsRepoName, CancellationToken.None).ConfigureAwait(false));

		//Assert
		Assert.That(exception?.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
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
		Assert.That(exception?.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
	}

	[Test]
	public async Task GetRepositoryViewStatisticsTest_ValidRepo()
	{
		//Arrange
		var gitHubApiV3Service = ServiceCollection.ServiceProvider.GetRequiredService<GitHubApiV3Service>();

		//Act
		var views = await gitHubApiV3Service.GetRepositoryViewStatistics(GitHubConstants.GitTrendsRepoOwner, GitHubConstants.GitTrendsRepoName, CancellationToken.None).ConfigureAwait(false);

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(views, Is.Not.Null);
			Assert.That(views.DailyViewsList, Is.Not.Null);
			Assert.That(views.DailyViewsList, Is.Not.Empty);
			Assert.That(views.RepositoryName, Is.EqualTo(GitHubConstants.GitTrendsRepoName));
			Assert.That(views.RepositoryOwner, Is.EqualTo(GitHubConstants.GitTrendsRepoOwner));
		});
	}

	[Test]
	public void GetRepositoryViewStatisticsTest_InvalidRepo()
	{
		//Arrange
		var gitHubApiV3Service = ServiceCollection.ServiceProvider.GetRequiredService<GitHubApiV3Service>();

		//Act
		var exception = Assert.ThrowsAsync<ApiException>(async () => await gitHubApiV3Service.GetRepositoryViewStatistics("xamarin", GitHubConstants.GitTrendsRepoName, CancellationToken.None).ConfigureAwait(false));

		//Assert
		Assert.That(exception?.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
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
		Assert.That(exception?.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
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
		Assert.Multiple(() =>
		{
			Assert.That(starGazers.StarredAt, Is.Not.Empty);
			Assert.That(starGazers.StarredAt, Is.Not.Empty);
			Assert.That(starGazers.StarredAt, Has.Count.EqualTo(starGazers.TotalCount));
		});
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
		Assert.Multiple(() =>
		{
			Assert.That(starGazers.StarredAt, Is.Not.Empty);
			Assert.That(starGazers.StarredAt, Has.Count.GreaterThan(400));
			Assert.That(starGazers.StarredAt, Has.Count.EqualTo(starGazers.TotalCount));
		});
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
		starGazers = await gitHubApiV3Service.GetStarGazers(GitHubConstants.GitTrendsRepoOwner, GitHubConstants.GitTrendsRepoName, CancellationToken.None).ConfigureAwait(false);

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(starGazers, Is.Not.Null);
			Assert.That(starGazers.TotalCount, Is.GreaterThan(500));
			Assert.That(starGazers.StarredAt, Is.Not.Empty);
			Assert.That(starGazers.StarredAt, Has.Count.EqualTo(starGazers.TotalCount));
		});
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
		Assert.That(exception?.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));

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
		Assert.Multiple(() =>
		{
			Assert.That(starGazers, Is.Not.Null);
			Assert.That(starGazers.TotalCount, Is.GreaterThan(500));
			Assert.That(starGazers.StarredAt, Is.Not.Empty);
			Assert.That(starGazers.StarredAt, Has.Count.EqualTo(starGazers.TotalCount));
		});
	}
}