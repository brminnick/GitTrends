using System.Net;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using GitTrends.Shared;
using Refit;

namespace GitTrends.UnitTests;

class GitHubGraphQLApiServiceTests : BaseTest
{
	[Test]
	public void GetCurrentUserInfoTest_Unauthenticated()
	{
		//Arrange
		var githubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();

		//Act
		var exception = Assert.ThrowsAsync<ApiException>(async () => await githubGraphQLApiService.GetCurrentUserInfo(CancellationToken.None).ConfigureAwait(false));

		//Assert
		Assert.AreEqual(HttpStatusCode.Unauthorized, exception?.StatusCode);
	}

	[Test]
	public void GetCurrentUserInfoTest_DemoUser()
	{
		//Arrange
		var githubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();
		var gitHubAuthenticationService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubAuthenticationService>();

		//Act
		var exception = Assert.ThrowsAsync<ApiException>(async () => await githubGraphQLApiService.GetCurrentUserInfo(CancellationToken.None).ConfigureAwait(false));

		//Assert
		Assert.AreEqual(HttpStatusCode.Unauthorized, exception?.StatusCode);
	}

	[Test]
	public async Task GetCurrentUserInfoTest_AuthenticatedUser()
	{
		//Arrange
		var githubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();
		var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();

		//Act
		await AuthenticateUser(gitHubUserService, githubGraphQLApiService, TestCancellationTokenSource.Token).ConfigureAwait(false);

		var (login, name, avatarUri) = await githubGraphQLApiService.GetCurrentUserInfo(CancellationToken.None).ConfigureAwait(false);

		//Assert
		Assert.AreEqual(gitHubUserService.Alias, login);
		Assert.AreEqual(gitHubUserService.Name, name);
		Assert.AreEqual(new Uri(gitHubUserService.AvatarUrl), avatarUri);
	}

	[Test]
	public void GetRepositoriesTest_Unauthenticated()
	{
		//Arrange
		var githubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();
		var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();

		//Act
		var exception = Assert.ThrowsAsync<ApiException>(async () =>
		{
			await foreach (var retrievedRepositories in githubGraphQLApiService.GetRepositories(gitHubUserService.Alias, TestCancellationTokenSource.Token).ConfigureAwait(false))
			{

			}
		});

		//Assert
		Assert.AreEqual(HttpStatusCode.Unauthorized, exception?.StatusCode);
	}

	[Test]
	public async Task GetRepositoriesTest_DemoUser()
	{
		//Arrange
		List<Repository> repositories = [];
		var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
		var githubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();
		var gitHubAuthenticationService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubAuthenticationService>();

		//Act
		await gitHubAuthenticationService.ActivateDemoUser(TestCancellationTokenSource.Token).ConfigureAwait(false);

		await foreach (var repository in githubGraphQLApiService.GetRepositories(gitHubUserService.Alias, CancellationToken.None).ConfigureAwait(false))
		{
			repositories.Add(repository);
		}

		//Assert
		Assert.IsNotEmpty(repositories);
		Assert.AreEqual(DemoDataConstants.RepoCount, repositories.Count);

		foreach (var repository in repositories)
		{
			Assert.GreaterOrEqual(DemoDataConstants.MaximumRandomNumber, repository.IssuesCount);
			Assert.GreaterOrEqual(DemoDataConstants.MaximumRandomNumber, repository.ForkCount);
			Assert.AreEqual(DemoUserConstants.Alias, repository.OwnerLogin);

			Assert.IsNull(repository.TotalClones);
			Assert.IsNull(repository.TotalUniqueClones);
			Assert.IsNull(repository.TotalViews);
			Assert.IsNull(repository.TotalUniqueViews);
		}
	}

