using GitTrends.Mobile.Common;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace GitTrends.UnitTests
{
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
            Assert.AreEqual(PreferredTheme.Default, preferredTheme_Initial);
            Assert.AreEqual(expectedFinalPreferredTheme, preferredTheme_Final);
        }
    }
}
