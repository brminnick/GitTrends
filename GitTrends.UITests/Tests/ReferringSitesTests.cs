using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GitTrends.Shared;
using NUnit.Framework;
using Xamarin.UITest;

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

            var repositories = RepositoryPage.GetVisibleRepositoryList();

            RepositoryPage.TapRepository(repositories.First().Name);

            await TrendsPage.WaitForPageToLoad().ConfigureAwait(false);
            TrendsPage.TapReferringSitesButton();

            await ReferringSitesPage.WaitForPageToLoad().ConfigureAwait(false);
            ReferringSitesPage.WaitForNoActivityIndicator();
            ReferringSitesPage.WaitForNoActivityIndicator();
        }

        [Test]
        public void ReferringSitesPageDoesLoad()
        {
            //Arrange
            IReadOnlyList<ReferringSiteModel> referringSites;

            //Act

            //Assert
            referringSites = ReferringSitesPage.GetVisibleReferringSitesList();
            Assert.IsTrue(referringSites.Any());
        }
    }
}
