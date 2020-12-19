using System.Threading.Tasks;
using GitTrends.Mobile.Common.Constants;
using NUnit.Framework;
using Xamarin.UITest;

namespace GitTrends.UITests
{
    [TestFixture(Platform.iOS, UserType.Demo)]
    [TestFixture(Platform.iOS, UserType.LoggedIn)]
    [TestFixture(Platform.Android, UserType.Demo)]
    [TestFixture(Platform.Android, UserType.LoggedIn)]
    class AboutTests : BaseUITest
    {
        public AboutTests(Platform platform, UserType userType) : base(platform, userType)
        {
        }

        public override async Task BeforeEachTest()
        {
            await base.BeforeEachTest();

            RepositoryPage.TapSettingsButton();
            await SettingsPage.WaitForPageToLoad().ConfigureAwait(false);

            Assert.AreEqual(AboutPageConstants.About, SettingsPage.AboutLabelText);

            SettingsPage.TapAboutButton();
            await AboutPage.WaitForPageToLoad().ConfigureAwait(false);
        }

        [Test]
        public async Task TapAboutLabelToOpenPage()
        {
            //Arrange

            //Act
            App.Back();
            await SettingsPage.WaitForPageToLoad().ConfigureAwait(false);

            SettingsPage.TapAboutLabel();

            //Assert
            await AboutPage.WaitForPageToLoad().ConfigureAwait(false);
        }
    }
}
