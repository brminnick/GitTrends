using GitTrends.Mobile.Common;

namespace GitTrends.UnitTests;

class ThemeServiceTests : BaseTest
{
	[Test]
	public void PreferenceTest()
	{
		//Arrange
		PreferredTheme preferredTheme_Initial, preferredTheme_Final;
		const PreferredTheme expectedFinalPreferredTheme = PreferredTheme.Dark;

		var themeService = ServiceCollection.ServiceProvider.GetRequiredService<ThemeService>();

		//Act
		preferredTheme_Initial = themeService.Preference;

		themeService.Preference = expectedFinalPreferredTheme;

		preferredTheme_Final = themeService.Preference;

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(preferredTheme_Initial, Is.EqualTo(PreferredTheme.Default));
			Assert.That(preferredTheme_Final, Is.EqualTo(expectedFinalPreferredTheme));
		});
	}
}