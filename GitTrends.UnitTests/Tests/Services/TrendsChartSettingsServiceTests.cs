using GitTrends.Mobile.Common;

namespace GitTrends.UnitTests;

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
		Assert.Multiple(() =>
		{
			Assert.That(trendsChartOption_Initial, Is.EqualTo(TrendsChartOption.All));
			Assert.That(shouldShowClonesByDefault_Initial);
			Assert.That(shouldShowUniqueClonesByDefault_Initial);
			Assert.That(shouldShowViewsByDefault_Initial);
			Assert.That(shouldShowUniqueViewsByDefault_Initial);

			Assert.That(trendsChartOption_AfterNoUniques, Is.EqualTo(TrendsChartOption.NoUniques));
			Assert.That(shouldShowClonesByDefault_AfterNoUniques);
			Assert.That(shouldShowUniqueClonesByDefault_AfterNoUniques, Is.False);
			Assert.That(shouldShowViewsByDefault_AfterNoUniques);
			Assert.That(shouldShowUniqueViewsByDefault_AfterNoUniques, Is.False);

			Assert.That(trendsChartOption_AfterJustUniques, Is.EqualTo(TrendsChartOption.JustUniques));
			Assert.That(shouldShowClonesByDefault_AfterJustUniques, Is.False);
			Assert.That(shouldShowUniqueClonesByDefault_AfterJustUniques);
			Assert.That(shouldShowViewsByDefault_AfterJustUniques, Is.False);
			Assert.That(shouldShowUniqueViewsByDefault_AfterJustUniques);

			Assert.That(trendsChartOption_AfterAll, Is.EqualTo(TrendsChartOption.All));
			Assert.That(shouldShowClonesByDefault_AfterAll);
			Assert.That(shouldShowUniqueClonesByDefault_AfterAll);
			Assert.That(shouldShowViewsByDefault_AfterAll);
			Assert.That(shouldShowUniqueViewsByDefault_AfterAll);
		});
	}
}