using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GitTrends.Mobile.Shared;
using GitTrends.Shared;
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
        }

        [Test]
        public async Task Login()
        {
            //Arrange
            IReadOnlyList<Repository> visibleRepositoryList;

            //Act
            await RepositoryPage.WaitForNoPullToRefresh().ConfigureAwait(false);

            visibleRepositoryList = RepositoryPage.GetVisibleRepositoryList();

            //Assert
            Assert.IsTrue(visibleRepositoryList.Any());

        }

        [Test]
        public void LogOut()
        {
            //Arrange

            //Act
            RepositoryPage.TapSettingsButton();

            //Assert
            Assert.AreEqual(GitHubLoginButtonConstants.Disconnect, SettingsPage.GitHubButtonText);

            //Act
            SettingsPage.TapGitHubButton();
            SettingsPage.WaitForGitHubLogoutToComplete();

            //Assert
            Assert.AreEqual(GitHubLoginButtonConstants.ConnectWithGitHub, SettingsPage.GitHubButtonText);
        }
    }
}
