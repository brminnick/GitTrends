using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using GitTrends.Shared;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Refit;

namespace GitTrends.UnitTests
{
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
            await AuthenticateUser(gitHubUserService, githubGraphQLApiService).ConfigureAwait(false);

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
                await foreach (var retrievedRepositories in githubGraphQLApiService.GetRepositories(gitHubUserService.Alias, CancellationToken.None).ConfigureAwait(false))
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
            List<Repository> repositories = new List<Repository>();
            var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
            var githubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();
            var gitHubAuthenticationService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubAuthenticationService>();

            //Act
            await gitHubAuthenticationService.ActivateDemoUser().ConfigureAwait(false);

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
            List<Repository> repositories = new List<Repository>();
            var githubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();
            var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
            gitHubUserService.ShouldIncludeOrganizations = true;

            //Act
            await AuthenticateUser(gitHubUserService, githubGraphQLApiService).ConfigureAwait(false);

            await foreach (var repository in githubGraphQLApiService.GetRepositories(gitHubUserService.Alias, CancellationToken.None).ConfigureAwait(false))
            {
                repositories.Add(repository);
            }

            //Assert
            Assert.GreaterOrEqual(repositories.Count, 0);

            var gitTrendsRepository = repositories.Single(x => x.Name is GitHubConstants.GitTrendsRepoName
                                                                && x.OwnerLogin is GitHubConstants.GitTrendsRepoOwner);

            Assert.AreEqual(GitHubConstants.GitTrendsRepoName, gitTrendsRepository.Name);
            Assert.AreEqual(GitHubConstants.GitTrendsRepoOwner, gitTrendsRepository.OwnerLogin);
            Assert.AreEqual(AuthenticatedGitHubUserAvatarUrl, gitTrendsRepository.OwnerAvatarUrl);

            Assert.AreEqual(0, repositories.Sum(x => x.TotalViews));
            Assert.AreEqual(0, repositories.Sum(x => x.TotalUniqueViews));
            Assert.AreEqual(0, repositories.Sum(x => x.TotalClones));
            Assert.AreEqual(0, repositories.Sum(x => x.TotalUniqueClones));
            Assert.AreEqual(0, repositories.Sum(x => x.StarCount));
        }

        [Test]
        public void GetViewerOrganizationRepositoriesTest_Unauthenticated()
        {
            //Arrange
            List<Repository> repositories = new List<Repository>();
            var githubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();

            //Act
            var apiException = Assert.ThrowsAsync<ApiException>(async () =>
            {
                await foreach (var repository in githubGraphQLApiService.GetViewerOrganizationRepositories(CancellationToken.None).ConfigureAwait(false))
                {
                    repositories.Add(repository);
                }
            });

            //Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, apiException?.StatusCode);
            Assert.IsEmpty(repositories);
        }

        [Test]
        public async Task GetViewerOrganizationRepositoriesTest_Demo()
        {
            //Arrange
            List<Repository> repositories = new List<Repository>();
            var githubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();
            var gitHubAuthenticationService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubAuthenticationService>();

            await gitHubAuthenticationService.ActivateDemoUser().ConfigureAwait(false);

            //Act
            var apiException = Assert.ThrowsAsync<ApiException>(async () =>
            {
                await foreach (var repository in githubGraphQLApiService.GetViewerOrganizationRepositories(CancellationToken.None).ConfigureAwait(false))
                {
                    repositories.Add(repository);
                }
            });

            //Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, apiException?.StatusCode);
            Assert.IsEmpty(repositories);
        }

        [Test]
        public async Task GetViewerOrganizationRepositoriesTest_AuthenticatedUser()
        {
            //Arrange
            List<Repository> repositories = new List<Repository>();
            var githubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();
            var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();

            //Act
            await AuthenticateUser(gitHubUserService, githubGraphQLApiService).ConfigureAwait(false);

            await foreach (var repository in githubGraphQLApiService.GetViewerOrganizationRepositories(CancellationToken.None).ConfigureAwait(false))
            {
                repositories.Add(repository);
            }

            //Assert
            if (gitHubUserService.Alias is GitHubConstants.GitTrendsRepoOwner)
            {
                Assert.IsNotEmpty(repositories);
                Assert.GreaterOrEqual(repositories.Count, 100);
            }
            else
            {
                Assert.IsEmpty(repositories);
            }

            Assert.AreEqual(0, repositories.Sum(x => x.TotalViews));
            Assert.AreEqual(0, repositories.Sum(x => x.TotalUniqueViews));
            Assert.AreEqual(0, repositories.Sum(x => x.TotalClones));
            Assert.AreEqual(0, repositories.Sum(x => x.TotalUniqueClones));
            Assert.AreEqual(0, repositories.Sum(x => x.StarCount));
        }

        [Test]
        public void GetOrganizationRepositoriesTest_Unauthenticated()
        {
            //Arrange
            var githubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();

            //Act
            var apiException = Assert.ThrowsAsync<ApiException>(() => githubGraphQLApiService.GetOrganizationRepositories(nameof(GitTrends), CancellationToken.None));

            //Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, apiException?.StatusCode);
        }

        [Test]
        public async Task GetOrganizationRepositoriesTest_Demo()
        {
            //Arrange
            var githubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();
            var gitHubAuthenticationService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubAuthenticationService>();

            await gitHubAuthenticationService.ActivateDemoUser().ConfigureAwait(false);

            //Act
            var apiException = Assert.ThrowsAsync<ApiException>(() => githubGraphQLApiService.GetOrganizationRepositories(nameof(GitTrends), CancellationToken.None));

            //Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, apiException?.StatusCode);
        }

        [Test]
        public async Task GetOrganizationRepositoriesTest_AuthenticatedUser()
        {
            //Arrange
            IReadOnlyList<Repository> repositories;
            var githubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();
            var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();

            //Act
            await AuthenticateUser(gitHubUserService, githubGraphQLApiService).ConfigureAwait(false);

            repositories = await githubGraphQLApiService.GetOrganizationRepositories(nameof(GitTrends), CancellationToken.None).ConfigureAwait(false);

            //Assert
            Assert.IsNotEmpty(repositories);
            Assert.Greater(repositories.Count, 0);

            Assert.AreEqual(0, repositories.Sum(x => x.TotalViews));
            Assert.AreEqual(0, repositories.Sum(x => x.TotalUniqueViews));
            Assert.AreEqual(0, repositories.Sum(x => x.TotalClones));
            Assert.AreEqual(0, repositories.Sum(x => x.TotalUniqueClones));
            Assert.AreEqual(0, repositories.Sum(x => x.StarCount));
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
            await gitHubAuthenticationService.ActivateDemoUser().ConfigureAwait(false);
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

            await AuthenticateUser(gitHubUserService, githubGraphQLApiService).ConfigureAwait(false);

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

            await gitHubAuthenticationService.ActivateDemoUser().ConfigureAwait(false);

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

            await AuthenticateUser(gitHubUserService, githubGraphQLApiService).ConfigureAwait(false);

            //Act
            starGazers = await githubGraphQLApiService.GetStarGazers(GitHubConstants.GitTrendsRepoName, GitHubConstants.GitTrendsRepoOwner, CancellationToken.None).ConfigureAwait(false);

            //Assert
            Assert.IsNotEmpty(starGazers.StarredAt);
            Assert.Greater(starGazers.StarredAt.Count, 400);
            Assert.AreEqual(starGazers.TotalCount, starGazers.StarredAt.Count);
        }
    }
}
