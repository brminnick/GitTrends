using System.Threading.Tasks;
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
        public void Login()
        {

        }
    }
}