	[Test]
	public async Task GetRepositoriesTest_AuthenticatedUser()
	{
		//Arrange
		List<Repository> repositories = [];
		var githubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();
		var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
		gitHubUserService.ShouldIncludeOrganizations = true;

		//Act
		await AuthenticateUser(gitHubUserService, githubGraphQLApiService, TestCancellationTokenSource.Token).ConfigureAwait(false);

		await foreach (var repository in githubGraphQLApiService.GetRepositories(gitHubUserService.Alias, CancellationToken.None).ConfigureAwait(false))
		{
			repositories.Add(repository);
		}

		//Assert
		Assert.GreaterOrEqual(repositories.Count, 0);

		var gitTrendsRepository = repositories.Single(static x => x.Name is GitHubConstants.GitTrendsRepoName
			&& x.OwnerLogin is GitHubConstants.GitTrendsRepoOwner);

		Assert.AreEqual(GitHubConstants.GitTrendsRepoName, gitTrendsRepository.Name);
		Assert.AreEqual(GitHubConstants.GitTrendsRepoOwner, gitTrendsRepository.OwnerLogin);
		Assert.AreEqual(AuthenticatedGitHubUserAvatarUrl, gitTrendsRepository.OwnerAvatarUrl);

		Assert.Greater(repositories.Sum(static x => x.StarCount), 0);
		Assert.AreEqual(0, repositories.Sum(static x => x.TotalViews));
		Assert.AreEqual(0, repositories.Sum(static x => x.TotalUniqueViews));
		Assert.AreEqual(0, repositories.Sum(static x => x.TotalClones));
		Assert.AreEqual(0, repositories.Sum(static x => x.TotalUniqueClones));
		Assert.AreEqual(0, repositories.Sum(static x => x.StarredAt?.Count));
	}

	[Test]
	public void GetViewerOrganizationRepositoriesTest_Unauthenticated()
	{
		//Arrange
		List<Repository> repositories = [];
		var githubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();

		//Act
		var exception = Assert.ThrowsAsync<ApiException>(async () =>
		{
			await foreach (var repository in githubGraphQLApiService.GetViewerOrganizationRepositories(CancellationToken.None).ConfigureAwait(false))
			{
				repositories.Add(repository);
			}
		});

		//Assert
		Assert.AreEqual(HttpStatusCode.Unauthorized, exception?.StatusCode);
		Assert.IsEmpty(repositories);
	}

	[Test]
	public async Task GetViewerOrganizationRepositoriesTest_Demo()
	{
		//Arrange
		List<Repository> repositories = [];
		var githubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();
		var gitHubAuthenticationService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubAuthenticationService>();

		await gitHubAuthenticationService.ActivateDemoUser(TestCancellationTokenSource.Token).ConfigureAwait(false);

		//Act
		var exception = Assert.ThrowsAsync<ApiException>(async () =>
		{
			await foreach (var repository in githubGraphQLApiService.GetViewerOrganizationRepositories(CancellationToken.None).ConfigureAwait(false))
			{
				repositories.Add(repository);
			}
		});

		//Assert
		Assert.AreEqual(HttpStatusCode.Unauthorized, exception?.StatusCode);
		Assert.IsEmpty(repositories);
	}

	[Test]
	public async Task GetViewerOrganizationRepositoriesTest_AuthenticatedUser()
	{
		//Arrange
		List<Repository> repositories = [];
		var githubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();
		var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();

		//Act
		await AuthenticateUser(gitHubUserService, githubGraphQLApiService, TestCancellationTokenSource.Token).ConfigureAwait(false);

		await foreach (var repository in githubGraphQLApiService.GetViewerOrganizationRepositories(CancellationToken.None).ConfigureAwait(false))
		{
			repositories.Add(repository);
		}

		//Assert
		Assert.IsNotEmpty(repositories);
		Assert.GreaterOrEqual(repositories.Count, 1);


		Assert.Greater(repositories.Sum(static x => x.StarCount), 0);
		Assert.AreEqual(0, repositories.Sum(static x => x.TotalViews));
		Assert.AreEqual(0, repositories.Sum(static x => x.TotalUniqueViews));
		Assert.AreEqual(0, repositories.Sum(static x => x.TotalClones));
		Assert.AreEqual(0, repositories.Sum(static x => x.TotalUniqueClones));
		Assert.AreEqual(0, repositories.Sum(static x => x.StarredAt?.Count));
	}

