using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Xamarin.UITest;

namespace GitTrends.UITests
{
    [TestFixture(Platform.Android, UserType.LoggedIn)]
    [TestFixture(Platform.iOS, UserType.LoggedIn)]
    class AppStoreScreenShots : BaseTest
    {
        public AppStoreScreenShots(Platform platform, UserType userType) : base(platform, userType)
        {
        }

        [Test]
        public async Task AppStoreScreenShotsTest()
        {
            //Arrange
            var screenRect = App.Query().First().Rect;

            //Act
            App.Screenshot("Repository Page");

            RepositoryPage.TapSettingsButton();
            await SettingsPage.WaitForPageToLoad().ConfigureAwait(false);

            App.Screenshot("Settings Page");

            SettingsPage.TapBackButton();

            RepositoryPage.TapRepository(RepositoryPage.VisibleCollection.Skip(2).First().Name);

            await TrendsPage.WaitForPageToLoad().ConfigureAwait(false);

            App.TouchAndHoldCoordinates(screenRect.CenterX, screenRect.CenterY);
            App.Screenshot("Trends Page");

            TrendsPage.TapReferringSitesButton();
            await ReferringSitesPage.WaitForPageToLoad().ConfigureAwait(false);

            App.Screenshot("Referring Sites Page");

            //Assert
        }
    }
}
