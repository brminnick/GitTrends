using System.Diagnostics;
using System.Net;
using GitTrends.Common;
using GitTrends.Mobile.Common;
using Refit;

namespace GitTrends.UnitTests;

class GitHubApiRepositoriesServiceTests : BaseTest
{
	public override async Task Setup()
	{
		await base.Setup().ConfigureAwait(false);

		var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
		var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();

		await AuthenticateUser(gitHubUserService, gitHubGraphQLApiService, TestCancellationTokenSource.Token).ConfigureAwait(false);
	}

	[Test]
	public async Task UpdateRepositoriesWithViewsClonesStarsData_ValidRepo()
	{
		//Arrange
		IReadOnlyList<Repository> repositories_Filtered;
		IReadOnlyList<Repository> repositories_NoViewsClonesStarsData_Filtered;

		List<Repository> repositories = [];
		List<Repository> repositories_NoStarsData = [];
		List<Repository> repositories_NoViewsClonesStarsData = [];

		var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
		var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();
		var gitHubApiRepositoriesService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubApiRepositoriesService>();

		var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(1));

		//Act
		await foreach (var repository in gitHubGraphQLApiService.GetRepositories(gitHubUserService.Alias, cancellationTokenSource.Token).ConfigureAwait(false))
		{
			repositories_NoViewsClonesStarsData.Add(repository);
		}

		repositories_NoViewsClonesStarsData_Filtered = [.. repositories_NoViewsClonesStarsData.RemoveForksDuplicatesAndArchives()];

		await foreach (var repository in gitHubApiRepositoriesService.UpdateRepositoriesWithViewsAndClonesData(repositories_NoViewsClonesStarsData_Filtered, cancellationTokenSource.Token).ConfigureAwait(false))
		{
			repositories_NoStarsData.Add(repository);
		}

		repositories_Filtered = [.. repositories_NoStarsData.RemoveForksDuplicatesAndArchives()];

