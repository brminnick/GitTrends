using System.Net;
using GitTrends.Common;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
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
		Assert.That(exception?.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
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
		Assert.That(exception?.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
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
		Assert.Multiple(() =>
		{
			Assert.That(login, Is.EqualTo(gitHubUserService.Alias));
			Assert.That(name, Is.EqualTo(gitHubUserService.Name));
			Assert.That(avatarUri, Is.EqualTo(new Uri(gitHubUserService.AvatarUrl)));
		});
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
		Assert.That(exception?.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
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
		Assert.That(repositories, Is.Not.Empty);
		Assert.That(repositories, Has.Count.EqualTo(DemoDataConstants.RepoCount));

		foreach (var repository in repositories)
		{
			Assert.Multiple(() =>
			{
				Assert.That(repository.IssuesCount, Is.LessThan(DemoDataConstants.MaximumRandomNumber));
				Assert.That(repository.ForkCount, Is.LessThan(DemoDataConstants.MaximumRandomNumber));
				Assert.That(repository.OwnerLogin, Is.EqualTo(DemoUserConstants.Alias));

				Assert.That(repository.TotalClones, Is.Null);
				Assert.That(repository.TotalUniqueClones, Is.Null);
				Assert.That(repository.TotalViews, Is.Null);
				Assert.That(repository.TotalUniqueViews, Is.Null);
			});
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
		Assert.That(repositories, Has.Count.GreaterThanOrEqualTo(0));

		var gitTrendsRepository = repositories.Single(static x => x is { Name: GitHubConstants.GitTrendsRepoName, OwnerLogin: GitHubConstants.GitTrendsRepoOwner });

		Assert.Multiple(() =>
		{
			Assert.That(gitTrendsRepository.Name, Is.EqualTo(GitHubConstants.GitTrendsRepoName));
			Assert.That(gitTrendsRepository.OwnerLogin, Is.EqualTo(GitHubConstants.GitTrendsRepoOwner));
			Assert.That(gitTrendsRepository.OwnerAvatarUrl, Is.EqualTo(AuthenticatedGitHubUserAvatarUrl));

			Assert.That(repositories.Sum(static x => x.StarCount), Is.GreaterThan(0));
			Assert.That(repositories.Sum(static x => x.TotalViews), Is.EqualTo(0));
			Assert.That(repositories.Sum(static x => x.TotalUniqueViews), Is.EqualTo(0));
			Assert.That(repositories.Sum(static x => x.TotalClones), Is.EqualTo(0));
			Assert.That(repositories.Sum(static x => x.TotalUniqueClones), Is.EqualTo(0));
			Assert.That(repositories.Sum(static x => x.StarredAt?.Count), Is.EqualTo(0));
		});
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
		Assert.Multiple(() =>
		{
			Assert.That(exception?.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
			Assert.That(repositories, Is.Empty);
		});
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
		Assert.Multiple(() =>
		{
			Assert.That(exception?.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
			Assert.That(repositories, Is.Empty);
		});
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
		Assert.Multiple(() =>
		{
			Assert.That(repositories, Is.Not.Empty);
			Assert.That(repositories, Is.Not.Empty);

			Assert.That(repositories.Sum(static x => x.StarCount), Is.GreaterThan(0));
			Assert.That(repositories.Sum(static x => x.TotalViews), Is.EqualTo(0));
			Assert.That(repositories.Sum(static x => x.TotalUniqueViews), Is.EqualTo(0));
			Assert.That(repositories.Sum(static x => x.TotalClones), Is.EqualTo(0));
			Assert.That(repositories.Sum(static x => x.TotalUniqueClones), Is.EqualTo(0));
			Assert.That(repositories.Sum(static x => x.StarredAt?.Count), Is.EqualTo(0));
		});
	}

	[Test]
	public void GetOrganizationRepositoriesTest_Unauthenticated()
	{
		//Arrange
		var githubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();

		//Act //Assert
		var exception = Assert.ThrowsAsync<ApiException>(() => githubGraphQLApiService.GetOrganizationRepositories(nameof(GitTrends), CancellationToken.None));

		//Assert
		Assert.That(exception?.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
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
		Assert.That(exception?.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
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
		Assert.Multiple(() =>
		{
			Assert.That(repositories, Is.Not.Empty);
			Assert.That(repositories, Is.Not.Empty);

			Assert.That(repositories.Sum(static x => x.StarCount), Is.GreaterThan(0));
			Assert.That(repositories.Sum(static x => x.TotalViews), Is.EqualTo(0));
			Assert.That(repositories.Sum(static x => x.TotalUniqueViews), Is.EqualTo(0));
			Assert.That(repositories.Sum(static x => x.TotalClones), Is.EqualTo(0));
			Assert.That(repositories.Sum(static x => x.TotalUniqueClones), Is.EqualTo(0));
			Assert.That(repositories.Sum(static x => x.StarredAt?.Count), Is.EqualTo(0));
		});
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
		Assert.That(exception?.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
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
		Assert.That(exception?.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
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
		Assert.Multiple(() =>
		{
			Assert.That(repository.StarCount, Is.GreaterThan(150));
			Assert.That(repository.ForkCount, Is.GreaterThan(30));

			Assert.That(repository.Name, Is.EqualTo(GitHubConstants.GitTrendsRepoName));
			Assert.That(repository.OwnerLogin, Is.EqualTo(GitHubConstants.GitTrendsRepoOwner));
			Assert.That(repository.OwnerAvatarUrl, Is.EqualTo(AuthenticatedGitHubUserAvatarUrl));

			Assert.That(repository.TotalClones, Is.Null);
			Assert.That(repository.TotalUniqueClones, Is.Null);
			Assert.That(repository.TotalViews, Is.Null);
			Assert.That(repository.TotalUniqueViews, Is.Null);

			Assert.That(beforeDownload.CompareTo(repository.DataDownloadedAt), Is.LessThan(0));
			Assert.That(afterDownload.CompareTo(repository.DataDownloadedAt), Is.GreaterThan(0));

			Assert.That(string.IsNullOrWhiteSpace(repository.Description), Is.False);

			Assert.That(repository.IsFork, Is.False);
		});
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

		var githubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();
		var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();

		await AuthenticateUser(gitHubUserService, githubGraphQLApiService, TestCancellationTokenSource.Token).ConfigureAwait(false);

		//Act
		starGazers = await githubGraphQLApiService.GetStarGazers(GitHubConstants.GitTrendsRepoName, GitHubConstants.GitTrendsRepoOwner, CancellationToken.None).ConfigureAwait(false);

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(starGazers.StarredAt, Is.Not.Empty);
			Assert.That(starGazers.StarredAt, Has.Count.GreaterThan(500));
			Assert.That(starGazers.StarredAt, Has.Count.EqualTo(starGazers.TotalCount));
		});
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
		var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();

		await AuthenticateUser(gitHubUserService, gitHubGraphQLApiService, TestCancellationTokenSource.Token).ConfigureAwait(false);

		//Act
		var graphQLException = Assert.ThrowsAsync<GraphQLException<StarGazerResponse>>(() => gitHubGraphQLApiService.GetStarGazers(fakeRepoName, fakeRepoOwner, CancellationToken.None));

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(graphQLException?.StatusCode, Is.EqualTo(HttpStatusCode.OK));
			Assert.That(graphQLException?.Errors[0].Message.Contains("Could not resolve to a Repository", StringComparison.OrdinalIgnoreCase), Is.True);
		});

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
		Assert.That(exception?.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
	}
}