	[Test]
	public void GetOrganizationRepositoriesTest_Unauthenticated()
	{
		//Arrange
		var githubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();

		//Act //Assert
		var exception = Assert.ThrowsAsync<ApiException>(() => githubGraphQLApiService.GetOrganizationRepositories(nameof(GitTrends), CancellationToken.None));

		//Assert
		Assert.AreEqual(HttpStatusCode.Unauthorized, exception?.StatusCode);
	}

	[Test]
	public async Task GetOrganizationRepositoriesTest_Demo()
	{
		//Arrange
		var githubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();
		var gitHubAuthenticationService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubAuthenticationService>();

		await gitHubAuthenticationService.ActivateDemoUser(TestCancellationTokenSource.Token).ConfigureAwait(false);

		//Act //Assert
		var exception = Assert.ThrowsAsync<ApiException>(() => githubGraphQLApiService.GetOrganizationRepositories(nameof(GitTrends), CancellationToken.None));

		//Assert
		Assert.AreEqual(HttpStatusCode.Unauthorized, exception?.StatusCode);
	}

	[Test]
	public async Task GetOrganizationRepositoriesTest_AuthenticatedUser()
	{
		//Arrange
		IReadOnlyList<Repository> repositories;
		var githubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();
		var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();

		//Act
		await AuthenticateUser(gitHubUserService, githubGraphQLApiService, TestCancellationTokenSource.Token).ConfigureAwait(false);

		repositories = await githubGraphQLApiService.GetOrganizationRepositories(nameof(GitTrends), CancellationToken.None).ConfigureAwait(false);

		//Assert
		Assert.IsNotEmpty(repositories);
		Assert.Greater(repositories.Count, 0);

		Assert.Greater(repositories.Sum(static x => x.StarCount), 0);
		Assert.AreEqual(0, repositories.Sum(static x => x.TotalViews));
		Assert.AreEqual(0, repositories.Sum(static x => x.TotalUniqueViews));
		Assert.AreEqual(0, repositories.Sum(static x => x.TotalClones));
		Assert.AreEqual(0, repositories.Sum(static x => x.TotalUniqueClones));
		Assert.AreEqual(0, repositories.Sum(static x => x.StarredAt?.Count));
	}

	[Test]
	public void GetRepositoryTest_Unauthenticated()
	{
		//Arrange
		var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
		var githubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();

		//Act
		var exception = Assert.ThrowsAsync<ApiException>(async () => await githubGraphQLApiService.GetRepository(gitHubUserService.Alias, GitHubConstants.GitTrendsRepoName, CancellationToken.None).ConfigureAwait(false));

		//Assert
		Assert.AreEqual(HttpStatusCode.Unauthorized, exception?.StatusCode);
	}

	[Test]
	public async Task GetRepositoryTest_Demo()
	{
		//Arrange
		var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
		var githubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();
		var gitHubAuthenticationService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubAuthenticationService>();

		//Act
		await gitHubAuthenticationService.ActivateDemoUser(TestCancellationTokenSource.Token).ConfigureAwait(false);
		var exception = Assert.ThrowsAsync<ApiException>(async () => await githubGraphQLApiService.GetRepository(gitHubUserService.Alias, GitHubConstants.GitTrendsRepoName, CancellationToken.None).ConfigureAwait(false));

		//Assert
		Assert.AreEqual(HttpStatusCode.Unauthorized, exception?.StatusCode);
	}

