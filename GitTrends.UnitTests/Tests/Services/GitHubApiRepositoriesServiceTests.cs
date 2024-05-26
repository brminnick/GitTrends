using System.Net;
using GitTrends.Mobile.Common;
using GitTrends.Shared;
using Refit;

namespace GitTrends.UnitTests
{
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

			repositories_NoViewsClonesStarsData_Filtered = repositories_NoViewsClonesStarsData.RemoveForksDuplicatesAndArchives().ToList();

			await foreach (var repository in gitHubApiRepositoriesService.UpdateRepositoriesWithViewsAndClonesData(repositories_NoViewsClonesStarsData_Filtered, CancellationToken.None).ConfigureAwait(false))
			{
				repositories_NoStarsData.Add(repository);
			}

			repositories_Filtered = repositories_NoStarsData.RemoveForksDuplicatesAndArchives().ToList();

			await foreach (var repository in gitHubApiRepositoriesService.UpdateRepositoriesWithStarsData(repositories_Filtered, CancellationToken.None).ConfigureAwait(false))
			{
				repositories.Add(repository);
			}

			//Assert
			Assert.IsNotEmpty(repositories);
			Assert.IsNotEmpty(repositories_NoStarsData);
			Assert.IsNotEmpty(repositories_NoViewsClonesStarsData);

			foreach (var repository in repositories_NoViewsClonesStarsData)
			{
				Assert.IsNull(repository.StarredAt);
				Assert.IsNull(repository.TotalClones);
				Assert.IsNull(repository.TotalUniqueClones);
				Assert.IsNull(repository.TotalViews);
				Assert.IsNull(repository.TotalUniqueViews);
			}

			foreach (var repository in repositories_NoStarsData)
			{
				Assert.IsNull(repository.StarredAt);
				Assert.IsNotNull(repository.TotalClones);
				Assert.IsNotNull(repository.TotalUniqueClones);
				Assert.IsNotNull(repository.TotalViews);
				Assert.IsNotNull(repository.TotalUniqueViews);
			}

			Assert.IsNotEmpty(repositories.SelectMany(static x => x.StarredAt ?? throw new InvalidOperationException()));
			Assert.IsNotEmpty(repositories.SelectMany(static x => x.DailyViewsList ?? throw new InvalidOperationException()));
			Assert.IsNotEmpty(repositories.SelectMany(static x => x.DailyClonesList ?? throw new InvalidOperationException()));

