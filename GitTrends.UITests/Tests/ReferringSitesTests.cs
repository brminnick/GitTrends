using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GitTrends.Shared;
using NUnit.Framework;
using Xamarin.UITest;
using Xamarin.UITest.Android;
using Xamarin.UITest.iOS;

namespace GitTrends.UITests
{
    [TestFixture(Platform.Android, UserType.Demo)]
    [TestFixture(Platform.Android, UserType.LoggedIn)]
    [TestFixture(Platform.iOS, UserType.LoggedIn)]
    [TestFixture(Platform.iOS, UserType.Demo)]
    class ReferringSitesTests : BaseTest
    {
        public ReferringSitesTests(Platform platform, UserType userType) : base(platform, userType)
        {
        }

        public override async Task BeforeEachTest()
        {
            await base.BeforeEachTest().ConfigureAwait(false);

            var repositories = RepositoryPage.VisibleCollection;

            RepositoryPage.TapRepository(repositories.First().Name);

            await TrendsPage.WaitForPageToLoad().ConfigureAwait(false);
            TrendsPage.TapReferringSitesButton();

            await ReferringSitesPage.WaitForPageToLoad().ConfigureAwait(false);
        }

        [Test]
        public async Task ReferringSitesPageDoesLoad()
        {
            //Arrange
            IReadOnlyCollection<ReferringSiteModel> referringSiteList = ReferringSitesPage.VisibleCollection;
            var referringSite = referringSiteList.First();
            bool isUrlValid = referringSite.IsReferrerUriValid;

            //Act
            if (isUrlValid)
            {
                App.Tap(referringSite.Referrer);
                await Task.Delay(1000).ConfigureAwait(false);
            }

            //Assert
            Assert.IsTrue(referringSiteList.Any());

            if (isUrlValid && App is iOSApp)
            {
                SettingsPage.WaitForBrowserToOpen();
                Assert.IsTrue(ReferringSitesPage.IsBrowserOpen);
            }
            else if (!isUrlValid)
                Assert.IsTrue(App.Query(referringSite.Referrer).Any());

        }
    }
}
