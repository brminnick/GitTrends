using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GitTrends.Mobile.Shared;
using NUnit.Framework;
using Xamarin.UITest;

namespace GitTrends.UITests
{
    class LoginTests : BaseTest
    {
        public LoginTests(Platform platform) : base(platform)
        {

        }

        public override async Task BeforeEachTest()
        {
            await base.BeforeEachTest().ConfigureAwait(false);

            await LoginToGitHub().ConfigureAwait(false);

            RepositoryPage.WaitForPageToLoad();

            try
            {
                RepositoryPage.WaitForGitHubUserNotFoundPopup();
                RepositoryPage.AcceptGitHubUserNotFoundPopup();
            }
            catch
            {
                RepositoryPage.TapSettingsButton();
            }

            SettingsPage.WaitForPageToLoad();
            SettingsPage.DismissSyncfusionLicensePopup();

            SettingsPage.WaitForGitHubLoginToComplete();
            SettingsPage.TapBackButton();

            RepositoryPage.WaitForPageToLoad();
            await RepositoryPage.WaitForNoPullToRefresh().ConfigureAwait(false);
        }

        [Test]
        public async Task Login()
        {
            //Arrange
            IReadOnlyList<Repository> visibleRepositoryList;

            //Act
            await RepositoryPage.WaitForNoPullToRefresh().ConfigureAwait(false);

            //Assert
            visibleRepositoryList = RepositoryPage.GetVisibleRepositoryList();
            Assert.IsTrue(visibleRepositoryList.Any());

        }

        [Test]
        public async Task LogOut()
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
