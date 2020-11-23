using System;
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
            DateTimeOffset beforeTest, afterTest;
            var gitTrendsContributorsService = ServiceCollection.ServiceProvider.GetRequiredService<GitTrendsContributorsService>();

            //Act
            beforeTest = DateTimeOffset.UtcNow;
            contributors_Initial = gitTrendsContributorsService.Contributors;

            await gitTrendsContributorsService.Initialize(CancellationToken.None).ConfigureAwait(false);

            contributors_Final = gitTrendsContributorsService.Contributors;
            afterTest = DateTimeOffset.UtcNow;

            //Assert
            Assert.IsNotNull(contributors_Initial);
            Assert.IsEmpty(contributors_Initial);

            Assert.IsNotEmpty(contributors_Final);

            foreach (var contributor in contributors_Final)
            {
                var isAvatarUrlValid = Uri.TryCreate(contributor.AvatarUrl.ToString(), UriKind.Absolute, out _);
                var isGitHubUrlValid = Uri.TryCreate(contributor.GitHubUrl.ToString(), UriKind.Absolute, out _);

                Assert.IsTrue(isAvatarUrlValid);
                Assert.IsTrue(isGitHubUrlValid);
                Assert.Greater(contributor.ContributionCount, 0);
                Assert.Greater(afterTest, contributor.DataDownloadedAt);
                Assert.Less(beforeTest, contributor.DataDownloadedAt);
                Assert.IsFalse(string.IsNullOrEmpty(contributor.Login));
            }
        }
    }
}