			Assert.Less(0, repositories.SelectMany(static x => x.StarredAt ?? throw new InvalidOperationException()).Count());
			Assert.Less(0, repositories.Sum(static x => x.TotalClones));
			Assert.Less(0, repositories.Sum(static x => x.TotalUniqueClones));
			Assert.Less(0, repositories.Sum(static x => x.TotalViews));
			Assert.Less(0, repositories.Sum(static x => x.TotalUniqueViews));
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
			Assert.IsTrue(exception?.Message.Contains("more than one matching element"));
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
			Assert.IsTrue(exception?.Message.Contains("more than one matching element"));
		}

		[Test]
		public async Task UpdateRepositoriesWithViewsClonesData_EmptyRepos()
		{
			//Arrange
			var repositories = new List<Repository>();

			var gitHubApiRepositoriesService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubApiRepositoriesService>();

			//Act
			await foreach (var repository in gitHubApiRepositoriesService.UpdateRepositoriesWithViewsAndClonesData(new List<Repository>(), CancellationToken.None).ConfigureAwait(false))
			{
				repositories.Add(repository);
			}

			//Assert
			Assert.IsEmpty(repositories);
		}

		[Test]
		public async Task UpdateRepositoriesWithViewsClonesStarsData_EmptyRepos()
		{
			//Arrange
			var repositories = new List<Repository>();

			var gitHubApiRepositoriesService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubApiRepositoriesService>();

			//Act
			await foreach (var repository in gitHubApiRepositoriesService.UpdateRepositoriesWithStarsData(new List<Repository>(), CancellationToken.None).ConfigureAwait(false))
			{
				repositories.Add(repository);
			}

			//Assert
			Assert.IsEmpty(repositories);
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

			repositories_NoViewsClonesStarsData_Filtered = repositories_NoViewsClonesStarsData.RemoveForksDuplicatesAndArchives().ToList();

			gitHubUserService.InvalidateToken();

			await foreach (var repository in gitHubApiRepositoriesService.UpdateRepositoriesWithViewsAndClonesData(repositories_NoViewsClonesStarsData_Filtered, CancellationToken.None).ConfigureAwait(false))
			{
				repositories_NoStarsData.Add(repository);
			};

			repositories_NoStarsData_Filtered = repositories_NoStarsData.RemoveForksDuplicatesAndArchives().ToList();

			await foreach (var repository in gitHubApiRepositoriesService.UpdateRepositoriesWithStarsData(repositories_NoStarsData_Filtered, CancellationToken.None).ConfigureAwait(false))
			{
				repositories.Add(repository);
			};

			//Assert
			Assert.IsEmpty(repositories.SelectMany(static x => x.StarredAt ?? throw new InvalidOperationException()));
			Assert.IsEmpty(repositories.SelectMany(static x => x.DailyViewsList ?? throw new InvalidOperationException()));
			Assert.IsEmpty(repositories.SelectMany(static x => x.DailyClonesList ?? throw new InvalidOperationException()));
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

			repositories_NoViewsClonesStarsData_Filtered = repositories_NoViewsClonesStarsData.RemoveForksDuplicatesAndArchives().ToList();

			await foreach (var repository in gitHubApiRepositoriesService.UpdateRepositoriesWithViewsAndClonesData(repositories_NoViewsClonesStarsData_Filtered, CancellationToken.None).ConfigureAwait(false))
			{
				repositories_NoStarsData.Add(repository);
			};

			repositories_NoStarsData_Filtered = repositories_NoStarsData.RemoveForksDuplicatesAndArchives().ToList();

			await foreach (var repository in gitHubApiRepositoriesService.UpdateRepositoriesWithStarsData(repositories_NoStarsData_Filtered, CancellationToken.None).ConfigureAwait(false))
			{
				repositories.Add(repository);
			};

			//Assert
			Assert.AreEqual(repositories_NoViewsClonesStarsData_Filtered.Count, repositories.Count);

			foreach (var repository in repositories)
			{
				Assert.IsNotNull(repository.DailyClonesList);
				Assert.IsNotNull(repository.DailyViewsList);
				Assert.IsNotNull(repository.StarredAt);
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
				Assert.IsTrue(referringSite.IsReferrerUriValid);
				Assert.IsNotNull(referringSite.Referrer);
				Assert.IsNotEmpty(referringSite.Referrer);
				Assert.IsNotNull(referringSite.ReferrerUri);
				Assert.IsTrue(Uri.IsWellFormedUriString(referringSite.ReferrerUri?.ToString(), UriKind.Absolute));
				Assert.Greater(referringSite.TotalCount, 0);
				Assert.Greater(referringSite.TotalUniqueCount, 0);
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
				Assert.IsTrue(referringSite.IsReferrerUriValid);
				Assert.IsNotNull(referringSite.Referrer);
				Assert.IsNotEmpty(referringSite.Referrer);
				Assert.IsNotNull(referringSite.ReferrerUri);
				Assert.IsTrue(Uri.IsWellFormedUriString(referringSite.ReferrerUri?.ToString(), UriKind.Absolute));
				Assert.Greater(referringSite.TotalCount, 0);
				Assert.Greater(referringSite.TotalUniqueCount, 0);
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
				Assert.IsFalse(referringSite.IsReferrerUriValid);
				Assert.IsNotNull(referringSite.Referrer);
				Assert.IsNotEmpty(referringSite.Referrer);
				Assert.IsNull(referringSite.ReferrerUri);
				Assert.IsFalse(Uri.IsWellFormedUriString(referringSite.ReferrerUri?.ToString(), UriKind.Absolute));
				Assert.GreaterOrEqual(referringSite.TotalCount, 0);
				Assert.LessOrEqual(referringSite.TotalCount, DemoDataConstants.MaximumRandomNumber);
				Assert.GreaterOrEqual(referringSite.TotalUniqueCount, 0);
				Assert.LessOrEqual(referringSite.TotalUniqueCount, DemoDataConstants.MaximumRandomNumber);
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
			Assert.AreEqual(HttpStatusCode.NotFound, exception?.StatusCode);
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
				Assert.IsNotNull(mobileReferringSite.FavIcon);
				Assert.IsNotNull(mobileReferringSite.FavIconImageUrl);
				if (string.Empty == mobileReferringSite.FavIconImageUrl)
					Assert.AreEqual(FavIconService.DefaultFavIcon, ((FileImageSource?)mobileReferringSite.FavIcon)?.File);
				else
					Assert.IsNotEmpty(mobileReferringSite.FavIconImageUrl);
				Assert.IsTrue(mobileReferringSite.IsReferrerUriValid);
				Assert.IsNotNull(mobileReferringSite.Referrer);
				Assert.IsNotEmpty(mobileReferringSite.Referrer);
				Assert.IsNotNull(mobileReferringSite.ReferrerUri);
				Assert.IsTrue(Uri.IsWellFormedUriString(mobileReferringSite.ReferrerUri?.ToString(), UriKind.Absolute));
				Assert.Greater(mobileReferringSite.TotalCount, 0);
				Assert.Greater(mobileReferringSite.TotalUniqueCount, 0);
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
				Assert.IsNotNull(mobileReferringSite.FavIcon);
				Assert.IsNotNull(mobileReferringSite.FavIconImageUrl);
				if (string.Empty == mobileReferringSite.FavIconImageUrl)
					Assert.AreEqual(FavIconService.DefaultFavIcon, ((FileImageSource?)mobileReferringSite.FavIcon)?.File);
				else
					Assert.IsNotEmpty(mobileReferringSite.FavIconImageUrl);
				Assert.IsTrue(mobileReferringSite.IsReferrerUriValid);
				Assert.IsNotNull(mobileReferringSite.Referrer);
				Assert.IsNotEmpty(mobileReferringSite.Referrer);
				Assert.IsNotNull(mobileReferringSite.ReferrerUri);
				Assert.IsTrue(Uri.IsWellFormedUriString(mobileReferringSite.ReferrerUri?.ToString(), UriKind.Absolute));
				Assert.Greater(mobileReferringSite.TotalCount, 0);
				Assert.Greater(mobileReferringSite.TotalUniqueCount, 0);
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
				Assert.AreEqual("DefaultReferringSiteImage", FavIconService.DefaultFavIcon);

				Assert.IsInstanceOf<FileImageSource>(mobileReferringSite.FavIcon);
				Assert.AreEqual(FavIconService.DefaultFavIcon, ((FileImageSource?)mobileReferringSite.FavIcon)?.File);

				Assert.IsNotNull(mobileReferringSite.FavIconImageUrl);
				Assert.IsEmpty(mobileReferringSite.FavIconImageUrl);
				Assert.IsFalse(mobileReferringSite.IsReferrerUriValid);
				Assert.IsNotNull(mobileReferringSite.Referrer);
				Assert.IsNotEmpty(mobileReferringSite.Referrer);
				Assert.IsNull(mobileReferringSite.ReferrerUri);
				Assert.IsFalse(Uri.IsWellFormedUriString(mobileReferringSite.ReferrerUri?.ToString(), UriKind.Absolute));
				Assert.GreaterOrEqual(mobileReferringSite.TotalCount, 0);
				Assert.LessOrEqual(mobileReferringSite.TotalCount, DemoDataConstants.MaximumRandomNumber);
				Assert.GreaterOrEqual(mobileReferringSite.TotalUniqueCount, 0);
				Assert.LessOrEqual(mobileReferringSite.TotalUniqueCount, DemoDataConstants.MaximumRandomNumber);
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
			await foreach (var mobileReferringSite in gitHubApiRepositoriesService.GetMobileReferringSites(new List<ReferringSiteModel> { referringSite }, string.Empty, CancellationToken.None))
			{
				mobileReferringSiteModel = mobileReferringSite;
			}

			Assert.AreEqual("DefaultReferringSiteImage", FavIconService.DefaultFavIcon);

			Assert.IsInstanceOf<FileImageSource>(mobileReferringSiteModel?.FavIcon);
			Assert.AreEqual(FavIconService.DefaultFavIcon, ((FileImageSource?)mobileReferringSiteModel?.FavIcon)?.File);
		}
	}
}