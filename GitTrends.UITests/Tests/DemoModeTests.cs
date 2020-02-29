using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GitTrends.Mobile.Shared;
using NUnit.Framework;
using Xamarin.UITest;

namespace GitTrends.UITests
{
    class DemoModeTests : BaseTest
    {
        public DemoModeTests(Platform platform) : base(platform)
        {
        }

        public override async Task BeforeEachTest()
        {
            await base.BeforeEachTest().ConfigureAwait(false);

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

            SettingsPage.TapDemoModeButton();
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

            //Assert
            visibleRepositoryList = RepositoryPage.GetVisibleRepositoryList();
            Assert.IsFalse(visibleRepositoryList.Any());
        }
    }
}