	[Test]
	public async Task GetRepositoryTest_AuthenticatedUser()
	{
		//Arrange
		Repository repository;
		DateTimeOffset beforeDownload, afterDownload;

		var githubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();
		var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();

		await AuthenticateUser(gitHubUserService, githubGraphQLApiService, TestCancellationTokenSource.Token).ConfigureAwait(false);

		//Act
		beforeDownload = DateTimeOffset.UtcNow;

		repository = await githubGraphQLApiService.GetRepository(GitHubConstants.GitTrendsRepoOwner, GitHubConstants.GitTrendsRepoName, CancellationToken.None).ConfigureAwait(false);

		afterDownload = DateTimeOffset.UtcNow;

		//Assert
		Assert.Greater(repository.StarCount, 150);
		Assert.Greater(repository.ForkCount, 30);

		Assert.AreEqual(GitHubConstants.GitTrendsRepoName, repository.Name);
		Assert.AreEqual(GitHubConstants.GitTrendsRepoOwner, repository.OwnerLogin);
		Assert.AreEqual(AuthenticatedGitHubUserAvatarUrl, repository.OwnerAvatarUrl);

		Assert.IsNull(repository.TotalClones);
		Assert.IsNull(repository.TotalUniqueClones);
		Assert.IsNull(repository.TotalViews);
		Assert.IsNull(repository.TotalUniqueViews);

		Assert.IsTrue(beforeDownload.CompareTo(repository.DataDownloadedAt) < 0);
		Assert.IsTrue(afterDownload.CompareTo(repository.DataDownloadedAt) > 0);

		Assert.IsFalse(string.IsNullOrWhiteSpace(repository.Description));

		Assert.IsFalse(repository.IsFork);
	}

	[Test]
	public async Task GetStarGazersTest_DemoUser()
	{
		//Arrange
		StarGazers starGazers;

		var gitHubAuthenticationService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubAuthenticationService>();
		var githubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();

		await gitHubAuthenticationService.ActivateDemoUser(TestCancellationTokenSource.Token).ConfigureAwait(false);

		//Act
		starGazers = await githubGraphQLApiService.GetStarGazers(GitHubConstants.GitTrendsRepoName, GitHubConstants.GitTrendsRepoOwner, CancellationToken.None).ConfigureAwait(false);

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

		var githubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();
		var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();

		await AuthenticateUser(gitHubUserService, githubGraphQLApiService, TestCancellationTokenSource.Token).ConfigureAwait(false);

		//Act
		starGazers = await githubGraphQLApiService.GetStarGazers(GitHubConstants.GitTrendsRepoName, GitHubConstants.GitTrendsRepoOwner, CancellationToken.None).ConfigureAwait(false);

		//Assert
		Assert.IsNotEmpty(starGazers.StarredAt);
		Assert.Greater(starGazers.StarredAt.Count, 500);
		Assert.AreEqual(starGazers.TotalCount, starGazers.StarredAt.Count);
	}

	[Test]
	public async Task GetStarGazers_ValidRepo()
	{
		//Arrange
		StarGazers starGazers;

		var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
		var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();

		await AuthenticateUser(gitHubUserService, gitHubGraphQLApiService, TestCancellationTokenSource.Token).ConfigureAwait(false);

		//Act
		starGazers = await gitHubGraphQLApiService.GetStarGazers(GitHubConstants.GitTrendsRepoName, GitHubConstants.GitTrendsRepoOwner, CancellationToken.None).ConfigureAwait(false);
		;

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
		var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();

		await AuthenticateUser(gitHubUserService, gitHubGraphQLApiService, TestCancellationTokenSource.Token).ConfigureAwait(false);

		//Act
		var graphQLException = Assert.ThrowsAsync<GraphQLException<StarGazerResponse>>(() => gitHubGraphQLApiService.GetStarGazers(fakeRepoName, fakeRepoOwner, CancellationToken.None));

		//Assert
		Assert.AreEqual(HttpStatusCode.OK, graphQLException?.StatusCode);
		Assert.IsTrue(graphQLException?.Errors.First().Message.Contains("Could not resolve to a Repository", StringComparison.OrdinalIgnoreCase));

		//"Could not resolve to a Repository with the name 'zxcvbnmlkjhgfdsa1234567890/abc123321'."
	}

	[Test]
	public void GetStarGazers_Unauthenticated()
	{
		//Arrange
		var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
		var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();

		//Act
		gitHubUserService.InvalidateToken();
		var exception = Assert.ThrowsAsync<ApiException>(() => gitHubGraphQLApiService.GetStarGazers(GitHubConstants.GitTrendsRepoName, GitHubConstants.GitTrendsRepoOwner, CancellationToken.None));
		
		//Assert
		Assert.AreEqual(HttpStatusCode.Unauthorized, exception?.StatusCode);
	}
}