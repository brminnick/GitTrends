using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using GitTrends.Shared;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
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
        public async Task GetReferringSitesTest_ValidRepo_Authenticated()
        {
            // Arrange
            var gitHubApiRepositoriesService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubApiRepositoriesService>();

            throw new NotImplementedException();
        }

        [Test]
        public async Task GetReferringSitesTest_ValidRepo_NotAuthenticated()
        {
            // Arrange
            var gitHubApiRepositoriesService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubApiRepositoriesService>();

            throw new NotImplementedException();
        }

        [Test]
        public async Task GetReferringSitesTest_ValidRepo_DemoUser()
        {
            // Arrange
            var gitHubApiRepositoriesService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubApiRepositoriesService>();

            // Act 
            throw new NotImplementedException();
        }

        [Test]
        public async Task GetReferringSitesTest_InvalidRepo()
        {
            // Arrange
            var gitHubApiRepositoriesService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubApiRepositoriesService>();

            // Act 
            throw new NotImplementedException();
        }

        [Test]
        public async Task GetMobileReferringSiteTest_ValidRepo_Authenticated()
        {
            // Arrange
            var gitHubApiRepositoriesService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubApiRepositoriesService>();

            // Act 
            throw new NotImplementedException();
        }

        [Test]
        public async Task GetMobileReferringSiteTest_ValidRepo_NotAuthenticated()
        {
            // Arrange
            var gitHubApiRepositoriesService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubApiRepositoriesService>();

            // Act 
            throw new NotImplementedException();
        }

        [Test]
        public async Task GetMobileReferringSiteTest_ValidRepo_DemoUser()
        {
            // Arrange
            var gitHubApiRepositoriesService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubApiRepositoriesService>();

            // Act 
            throw new NotImplementedException();
        }

        [Test]
        public async Task GetMobileReferringSiteTest_InvalidRepo()
        {
            // Arrange
            var gitHubApiRepositoriesService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubApiRepositoriesService>();

            // Act 
            throw new NotImplementedException();
        }
    }
}
