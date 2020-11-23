using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GitTrends.Shared;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace GitTrends.UnitTests
{
    class GitTrendsContributorsServiceTests : BaseTest
    {
        [Test]
        public async Task InitializeContributorsTest()
        {
            //Arrange
            IReadOnlyList<Contributor> contributors_Initial, contributors_Final;
            var gitTrendsContributorsService = ServiceCollection.ServiceProvider.GetRequiredService<GitTrendsContributorsService>();

            //Act
            contributors_Initial = gitTrendsContributorsService.Contributors;

            await gitTrendsContributorsService.Initialize(CancellationToken.None).ConfigureAwait(false);

            contributors_Final = gitTrendsContributorsService.Contributors;

            //Assert
            Assert.IsNotNull(contributors_Initial);
            Assert.IsEmpty(contributors_Initial);

            Assert.IsNotEmpty(contributors_Final);
        }
    }
}
