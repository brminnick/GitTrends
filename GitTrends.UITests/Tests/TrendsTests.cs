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
        Repository? _selectedRepository;

        public TrendsTests(Platform platform, UserType userType) : base(platform, userType)
        {
        }

        public override async Task BeforeEachTest()
        {
            await base.BeforeEachTest().ConfigureAwait(false);

            _selectedRepository = RepositoryPage.VisibleCollection.First();

            RepositoryPage.TapRepository(_selectedRepository.Name);

            await TrendsPage.WaitForPageToLoad().ConfigureAwait(false);

            Assert.IsTrue(App.Query(_selectedRepository.Name).Any());

            Assert.AreEqual(_selectedRepository.TotalViews.ToAbbreviatedText(), TrendsPage.ViewsStatisticsLabelText);
            Assert.AreEqual(_selectedRepository.TotalUniqueViews.ToAbbreviatedText(), TrendsPage.UniqueViewsStatisticsLabelText);
            Assert.AreEqual(_selectedRepository.TotalClones.ToAbbreviatedText(), TrendsPage.ClonesStatisticsLabelText);
            Assert.AreEqual(_selectedRepository.TotalUniqueClones.ToAbbreviatedText(), TrendsPage.UniqueClonesStatisticsLabelText);
        }

        [Test]
        public void EnsureViewsClonesCardsAreInteractive()
        {
            //Arrange
            bool isViewsSeriesVisible_Initial = TrendsPage.IsViewsClonesChartSeriesVisible(TrendsChartTitleConstants.TotalViewsTitle);
            bool isUniqueViewsSeriesVisible_Initial = TrendsPage.IsViewsClonesChartSeriesVisible(TrendsChartTitleConstants.UniqueViewsTitle);
            bool isClonesSeriesVisible_Initial = TrendsPage.IsViewsClonesChartSeriesVisible(TrendsChartTitleConstants.TotalClonesTitle);
            bool isUniqueClonesSeriesVisible_Initial = TrendsPage.IsViewsClonesChartSeriesVisible(TrendsChartTitleConstants.UniqueClonesTitle);

            //Act
            TrendsPage.TapViewsCard();

            //Assert
            Assert.AreNotEqual(isViewsSeriesVisible_Initial, TrendsPage.IsViewsClonesChartSeriesVisible(TrendsChartTitleConstants.TotalViewsTitle));

            //Act
            TrendsPage.TapUniqueViewsCard();

            //Assert
            Assert.AreNotEqual(isUniqueViewsSeriesVisible_Initial, TrendsPage.IsViewsClonesChartSeriesVisible(TrendsChartTitleConstants.UniqueViewsTitle));

            //Act
            TrendsPage.TapClonesCard();

            //Assert
            Assert.AreNotEqual(isClonesSeriesVisible_Initial, TrendsPage.IsViewsClonesChartSeriesVisible(TrendsChartTitleConstants.TotalClonesTitle));

            //Act
            TrendsPage.TapUniqueClonesCard();

            //Assert
            Assert.AreNotEqual(isUniqueClonesSeriesVisible_Initial, TrendsPage.IsViewsClonesChartSeriesVisible(TrendsChartTitleConstants.UniqueClonesTitle));
        }

        [Test]
        public async Task ViewStarsChart()
        {
            //Arrange
            Assert.IsTrue(TrendsPage.IsViewsClonesChartVisible);

            //Act
            await TrendsPage.MoveToNextPage();
            await TrendsPage.WaitForPageToLoad();

            //Assert
            Assert.IsTrue(TrendsPage.IsStarsChartVisible);
            Assert.AreEqual(_selectedRepository?.StarCount.ToAbbreviatedText(), TrendsPage.StarsStatisticsLabelText);
        }
    }
}
