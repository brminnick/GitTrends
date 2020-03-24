using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GitTrends.Mobile.Shared;
using GitTrends.Shared;
using NUnit.Framework;
using Xamarin.UITest;

namespace GitTrends.UITests
{
    [TestFixture(Platform.Android, UserType.Demo)]
    [TestFixture(Platform.Android, UserType.LoggedIn)]
    [TestFixture(Platform.iOS, UserType.LoggedIn)]
    [TestFixture(Platform.iOS, UserType.Demo)]
    class RepositoriesTests : BaseTest
    {
        public RepositoriesTests(Platform platform, UserType userType) : base(platform, userType)
        {
        }

        [TestCase(SortingConstants.DefaultSortingOption)]
        [TestCase(SortingOption.Clones)]
        [TestCase(SortingOption.Forks)]
        [TestCase(SortingOption.Issues)]
        [TestCase(SortingOption.Stars)]
        [TestCase(SortingOption.UniqueClones)]
        [TestCase(SortingOption.UniqueViews)]
        [TestCase(SortingOption.Views)]
        [TestCase(SortingOption.Trending)]
        public async Task VerifySortingOptions(SortingOption sortingOption)
        {
            //Arrange
            Repository finalTopRepository;
            Repository finalSecondTopRepository;
            Repository initialTopRepository = RepositoryPage.GetVisibleRepositoryList().First();
            Repository initialSecondTopRepository = RepositoryPage.GetVisibleRepositoryList().Skip(1).First();

            //Act
            RepositoryPage.SetSortingOption(sortingOption);

            //Allow RepositoryList to update
            await Task.Delay(1000).ConfigureAwait(false);

            //Assert
            finalTopRepository = RepositoryPage.GetVisibleRepositoryList().First();
            finalSecondTopRepository = RepositoryPage.GetVisibleRepositoryList().Skip(1).First();

            if (initialTopRepository.IsTrending && initialSecondTopRepository.IsTrending
                || !initialTopRepository.IsTrending && !initialSecondTopRepository.IsTrending)
            {
                Assert.GreaterOrEqual(initialSecondTopRepository.TotalViews, initialSecondTopRepository.TotalViews);
            }

            Assert.AreNotEqual(initialTopRepository, finalTopRepository);
            Assert.AreNotEqual(initialSecondTopRepository, finalSecondTopRepository);
            Assert.AreNotEqual(initialTopRepository, initialSecondTopRepository);
            Assert.AreNotEqual(finalTopRepository, finalSecondTopRepository);

            switch (sortingOption)
            {
                case SortingOption.Stars:
                    Assert.GreaterOrEqual(finalTopRepository.StarCount, finalSecondTopRepository.StarCount);
                    break;
                case SortingOption.Clones:
                    Assert.GreaterOrEqual(finalTopRepository.TotalClones, finalSecondTopRepository.TotalClones);
                    break;
                case SortingOption.Forks:
                    Assert.GreaterOrEqual(finalTopRepository.ForkCount, finalSecondTopRepository.ForkCount);
                    break;
                case SortingOption.Issues:
                    Assert.GreaterOrEqual(finalTopRepository.IssuesCount, finalSecondTopRepository.IssuesCount);
                    break;
                case SortingOption.UniqueClones:
                    Assert.GreaterOrEqual(finalTopRepository.TotalUniqueClones, finalSecondTopRepository.TotalUniqueClones);
                    break;
                case SortingOption.UniqueViews:
                    Assert.GreaterOrEqual(finalTopRepository.TotalUniqueViews, finalSecondTopRepository.TotalUniqueViews);
                    break;
                case SortingOption.Views:
                    Assert.GreaterOrEqual(finalTopRepository.TotalViews, finalSecondTopRepository.TotalViews);
                    break;
                case SortingOption.Trending:
                    Assert.LessOrEqual(finalTopRepository.TotalViews, finalSecondTopRepository.TotalViews);
                    break;
                default:
                    throw new NotSupportedException();

            };
        }

        [Test]
        public async Task VerifyRepositoriesAfterLogin()
        {
            //Arrange
            IReadOnlyList<Repository> visibleRepositoryList;

            //Act
            RepositoryPage.TriggerPullToRefresh();
            await RepositoryPage.WaitForNoPullToRefreshIndicator().ConfigureAwait(false);

            //Assert
            visibleRepositoryList = RepositoryPage.GetVisibleRepositoryList();
            Assert.IsTrue(visibleRepositoryList.Any());

        }

        [Test]
        public async Task VerifyNoRepositoriesAfterLogOut()
        {
            //Arrange
            IReadOnlyList<Repository> visibleRepositoryList;

            //Act
            RepositoryPage.TapSettingsButton();

            //Assert
            Assert.AreEqual(GitHubLoginButtonConstants.Disconnect, SettingsPage.GitHubButtonText);

            //Act
            SettingsPage.TapGitHubButton();
            SettingsPage.WaitForGitHubLogoutToComplete();

            //Assert
            Assert.AreEqual(GitHubLoginButtonConstants.ConnectWithGitHub, SettingsPage.GitHubButtonText);

            //Act
            SettingsPage.TapBackButton();

            RepositoryPage.DeclineGitHubUserNotFoundPopup();
            RepositoryPage.TriggerPullToRefresh();

            await RepositoryPage.WaitForPullToRefreshIndicator().ConfigureAwait(false);
            await RepositoryPage.WaitForNoPullToRefreshIndicator().ConfigureAwait(false);

            //Assert
            visibleRepositoryList = RepositoryPage.GetVisibleRepositoryList();
            Assert.IsFalse(visibleRepositoryList.Any());
        }
    }
}