		await foreach (var repository in gitHubApiRepositoriesService.UpdateRepositoriesWithStarsData(repositories_Filtered, cancellationTokenSource.Token).ConfigureAwait(false))
		{
			repositories.Add(repository);
		}

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(repositories, Is.Not.Empty);
			Assert.That(repositories_NoStarsData, Is.Not.Empty);
			Assert.That(repositories_NoViewsClonesStarsData, Is.Not.Empty);
		});

		foreach (var repository in repositories_NoViewsClonesStarsData)
		{
			Assert.Multiple(() =>
			{
				Assert.That(repository.StarredAt, Is.Null);
				Assert.That(repository.TotalClones, Is.Null);
				Assert.That(repository.TotalUniqueClones, Is.Null);
				Assert.That(repository.TotalViews, Is.Null);
				Assert.That(repository.TotalUniqueViews, Is.Null);
			});
		}

		foreach (var repository in repositories_NoStarsData)
		{
			Assert.Multiple(() =>
			{
				Assert.That(repository.StarredAt, Is.Null);
				Assert.That(repository.TotalClones, Is.Not.Null);
				Assert.That(repository.TotalUniqueClones, Is.Not.Null);
				Assert.That(repository.TotalViews, Is.Not.Null);
				Assert.That(repository.TotalUniqueViews, Is.Not.Null);
			});
		}

		foreach (var repository in repositories)
		{
			Assert.Multiple(() =>
			{
				Assert.That(repository.StarredAt, Is.Not.Null);
				Assert.That(repository.DailyViewsList, Is.Not.Empty);
				Assert.That(repository.DailyClonesList, Is.Not.Empty);
				Assert.That(repository.StarredAt?.Count, Is.InRange(repository.StarCount - 1, repository.StarCount + 1));
			});
		}

		Assert.Multiple(() =>
		{
			Assert.That(repositories.Sum(static x => x.TotalClones ?? throw new InvalidOperationException($"{nameof(x.TotalClones)} cannot be null")), Is.GreaterThan(0));
			Assert.That(repositories.Sum(static x => x.TotalUniqueClones ?? throw new InvalidOperationException($"{nameof(x.TotalUniqueClones)} cannot be null")), Is.GreaterThan(0));
			Assert.That(repositories.Sum(static x => x.TotalViews ?? throw new InvalidOperationException($"{nameof(x.TotalViews)} cannot be null")), Is.GreaterThan(0));
			Assert.That(repositories.Sum(static x => x.TotalUniqueViews ?? throw new InvalidOperationException($"{nameof(x.TotalUniqueViews)} cannot be null")), Is.GreaterThan(0));
		});
	}

	[Test]
	public async Task UpdateRepositoriesWithViewsClonesData_ValidRepo_NotFiltered()
	{
		//Arrange
		var repositories = new List<Repository>();
		var repositories_NoViewsClonesData = new List<Repository>();

		var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
		var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();
		var gitHubApiRepositoriesService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubApiRepositoriesService>();

		//Act
		await foreach (var repository in gitHubGraphQLApiService.GetRepositories(gitHubUserService.Alias, CancellationToken.None).ConfigureAwait(false))
		{
			repositories_NoViewsClonesData.Add(repository);
		}

		var exception = Assert.ThrowsAsync<InvalidOperationException>(async () =>
		{
			await foreach (var repository in gitHubApiRepositoriesService.UpdateRepositoriesWithViewsAndClonesData(repositories_NoViewsClonesData, CancellationToken.None).ConfigureAwait(false))
			{
				repositories.Add(repository);
			}
		});

		//Assert
		Assert.That(exception?.Message.Contains("more than one matching element"), Is.True);
	}

	[Test]
	public async Task UpdateRepositoriesWithStarsData_ValidRepo_NotFiltered()
	{
		//Arrange
		var repositories = new List<Repository>();
		var repositories_NoViewsClonesStarsData = new List<Repository>();

		var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
		var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();
		var gitHubApiRepositoriesService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubApiRepositoriesService>();

		//Act
		await foreach (var repository in gitHubGraphQLApiService.GetRepositories(gitHubUserService.Alias, CancellationToken.None).ConfigureAwait(false))
		{
			repositories_NoViewsClonesStarsData.Add(repository);
		}

		var exception = Assert.ThrowsAsync<InvalidOperationException>(async () =>
		{
			await foreach (var repository in gitHubApiRepositoriesService.UpdateRepositoriesWithStarsData(repositories_NoViewsClonesStarsData, CancellationToken.None).ConfigureAwait(false))
			{
				repositories.Add(repository);
			}
		});

		//Assert
		Assert.That(exception?.Message.Contains("more than one matching element"), Is.True);
	}

	[Test]
	public async Task UpdateRepositoriesWithViewsClonesData_EmptyRepos()
	{
		//Arrange
		var repositories = new List<Repository>();

		var gitHubApiRepositoriesService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubApiRepositoriesService>();

		//Act
		await foreach (var repository in gitHubApiRepositoriesService.UpdateRepositoriesWithViewsAndClonesData([], CancellationToken.None).ConfigureAwait(false))
		{
			repositories.Add(repository);
		}

		//Assert
		Assert.That(repositories, Is.Empty);
	}

	[Test]
	public async Task UpdateRepositoriesWithViewsClonesStarsData_EmptyRepos()
	{
		//Arrange
		var repositories = new List<Repository>();

		var gitHubApiRepositoriesService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubApiRepositoriesService>();

		//Act
		await foreach (var repository in gitHubApiRepositoriesService.UpdateRepositoriesWithStarsData([], CancellationToken.None).ConfigureAwait(false))
		{
			repositories.Add(repository);
		}

		//Assert
		Assert.That(repositories, Is.Empty);
	}

	[Test]
	public async Task UpdateRepositoriesWithViewsClonesAndStarsData_ValidRepo_Unauthenticated()
	{
		//Arrange
		IReadOnlyList<Repository> repositories_NoStarsData_Filtered;
		IReadOnlyList<Repository> repositories_NoViewsClonesStarsData_Filtered;

		var repositories = new List<Repository>();
		var repositories_NoStarsData = new List<Repository>();
		var repositories_NoViewsClonesStarsData = new List<Repository>();

		var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
		var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();
		var gitHubApiRepositoriesService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubApiRepositoriesService>();


		//Act
		await foreach (var repository in gitHubGraphQLApiService.GetRepositories(gitHubUserService.Alias, CancellationToken.None).ConfigureAwait(false))
		{
			repositories_NoViewsClonesStarsData.Add(repository);
		}

		repositories_NoViewsClonesStarsData_Filtered = [.. repositories_NoViewsClonesStarsData.RemoveForksDuplicatesAndArchives()];

		gitHubUserService.InvalidateToken();

		await foreach (var repository in gitHubApiRepositoriesService.UpdateRepositoriesWithViewsAndClonesData(repositories_NoViewsClonesStarsData_Filtered, CancellationToken.None).ConfigureAwait(false))
		{
			repositories_NoStarsData.Add(repository);
		}

		repositories_NoStarsData_Filtered = [.. repositories_NoStarsData.RemoveForksDuplicatesAndArchives()];

		await foreach (var repository in gitHubApiRepositoriesService.UpdateRepositoriesWithStarsData(repositories_NoStarsData_Filtered, CancellationToken.None).ConfigureAwait(false))
		{
			repositories.Add(repository);
		}

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(repositories.SelectMany(static x => x.StarredAt ?? throw new InvalidOperationException()), Is.Empty);
			Assert.That(repositories.SelectMany(static x => x.DailyViewsList ?? throw new InvalidOperationException()), Is.Empty);
			Assert.That(repositories.SelectMany(static x => x.DailyClonesList ?? throw new InvalidOperationException()), Is.Empty);
		});
	}

	[Test]
	public async Task UpdateRepositoriesWithViewsClonesAndStarsData_ValidRepo_Authenticated()
	{
		//Arrange
		IReadOnlyList<Repository> repositories_NoStarsData_Filtered;
		IReadOnlyList<Repository> repositories_NoViewsClonesStarsData_Filtered;

		var repositories = new List<Repository>();
		var repositories_NoStarsData = new List<Repository>();
		var repositories_NoViewsClonesStarsData = new List<Repository>();

		var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
		var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();
		var gitHubApiRepositoriesService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubApiRepositoriesService>();

		await AuthenticateUser(gitHubUserService, gitHubGraphQLApiService, TestCancellationTokenSource.Token).ConfigureAwait(false);

		//Act
		await foreach (var repository in gitHubGraphQLApiService.GetRepositories("GitTrendsApp", CancellationToken.None).ConfigureAwait(false))
		{
			repositories_NoViewsClonesStarsData.Add(repository);
		}

		repositories_NoViewsClonesStarsData_Filtered = [.. repositories_NoViewsClonesStarsData.RemoveForksDuplicatesAndArchives()];

		await foreach (var repository in gitHubApiRepositoriesService.UpdateRepositoriesWithViewsAndClonesData(repositories_NoViewsClonesStarsData_Filtered, CancellationToken.None).ConfigureAwait(false))
		{
			repositories_NoStarsData.Add(repository);
		}
		;

		repositories_NoStarsData_Filtered = [.. repositories_NoStarsData.RemoveForksDuplicatesAndArchives()];

		await foreach (var repository in gitHubApiRepositoriesService.UpdateRepositoriesWithStarsData(repositories_NoStarsData_Filtered, CancellationToken.None).ConfigureAwait(false))
		{
			repositories.Add(repository);
		}
		;

		//Assert
		Assert.That(repositories, Has.Count.EqualTo(repositories_NoViewsClonesStarsData_Filtered.Count));

		foreach (var repository in repositories)
		{
			Assert.Multiple(() =>
			{
				Assert.That(repository.DailyClonesList, Is.Not.Null);
				Assert.That(repository.DailyViewsList, Is.Not.Null);
				Assert.That(repository.StarredAt, Is.Not.Null);
			});
		}
	}

	[Test]
	public async Task GetReferringSitesTest_ValidRepo_Authenticated()
	{
		//Arrange
		IReadOnlyList<ReferringSiteModel> referringSiteModels;

		var repository = new Repository(GitHubConstants.GitTrendsRepoName, GitHubConstants.GitTrendsRepoName, 1, GitHubConstants.GitTrendsRepoOwner,
			GitHubConstants.GitTrendsAvatarUrl, 1, 2, 3, "https://github.com/brminnick/gittrends", false, DateTimeOffset.UtcNow, RepositoryPermission.ADMIN, false);

		var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
		var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();
		var gitHubApiRepositoriesService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubApiRepositoriesService>();

		await AuthenticateUser(gitHubUserService, gitHubGraphQLApiService, TestCancellationTokenSource.Token).ConfigureAwait(false);

		//Act
		referringSiteModels = await gitHubApiRepositoriesService.GetReferringSites(repository, CancellationToken.None).ConfigureAwait(false);

		//Assert
		foreach (var referringSite in referringSiteModels)
		{
			Assert.Multiple(() =>
			{
				Assert.That(referringSite.IsReferrerUriValid);
				Assert.That(referringSite.Referrer, Is.Not.Null);
				Assert.That(referringSite.Referrer, Is.Not.Empty);
				Assert.That(referringSite.ReferrerUri, Is.Not.Null);
				Assert.That(Uri.IsWellFormedUriString(referringSite.ReferrerUri?.ToString(), UriKind.Absolute), Is.True);
				Assert.That(referringSite.TotalCount, Is.GreaterThan(0));
				Assert.That(referringSite.TotalUniqueCount, Is.GreaterThan(0));
			});
		}
	}

	[Test]
	public async Task GetReferringSitesTest_ValidRepo_NotAuthenticated()
	{
		//Arrange
		IReadOnlyList<ReferringSiteModel> referringSiteModels;

		var repository = new Repository(GitHubConstants.GitTrendsRepoName, GitHubConstants.GitTrendsRepoName, 1, GitHubConstants.GitTrendsRepoOwner,
			GitHubConstants.GitTrendsAvatarUrl, 1, 2, 3, "https://github.com/brminnick/gittrends", false, DateTimeOffset.UtcNow, RepositoryPermission.ADMIN, false);

		var gitHubApiRepositoriesService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubApiRepositoriesService>();

		//Act
		referringSiteModels = await gitHubApiRepositoriesService.GetReferringSites(repository, CancellationToken.None).ConfigureAwait(false);

		//Assert
		foreach (var referringSite in referringSiteModels)
		{
			Assert.Multiple(() =>
			{
				Assert.That(referringSite.IsReferrerUriValid, Is.True);
				Assert.That(referringSite.Referrer, Is.Not.Null);
				Assert.That(referringSite.Referrer, Is.Not.Empty);
				Assert.That(referringSite.ReferrerUri, Is.Not.Null);
				Assert.That(Uri.IsWellFormedUriString(referringSite.ReferrerUri?.ToString(), UriKind.Absolute));
				Assert.That(referringSite.TotalCount, Is.GreaterThan(0));
				Assert.That(referringSite.TotalUniqueCount, Is.GreaterThan(0));
			});
		}
	}

	[Test]
	public async Task GetReferringSitesTest_ValidRepo_DemoUser()
	{
		//Arrange
		IReadOnlyList<ReferringSiteModel> referringSiteModels;

		var repository = new Repository(GitHubConstants.GitTrendsRepoName, GitHubConstants.GitTrendsRepoName, 1, GitHubConstants.GitTrendsRepoOwner,
			GitHubConstants.GitTrendsAvatarUrl, 1, 2, 3, "https://github.com/brminnick/gittrends", false, DateTimeOffset.UtcNow, RepositoryPermission.ADMIN, false);

		var gitHubAuthenticationService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubAuthenticationService>();
		var gitHubApiRepositoriesService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubApiRepositoriesService>();

		await gitHubAuthenticationService.ActivateDemoUser(TestCancellationTokenSource.Token).ConfigureAwait(false);

		//Act
		referringSiteModels = await gitHubApiRepositoriesService.GetReferringSites(repository, CancellationToken.None).ConfigureAwait(false);

		//Assert
		foreach (var referringSite in referringSiteModels)
		{
			Assert.Multiple(() =>
			{
				Assert.That(referringSite.IsReferrerUriValid, Is.False);
				Assert.That(referringSite.Referrer, Is.Not.Null);
				Assert.That(referringSite.Referrer, Is.Not.Empty);
				Assert.That(referringSite.ReferrerUri, Is.Null);
				Assert.That(Uri.IsWellFormedUriString(referringSite.ReferrerUri?.ToString(), UriKind.Absolute), Is.False);
				Assert.That(referringSite.TotalCount, Is.GreaterThanOrEqualTo(0));
				Assert.That(referringSite.TotalCount, Is.LessThanOrEqualTo(DemoDataConstants.MaximumRandomNumber));
				Assert.That(referringSite.TotalUniqueCount, Is.GreaterThanOrEqualTo(0));
				Assert.That(referringSite.TotalUniqueCount, Is.LessThanOrEqualTo(DemoDataConstants.MaximumRandomNumber));
			});
		}
	}

	[Test]
	public void GetReferringSitesTest_InvalidRepo()
	{
		// Arrange
		var repository = new Repository(string.Empty, string.Empty, 1, string.Empty,
			string.Empty, 1, 2, 3, string.Empty, false, DateTimeOffset.UtcNow, RepositoryPermission.ADMIN, false);

		var gitHubApiRepositoriesService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubApiRepositoriesService>();

		// Act 
		var exception = Assert.ThrowsAsync<ApiException>(() => gitHubApiRepositoriesService.GetReferringSites(repository, CancellationToken.None));
		Assert.That(exception?.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
	}

	[Test]
	public async Task GetMobileReferringSiteTest_ValidRepo_Authenticated()
	{
		//Arrange
		IReadOnlyList<ReferringSiteModel> referringSiteModels;
		List<MobileReferringSiteModel> mobileReferringSiteModels = [];

		var repository = new Repository(GitHubConstants.GitTrendsRepoName, GitHubConstants.GitTrendsRepoName, 1, GitHubConstants.GitTrendsRepoOwner,
			GitHubConstants.GitTrendsAvatarUrl, 1, 2, 3, "https://github.com/brminnick/gittrends", false, DateTimeOffset.UtcNow, RepositoryPermission.ADMIN, false);

		var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
		var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();
		var gitHubApiRepositoriesService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubApiRepositoriesService>();

		await AuthenticateUser(gitHubUserService, gitHubGraphQLApiService, TestCancellationTokenSource.Token).ConfigureAwait(false);

		//Act
		referringSiteModels = await gitHubApiRepositoriesService.GetReferringSites(repository, CancellationToken.None).ConfigureAwait(false);
		await foreach (var mobileReferringSite in gitHubApiRepositoriesService.GetMobileReferringSites(referringSiteModels, repository.Url, CancellationToken.None).ConfigureAwait(false))
		{
			mobileReferringSiteModels.Add(mobileReferringSite);
		}

		//Assert
		foreach (var mobileReferringSite in mobileReferringSiteModels)
		{
			Assert.Multiple(() =>
			{
				Assert.That(mobileReferringSite.FavIconImageUrl, Is.Not.Null);
				Assert.That(mobileReferringSite.FavIcon, Is.Not.Null);
				if (string.Empty == mobileReferringSite.FavIconImageUrl)
					Assert.That(((FileImageSource?)mobileReferringSite.FavIcon)?.File, Is.EqualTo(FavIconService.DefaultFavIcon));
				else
					Assert.That(mobileReferringSite.FavIconImageUrl, Is.Not.Empty);
				Assert.That(mobileReferringSite.IsReferrerUriValid);
				Assert.That(mobileReferringSite.Referrer, Is.Not.Null);
				Assert.That(mobileReferringSite.ReferrerUri, Is.Not.Null);
				Assert.That(mobileReferringSite.Referrer, Is.Not.Empty);
				Assert.That(Uri.IsWellFormedUriString(mobileReferringSite.ReferrerUri?.ToString(), UriKind.Absolute), Is.True);
				Assert.That(mobileReferringSite.TotalCount, Is.GreaterThan(0));
				Assert.That(mobileReferringSite.TotalUniqueCount, Is.GreaterThan(0));
			});
		}
	}

	[Test]
	public async Task GetMobileReferringSiteTest_ValidRepo_NotAuthenticated()
	{
		//Arrange
		IReadOnlyList<ReferringSiteModel> referringSiteModels;
		List<MobileReferringSiteModel> mobileReferringSiteModels = [];

		var repository = new Repository(GitHubConstants.GitTrendsRepoName, GitHubConstants.GitTrendsRepoName, 1, GitHubConstants.GitTrendsRepoOwner,
			GitHubConstants.GitTrendsAvatarUrl, 1, 2, 3, "https://github.com/brminnick/gittrends", false, DateTimeOffset.UtcNow, RepositoryPermission.ADMIN, false);

		var gitHubApiRepositoriesService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubApiRepositoriesService>();

		//Act
		referringSiteModels = await gitHubApiRepositoriesService.GetReferringSites(repository, CancellationToken.None).ConfigureAwait(false);
		await foreach (var mobileReferringSite in gitHubApiRepositoriesService.GetMobileReferringSites(referringSiteModels, repository.Url, CancellationToken.None).ConfigureAwait(false))
		{
			mobileReferringSiteModels.Add(mobileReferringSite);
		}

		//Assert
		foreach (var mobileReferringSite in mobileReferringSiteModels)
		{
			Assert.Multiple(() =>
			{
				Assert.That(mobileReferringSite.FavIcon, Is.Not.Null);
				Assert.That(mobileReferringSite.FavIconImageUrl, Is.Not.Null);
				if (string.Empty == mobileReferringSite.FavIconImageUrl)
					Assert.That(((FileImageSource?)mobileReferringSite.FavIcon)?.File, Is.EqualTo(FavIconService.DefaultFavIcon));
				else
					Assert.That(mobileReferringSite.FavIconImageUrl, Is.Not.Empty);
				Assert.That(mobileReferringSite.IsReferrerUriValid);
				Assert.That(mobileReferringSite.Referrer, Is.Not.Null);
				Assert.That(mobileReferringSite.Referrer, Is.Not.Empty);
				Assert.That(mobileReferringSite.ReferrerUri, Is.Not.Null);
				Assert.That(Uri.IsWellFormedUriString(mobileReferringSite.ReferrerUri?.ToString(), UriKind.Absolute));
				Assert.That(mobileReferringSite.TotalCount, Is.GreaterThan(0));
				Assert.That(mobileReferringSite.TotalUniqueCount, Is.GreaterThan(0));
			});
		}
	}

	[Test]
	public async Task GetMobileReferringSiteTest_ValidRepo_DemoUser()
	{
		//Arrange
		IReadOnlyList<ReferringSiteModel> referringSiteModels;
		List<MobileReferringSiteModel> mobileReferringSiteModels = [];

		var repository = new Repository(GitHubConstants.GitTrendsRepoName, GitHubConstants.GitTrendsRepoName, 1, GitHubConstants.GitTrendsRepoOwner,
			GitHubConstants.GitTrendsAvatarUrl, 1, 2, 3, "https://github.com/brminnick/gittrends", false, DateTimeOffset.UtcNow, RepositoryPermission.ADMIN, false);

		var gitHubAuthenticationService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubAuthenticationService>();
		var gitHubApiRepositoriesService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubApiRepositoriesService>();

		await gitHubAuthenticationService.ActivateDemoUser(TestCancellationTokenSource.Token).ConfigureAwait(false);

		//Act
		referringSiteModels = await gitHubApiRepositoriesService.GetReferringSites(repository, CancellationToken.None).ConfigureAwait(false);
		await foreach (var mobileReferringSite in gitHubApiRepositoriesService.GetMobileReferringSites(referringSiteModels, repository.Url, TestCancellationTokenSource.Token).ConfigureAwait(false))
		{
			mobileReferringSiteModels.Add(mobileReferringSite);
		}

		//Assert
		foreach (var mobileReferringSite in mobileReferringSiteModels)
		{
			Assert.Multiple(() =>
			{
				Assert.That(FavIconService.DefaultFavIcon, Is.EqualTo("DefaultReferringSiteImage"));

				Assert.That(mobileReferringSite.FavIcon, Is.InstanceOf<FileImageSource>());
				Assert.That(((FileImageSource?)mobileReferringSite.FavIcon)?.File, Is.EqualTo(FavIconService.DefaultFavIcon));

				Assert.That(mobileReferringSite.FavIconImageUrl, Is.Not.Null);
				Assert.That(mobileReferringSite.FavIconImageUrl, Is.Empty);
				Assert.That(mobileReferringSite.IsReferrerUriValid, Is.False);
				Assert.That(mobileReferringSite.Referrer, Is.Not.Null);
				Assert.That(mobileReferringSite.Referrer, Is.Not.Empty);
				Assert.That(mobileReferringSite.ReferrerUri, Is.Null);
				Assert.That(Uri.IsWellFormedUriString(mobileReferringSite.ReferrerUri?.ToString(), UriKind.Absolute), Is.False);
				Assert.That(mobileReferringSite.TotalCount, Is.GreaterThanOrEqualTo(0));
				Assert.That(mobileReferringSite.TotalCount, Is.LessThanOrEqualTo(DemoDataConstants.MaximumRandomNumber));
				Assert.That(mobileReferringSite.TotalUniqueCount, Is.GreaterThanOrEqualTo(0));
				Assert.That(mobileReferringSite.TotalUniqueCount, Is.LessThanOrEqualTo(DemoDataConstants.MaximumRandomNumber));
			});
		}
	}

	[Test]
	public async Task GetMobileReferringSiteTest_InvalidRepo()
	{
		// Arrange
		MobileReferringSiteModel? mobileReferringSiteModel = null;
		var referringSite = new ReferringSiteModel(0, 0, string.Empty);

		var gitHubApiRepositoriesService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubApiRepositoriesService>();

		// Act 
		await foreach (var mobileReferringSite in gitHubApiRepositoriesService.GetMobileReferringSites(
		[
			referringSite
		], string.Empty, CancellationToken.None))
		{
			mobileReferringSiteModel = mobileReferringSite;
		}

		// Assert
		Assert.Multiple(() =>
		{
			Assert.That(FavIconService.DefaultFavIcon, Is.EqualTo("DefaultReferringSiteImage"));

			Assert.That(mobileReferringSiteModel?.FavIcon, Is.InstanceOf<FileImageSource>());
			Assert.That(((FileImageSource?)mobileReferringSiteModel?.FavIcon)?.File, Is.EqualTo(FavIconService.DefaultFavIcon));
		});
	}
}