using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GitTrends.Mobile.Shared;
using NUnit.Framework;
using Xamarin.UITest;

namespace GitTrends.UITests
{
    abstract class RepositoriesTests : BaseTest
    {
        protected RepositoriesTests(Platform platform, UserType userType) : base(platform, userType)
        {
        }

        [Test]
        public async Task VerifyRepositoriesAfterLogin()
        {
            //Arrange
            IReadOnlyList<Repository> visibleRepositoryList;

            //Act
            RepositoryPage.TriggerPullToRefresh();
            await RepositoryPage.WaitForNoPullToRefresh().ConfigureAwait(false);

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
            await RepositoryPage.WaitForNoPullToRefresh().ConfigureAwait(false);

            //Assert
            visibleRepositoryList = RepositoryPage.GetVisibleRepositoryList();
            Assert.IsFalse(visibleRepositoryList.Any());
        }
    }
}
