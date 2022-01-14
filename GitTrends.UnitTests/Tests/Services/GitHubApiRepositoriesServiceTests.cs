using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using GitTrends.Mobile.Common;
using GitTrends.Shared;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Refit;
using Xamarin.Forms;

namespace GitTrends.UnitTests
{
	class GitHubApiRepositoriesServiceTests : BaseTest
	{
		public override async Task Setup()
		{
			await base.Setup().ConfigureAwait(false);

			var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
			var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();

			await AuthenticateUser(gitHubUserService, gitHubGraphQLApiService).ConfigureAwait(false);
		}

		[Test]
		public async Task UpdateRepositoriesWithViewsAndClonesData_ValidRepo()
		{
			//Arrange
			IReadOnlyList<Repository> repositories_NoViewsClonesData_Filtered;

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

			repositories_NoViewsClonesData_Filtered = RepositoryService.RemoveForksAndDuplicates(repositories_NoViewsClonesData).ToList();

			await foreach (var repository in gitHubApiRepositoriesService.UpdateRepositoriesWithViewsClonesAndStarsData(repositories_NoViewsClonesData_Filtered, CancellationToken.None).ConfigureAwait(false))
			{
				repositories.Add(repository);
			}

			//Assert
			Assert.IsNotEmpty(repositories);
			Assert.IsNotEmpty(repositories_NoViewsClonesData);

			foreach (var repository in repositories_NoViewsClonesData)
			{
				Assert.IsNull(repository.StarredAt);
				Assert.IsNull(repository.TotalClones);
				Assert.IsNull(repository.TotalUniqueClones);
				Assert.IsNull(repository.TotalViews);
				Assert.IsNull(repository.TotalUniqueViews);
			}

			Assert.IsNotEmpty(repositories.SelectMany(x => x.StarredAt));
			Assert.IsNotEmpty(repositories.SelectMany(x => x.DailyViewsList));
			Assert.IsNotEmpty(repositories.SelectMany(x => x.DailyClonesList));

			Assert.Less(0, repositories.SelectMany(x => x.StarredAt).Count());
			Assert.Less(0, repositories.Sum(x => x.TotalClones));
			Assert.Less(0, repositories.Sum(x => x.TotalUniqueClones));
			Assert.Less(0, repositories.Sum(x => x.TotalViews));
			Assert.Less(0, repositories.Sum(x => x.TotalUniqueViews));
		}

