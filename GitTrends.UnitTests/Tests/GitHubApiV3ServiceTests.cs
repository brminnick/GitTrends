using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace GitTrends.UnitTests
{
    public class GitHubApiV3ServiceTests
    {
        [Test]
        public async Task GetReferringSitesTest()
        {
            //Arrange
            var gitHubApiV3Service = ContainerService.Container.GetService<GitHubApiV3Service>();

            //Act
            throw new NotImplementedException();

            //Assert
        }

        [Test]
        public async Task GetRepositoryCloneStatisticsTest()
        {
            //Arrange
            var gitHubApiV3Service = ContainerService.Container.GetService<GitHubApiV3Service>();

            //Act
            throw new NotImplementedException();

            //Assert
        }

        [Test]
        public async Task GetRepositoryViewStatisticsTest()
        {
            //Arrange
            var gitHubApiV3Service = ContainerService.Container.GetService<GitHubApiV3Service>();

            //Act
            throw new NotImplementedException();

            //Assert
        }

        [Test]
        public async Task UpdateRepositoriesWithViewsAndClonesDataTest()
        {
            //Arrange
            var gitHubApiV3Service = ContainerService.Container.GetService<GitHubApiV3Service>();

            //Act
            throw new NotImplementedException();

            //Assert
        }
    }
}
