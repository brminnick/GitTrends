using System.Linq;
using System.Threading.Tasks;
using GitTrends.Mobile.Shared;
using NUnit.Framework;
using Xamarin.UITest;
using Xamarin.UITest.iOS;

namespace GitTrends.UITests
{
    [TestFixture(Platform.Android, UserType.Demo)]
    [TestFixture(Platform.Android, UserType.LoggedIn)]
    [TestFixture(Platform.iOS, UserType.LoggedIn)]
    [TestFixture(Platform.iOS, UserType.Demo)]
    class TrendsTests : BaseTest
    {
        public TrendsTests(Platform platform, UserType userType) : base(platform, userType)
        {
        }

        public override async Task BeforeEachTest()
        {
            await base.BeforeEachTest().ConfigureAwait(false);

            var selectedRepository = RepositoryPage.GetVisibleRepositoryList().First();

            RepositoryPage.TapRepository(selectedRepository.Name);

            await TrendsPage.WaitForPageToLoad().ConfigureAwait(false);

            Assert.IsTrue(App.Query(selectedRepository.Name).Any());
        }

        [Test]
        public void EnsureLegendIsInteractive()
        {
            //Arrange
            bool isViewsSeriesVisible_Initial = TrendsPage.IsSeriesVisible(TrendsChartConstants.TotalViewsTitle);
            bool isUniqueViewsSeriesVisible_Initial = TrendsPage.IsSeriesVisible(TrendsChartConstants.UniqueViewsTitle);
            bool isClonesSeriesVisible_Initial = TrendsPage.IsSeriesVisible(TrendsChartConstants.TotalClonesTitle);
            bool isUniqueClonesSeriesVisible_Initial = TrendsPage.IsSeriesVisible(TrendsChartConstants.UniqueClonesTitle);

            //Act
            TrendsPage.TapViewsLegendIcon();
            TrendsPage.TapUniqueViewsLegendIcon();
            TrendsPage.TapClonesLegendIcon();
            TrendsPage.TapUniqueClonesLegendIcon();

            //Assert
            Assert.AreNotEqual(isViewsSeriesVisible_Initial, TrendsPage.IsSeriesVisible(TrendsChartConstants.TotalViewsTitle));
            Assert.AreNotEqual(isUniqueViewsSeriesVisible_Initial, TrendsPage.IsSeriesVisible(TrendsChartConstants.UniqueViewsTitle));
            Assert.AreNotEqual(isClonesSeriesVisible_Initial, TrendsPage.IsSeriesVisible(TrendsChartConstants.TotalClonesTitle));
            Assert.AreNotEqual(isUniqueClonesSeriesVisible_Initial, TrendsPage.IsSeriesVisible(TrendsChartConstants.UniqueClonesTitle));
        }
    }
}