		[Test]
		public async Task UpdateRepositoriesWithViewsClonesAndStarsData_ValidRepo_NotFiltered()
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
				await foreach (var repository in gitHubApiRepositoriesService.UpdateRepositoriesWithViewsClonesAndStarsData(repositories_NoViewsClonesData, CancellationToken.None).ConfigureAwait(false))
				{
					repositories.Add(repository);
				}
			});

			//Assert
			Assert.IsTrue(exception?.Message.Contains("more than one matching element"));
		}

		[Test]
		public async Task UpdateRepositoriesWithViewsClonesAndStarsData_EmptyRepos()
		{
			//Arrange
			var repositories = new List<Repository>();

			var gitHubApiRepositoriesService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubApiRepositoriesService>();

			//Act
			await foreach (var repository in gitHubApiRepositoriesService.UpdateRepositoriesWithViewsClonesAndStarsData(new List<Repository>(), CancellationToken.None).ConfigureAwait(false))
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
			IReadOnlyList<Repository> repositories_NoViewsClonesData_Filtered;
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

			repositories_NoViewsClonesData_Filtered = RepositoryService.RemoveForksAndDuplicates(repositories_NoViewsClonesData).ToList();

			gitHubUserService.InvalidateToken();

			await foreach (var repository in gitHubApiRepositoriesService.UpdateRepositoriesWithViewsClonesAndStarsData(repositories_NoViewsClonesData_Filtered, CancellationToken.None).ConfigureAwait(false))
			{
				repositories.Add(repository);
			};

			//Assert
			Assert.IsEmpty(repositories.SelectMany(x => x.StarredAt));
			Assert.IsEmpty(repositories.SelectMany(x => x.DailyViewsList));
			Assert.IsEmpty(repositories.SelectMany(x => x.DailyClonesList));
		}

		[Test]
		public async Task UpdateRepositoriesWithViewsClonesAndStarsData_ValidRepo_Authenticated()
		{
			//Arrange
			IReadOnlyList<Repository> repositories_NoViewsClonesData_Filtered;
			var repositories = new List<Repository>();
			var repositories_NoViewsClonesData = new List<Repository>();

			var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
			var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();
			var gitHubApiRepositoriesService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubApiRepositoriesService>();

			await AuthenticateUser(gitHubUserService, gitHubGraphQLApiService).ConfigureAwait(false);

			//Act
			await foreach (var repository in gitHubGraphQLApiService.GetRepositories(gitHubUserService.Alias, CancellationToken.None).ConfigureAwait(false))
			{
				repositories_NoViewsClonesData.Add(repository);
			}

			repositories_NoViewsClonesData_Filtered = RepositoryService.RemoveForksAndDuplicates(repositories_NoViewsClonesData).ToList();

			await foreach (var repository in gitHubApiRepositoriesService.UpdateRepositoriesWithViewsClonesAndStarsData(repositories_NoViewsClonesData_Filtered, CancellationToken.None).ConfigureAwait(false))
			{
				repositories.Add(repository);
			};

			//Assert
			Assert.AreEqual(repositories_NoViewsClonesData_Filtered.Count, repositories.Count);

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
												GitHubConstants.GitTrendsAvatarUrl, 1, 2, "https://github.com/brminnick/gittrends", false, DateTimeOffset.UtcNow, RepositoryPermission.ADMIN);

			var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
			var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();
			var gitHubApiRepositoriesService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubApiRepositoriesService>();

			await AuthenticateUser(gitHubUserService, gitHubGraphQLApiService).ConfigureAwait(false);

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
												GitHubConstants.GitTrendsAvatarUrl, 1, 2, "https://github.com/brminnick/gittrends", false, DateTimeOffset.UtcNow, RepositoryPermission.ADMIN);

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
												GitHubConstants.GitTrendsAvatarUrl, 1, 2, "https://github.com/brminnick/gittrends", false, DateTimeOffset.UtcNow, RepositoryPermission.ADMIN);

			var gitHubAuthenticationService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubAuthenticationService>();
			var gitHubApiRepositoriesService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubApiRepositoriesService>();

			await gitHubAuthenticationService.ActivateDemoUser().ConfigureAwait(false);

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
												string.Empty, 1, 2, string.Empty, false, DateTimeOffset.UtcNow, RepositoryPermission.ADMIN);

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
			List<MobileReferringSiteModel> mobileReferringSiteModels = new();

			var repository = new Repository(GitHubConstants.GitTrendsRepoName, GitHubConstants.GitTrendsRepoName, 1, GitHubConstants.GitTrendsRepoOwner,
												GitHubConstants.GitTrendsAvatarUrl, 1, 2, "https://github.com/brminnick/gittrends", false, DateTimeOffset.UtcNow, RepositoryPermission.ADMIN);

			var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
			var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();
			var gitHubApiRepositoriesService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubApiRepositoriesService>();

			await AuthenticateUser(gitHubUserService, gitHubGraphQLApiService).ConfigureAwait(false);

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
					Assert.AreEqual(FavIconService.DefaultFavIcon, ((Xamarin.Forms.FileImageSource?)mobileReferringSite.FavIcon)?.File);
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
			List<MobileReferringSiteModel> mobileReferringSiteModels = new();

			var repository = new Repository(GitHubConstants.GitTrendsRepoName, GitHubConstants.GitTrendsRepoName, 1, GitHubConstants.GitTrendsRepoOwner,
												GitHubConstants.GitTrendsAvatarUrl, 1, 2, "https://github.com/brminnick/gittrends", false, DateTimeOffset.UtcNow, RepositoryPermission.ADMIN);

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
					Assert.AreEqual(FavIconService.DefaultFavIcon, ((Xamarin.Forms.FileImageSource?)mobileReferringSite.FavIcon)?.File);
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
			List<MobileReferringSiteModel> mobileReferringSiteModels = new();

			var repository = new Repository(GitHubConstants.GitTrendsRepoName, GitHubConstants.GitTrendsRepoName, 1, GitHubConstants.GitTrendsRepoOwner,
												GitHubConstants.GitTrendsAvatarUrl, 1, 2, "https://github.com/brminnick/gittrends", false, DateTimeOffset.UtcNow, RepositoryPermission.ADMIN);

			var gitHubAuthenticationService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubAuthenticationService>();
			var gitHubApiRepositoriesService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubApiRepositoriesService>();

			await gitHubAuthenticationService.ActivateDemoUser().ConfigureAwait(false);

			//Act
			referringSiteModels = await gitHubApiRepositoriesService.GetReferringSites(repository, CancellationToken.None).ConfigureAwait(false);
			await foreach (var mobileReferringSite in gitHubApiRepositoriesService.GetMobileReferringSites(referringSiteModels, repository.Url, CancellationToken.None).ConfigureAwait(false))
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