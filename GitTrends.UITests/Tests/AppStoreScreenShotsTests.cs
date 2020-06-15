using System.Linq;
using System.Threading.Tasks;
using GitTrends.Mobile.Common;
using NUnit.Framework;
using Xamarin.UITest;

namespace GitTrends.UITests
{
    [TestFixture(Platform.Android, UserType.Neither)]
    [TestFixture(Platform.iOS, UserType.Neither)]
    class AppStoreScreenShotsTests : BaseUITest
    {
        public AppStoreScreenShotsTests(Platform platform, UserType userType) : base(platform, userType)
        {
        }

        [Test]
        public async Task AppStoreScreenShotsTest()
        {
            //Arrange
            var screenRect = App.Query().First().Rect;

            //Act
            await OnboardingPage.WaitForPageToLoad().ConfigureAwait(false);
            App.Screenshot("GitTrends Onboarding Page");

            await OnboardingPage.MoveToNextPage().ConfigureAwait(false);
            App.Screenshot("Charts Onboarding Page");

            await OnboardingPage.MoveToNextPage().ConfigureAwait(false);
            App.Screenshot("Notifications Onboarding Page");

            await OnboardingPage.MoveToNextPage().ConfigureAwait(false);
            App.Screenshot("Connect to GitHub Onboarding Page");

            await SetupLoggedInUser().ConfigureAwait(false);

            App.Screenshot("Repository Page Light");

            RepositoryPage.TapRepository(nameof(GitTrends));

            await TrendsPage.WaitForPageToLoad().ConfigureAwait(false);

            App.TouchAndHoldCoordinates(screenRect.CenterX, screenRect.CenterY);
            App.Screenshot("Trends Page Light");

            TrendsPage.TapReferringSitesButton();
            await ReferringSitesPage.WaitForPageToLoad().ConfigureAwait(false);

            App.Screenshot("Referring Sites Page Light");

            ReferringSitesPage.ClosePage();
            await TrendsPage.WaitForPageToLoad().ConfigureAwait(false);

            TrendsPage.TapBackButton();
            await RepositoryPage.WaitForPageToLoad().ConfigureAwait(false);

            RepositoryPage.TapSettingsButton();
            await SettingsPage.WaitForPageToLoad().ConfigureAwait(false);

            App.Screenshot("Settings Page Light");

            await SettingsPage.SelectTheme(PreferredTheme.Dark).ConfigureAwait(false);
            App.Screenshot("Settings Page Dark");

            SettingsPage.TapBackButton();
            await RepositoryPage.WaitForPageToLoad().ConfigureAwait(false);

            App.Screenshot("Repository Page Dark");

            RepositoryPage.TapRepository(nameof(GitTrends));

            await TrendsPage.WaitForPageToLoad().ConfigureAwait(false);

            App.TouchAndHoldCoordinates(screenRect.CenterX, screenRect.CenterY);
            App.Screenshot("Trends Page Dark");

            TrendsPage.TapReferringSitesButton();
            await ReferringSitesPage.WaitForPageToLoad().ConfigureAwait(false);

            App.Screenshot("Referring Sites Page Dark");

            //Assert
        }
    }
}
