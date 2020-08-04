using System.Linq;
using System.Threading.Tasks;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using NUnit.Framework;
using Xamarin.UITest;

namespace GitTrends.UITests
{
    [TestFixture(Platform.Android, UserType.Demo)]
    [TestFixture(Platform.Android, UserType.LoggedIn)]
    [TestFixture(Platform.iOS, UserType.LoggedIn)]
    [TestFixture(Platform.iOS, UserType.Demo)]
    class TrendsTests : BaseUITest
    {
        public TrendsTests(Platform platform, UserType userType) : base(platform, userType)
        {
        }

        public override async Task BeforeEachTest()
        {
            await base.BeforeEachTest().ConfigureAwait(false);

            var selectedRepository = RepositoryPage.VisibleCollection.First();

            RepositoryPage.TapRepository(selectedRepository.Name);

            await TrendsPage.WaitForPageToLoad().ConfigureAwait(false);

            Assert.IsTrue(App.Query(selectedRepository.Name).Any());

            Assert.AreEqual(selectedRepository.TotalViews.ToAbbreviatedText(), TrendsPage.ViewsStatisticsLabelText);
            Assert.AreEqual(selectedRepository.TotalUniqueViews.ToAbbreviatedText(), TrendsPage.UniqueViewsStatisticsLabelText);
            Assert.AreEqual(selectedRepository.TotalClones.ToAbbreviatedText(), TrendsPage.ClonesStatisticsLabelText);
            Assert.AreEqual(selectedRepository.TotalUniqueClones.ToAbbreviatedText(), TrendsPage.UniqueClonesStatisticsLabelText);
        }

        [Test]
        public void EnsureCardsAreInteractive()
        {
            //Arrange
            bool isViewsSeriesVisible_Initial = TrendsPage.IsSeriesVisible(TrendsChartTitleConstants.TotalViewsTitle);
            bool isUniqueViewsSeriesVisible_Initial = TrendsPage.IsSeriesVisible(TrendsChartTitleConstants.UniqueViewsTitle);
            bool isClonesSeriesVisible_Initial = TrendsPage.IsSeriesVisible(TrendsChartTitleConstants.TotalClonesTitle);
            bool isUniqueClonesSeriesVisible_Initial = TrendsPage.IsSeriesVisible(TrendsChartTitleConstants.UniqueClonesTitle);

            //Act
            TrendsPage.TapViewsCard();

            //Assert
            Assert.AreNotEqual(isViewsSeriesVisible_Initial, TrendsPage.IsSeriesVisible(TrendsChartTitleConstants.TotalViewsTitle));

            //Act
            TrendsPage.TapUniqueViewsCard();

            //Assert
            Assert.AreNotEqual(isUniqueViewsSeriesVisible_Initial, TrendsPage.IsSeriesVisible(TrendsChartTitleConstants.UniqueViewsTitle));

            //Act
            TrendsPage.TapClonesCard();

            //Assert
            Assert.AreNotEqual(isClonesSeriesVisible_Initial, TrendsPage.IsSeriesVisible(TrendsChartTitleConstants.TotalClonesTitle));

            //Act
            TrendsPage.TapUniqueClonesCard();

            //Assert
            Assert.AreNotEqual(isUniqueClonesSeriesVisible_Initial, TrendsPage.IsSeriesVisible(TrendsChartTitleConstants.UniqueClonesTitle));
        }
    }
}
