using GitTrends.Mobile.Common;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace GitTrends.UnitTests
{
    class TrendsChartSettingsServiceTests : BaseTest
    {
        [Test]
        public void CurrentTrendsChartOptionTest()
        {
            //Arrange
            TrendsChartOption trendsChartOption_Initial, trendsChartOption_AfterNoUniques, trendsChartOption_AfterJustUniques, trendsChartOption_AfterAll;
            bool shouldShowClonesByDefault_Initial, shouldShowClonesByDefault_AfterNoUniques, shouldShowClonesByDefault_AfterJustUniques, shouldShowClonesByDefault_AfterAll;
            bool shouldShowUniqueClonesByDefault_Initial, shouldShowUniqueClonesByDefault_AfterNoUniques, shouldShowUniqueClonesByDefault_AfterJustUniques, shouldShowUniqueClonesByDefault_AfterAll;
            bool shouldShowViewsByDefault_Initial, shouldShowViewsByDefault_AfterNoUniques, shouldShowViewsByDefault_AfterJustUniques, shouldShowViewsByDefault_AfterAll;
            bool shouldShowUniqueViewsByDefault_Initial, shouldShowUniqueViewsByDefault_AfterNoUniques, shouldShowUniqueViewsByDefault_AfterJustUniques, shouldShowUniqueViewsByDefault_AfterAll;

            var trendsChartSettingsService = ServiceCollection.ServiceProvider.GetRequiredService<TrendsChartSettingsService>();

            //Act
            trendsChartOption_Initial = trendsChartSettingsService.CurrentTrendsChartOption;
            shouldShowClonesByDefault_Initial = trendsChartSettingsService.ShouldShowClonesByDefault;
            shouldShowUniqueClonesByDefault_Initial = trendsChartSettingsService.ShouldShowUniqueClonesByDefault;
            shouldShowViewsByDefault_Initial = trendsChartSettingsService.ShouldShowViewsByDefault;
            shouldShowUniqueViewsByDefault_Initial = trendsChartSettingsService.ShouldShowUniqueViewsByDefault;

            trendsChartSettingsService.CurrentTrendsChartOption = TrendsChartOption.NoUniques;

            trendsChartOption_AfterNoUniques = trendsChartSettingsService.CurrentTrendsChartOption;
            shouldShowClonesByDefault_AfterNoUniques = trendsChartSettingsService.ShouldShowClonesByDefault;
            shouldShowUniqueClonesByDefault_AfterNoUniques = trendsChartSettingsService.ShouldShowUniqueClonesByDefault;
            shouldShowViewsByDefault_AfterNoUniques = trendsChartSettingsService.ShouldShowViewsByDefault;
            shouldShowUniqueViewsByDefault_AfterNoUniques = trendsChartSettingsService.ShouldShowUniqueViewsByDefault;

            trendsChartSettingsService.CurrentTrendsChartOption = TrendsChartOption.JustUniques;

            trendsChartOption_AfterJustUniques = trendsChartSettingsService.CurrentTrendsChartOption;
            shouldShowClonesByDefault_AfterJustUniques = trendsChartSettingsService.ShouldShowClonesByDefault;
            shouldShowUniqueClonesByDefault_AfterJustUniques = trendsChartSettingsService.ShouldShowUniqueClonesByDefault;
            shouldShowViewsByDefault_AfterJustUniques = trendsChartSettingsService.ShouldShowViewsByDefault;
            shouldShowUniqueViewsByDefault_AfterJustUniques = trendsChartSettingsService.ShouldShowUniqueViewsByDefault;

            trendsChartSettingsService.CurrentTrendsChartOption = TrendsChartOption.All;

            trendsChartOption_AfterAll = trendsChartSettingsService.CurrentTrendsChartOption;
            shouldShowClonesByDefault_AfterAll = trendsChartSettingsService.ShouldShowClonesByDefault;
            shouldShowUniqueClonesByDefault_AfterAll = trendsChartSettingsService.ShouldShowUniqueClonesByDefault;
            shouldShowViewsByDefault_AfterAll = trendsChartSettingsService.ShouldShowViewsByDefault;
            shouldShowUniqueViewsByDefault_AfterAll = trendsChartSettingsService.ShouldShowUniqueViewsByDefault;

            //Assert
            Assert.AreEqual(TrendsChartOption.All, trendsChartOption_Initial);
            Assert.IsTrue(shouldShowClonesByDefault_Initial);
            Assert.IsTrue(shouldShowUniqueClonesByDefault_Initial);
            Assert.IsTrue(shouldShowViewsByDefault_Initial);
            Assert.IsTrue(shouldShowUniqueViewsByDefault_Initial);

            Assert.AreEqual(TrendsChartOption.NoUniques, trendsChartOption_AfterNoUniques);
            Assert.IsTrue(shouldShowClonesByDefault_AfterNoUniques);
            Assert.IsFalse(shouldShowUniqueClonesByDefault_AfterNoUniques);
            Assert.IsTrue(shouldShowViewsByDefault_AfterNoUniques);
            Assert.IsFalse(shouldShowUniqueViewsByDefault_AfterNoUniques);

            Assert.AreEqual(TrendsChartOption.JustUniques, trendsChartOption_AfterJustUniques);
            Assert.IsFalse(shouldShowClonesByDefault_AfterJustUniques);
            Assert.IsTrue(shouldShowUniqueClonesByDefault_AfterJustUniques);
            Assert.IsFalse(shouldShowViewsByDefault_AfterJustUniques);
            Assert.IsTrue(shouldShowUniqueViewsByDefault_AfterJustUniques);

            Assert.AreEqual(TrendsChartOption.All, trendsChartOption_AfterAll);
            Assert.IsTrue(shouldShowClonesByDefault_AfterAll);
            Assert.IsTrue(shouldShowUniqueClonesByDefault_AfterAll);
            Assert.IsTrue(shouldShowViewsByDefault_AfterAll);
            Assert.IsTrue(shouldShowUniqueViewsByDefault_AfterAll);
        }
    }
}
