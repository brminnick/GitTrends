using System;
using System.Linq;
using System.Threading.Tasks;
using GitTrends.Mobile.Shared;
using NUnit.Framework;
using Xamarin.UITest;
using Xamarin.UITest.Android;
using Xamarin.UITest.iOS;

namespace GitTrends.UITests
{
    [TestFixture(Platform.iOS, UserType.Demo)]
    [TestFixture(Platform.iOS, UserType.LoggedIn)]
    [TestFixture(Platform.Android, UserType.Demo)]
    [TestFixture(Platform.Android, UserType.LoggedIn)]
    class SettingsTests : BaseUITest
    {
        public SettingsTests(Platform platform, UserType userType) : base(platform, userType)
        {
        }

        public override async Task BeforeEachTest()
        {
            await base.BeforeEachTest().ConfigureAwait(false);

            RepositoryPage.TapSettingsButton();

            await SettingsPage.WaitForPageToLoad().ConfigureAwait(false);
        }

        [Test]
        public void EnsureCreatedByLabelOpensBrowser()
        {
            //Arrange

            //Act
            SettingsPage.TapCreatedByLabel();

            //Assert
            if (App is iOSApp)
            {
                SettingsPage.WaitForBrowserToOpen();
                Assert.IsTrue(SettingsPage.IsBrowserOpen);
            }
        }

        [Test]
        public void EnsureGitHubUserViewOpensBrowser()
        {
            if (UserType is UserType.LoggedIn)
                EnsureGitHubUserViewOpensBrowser_LoggedIn();
            else if (UserType is UserType.Demo)
                EnsureGitHubUserViewOpensBrowser_Demo();
            else
                throw new NotImplementedException();
        }

        [Test]
        public async Task VerifyNotificationsSwitch()
        {
            //Arrange
            bool areNotificationsEnabled_Initial = SettingsPage.AreNotificationsEnabled;
            bool shouldSendNotifications_Initial = SettingsPage.ShouldSendNotifications;
            bool shouldSendNotifications_Final, areNotificationsEnabled_Final;

            //Act
            SettingsPage.ToggleRegisterForNotificationsSwitch();

            await Task.Delay(TimeSpan.FromSeconds(5)).ConfigureAwait(false);

            shouldSendNotifications_Final = SettingsPage.ShouldSendNotifications;
            areNotificationsEnabled_Final = SettingsPage.AreNotificationsEnabled;

            //Assert
            Assert.IsFalse(shouldSendNotifications_Initial);

            if (App is AndroidApp)
                Assert.IsTrue(areNotificationsEnabled_Initial);
            else
                Assert.IsFalse(areNotificationsEnabled_Initial);

            Assert.IsTrue(shouldSendNotifications_Final);
            Assert.IsTrue(areNotificationsEnabled_Final);
        }

        [Test]
        public async Task VerifyThemePicker()
        {
            //Arrange
            PreferredTheme preferredTheme_Initial = SettingsPage.PreferredTheme;
            PreferredTheme preferredTheme_Light, preferredTheme_Dark;

            //Act
            await SettingsPage.SelectTheme(PreferredTheme.Light).ConfigureAwait(false);
            preferredTheme_Light = SettingsPage.PreferredTheme;

            await SettingsPage.SelectTheme(PreferredTheme.Dark).ConfigureAwait(false);
            preferredTheme_Dark = SettingsPage.PreferredTheme;

            //Assert
            Assert.AreEqual(PreferredTheme.Default, preferredTheme_Initial);
            Assert.AreEqual(PreferredTheme.Light, preferredTheme_Light);
            Assert.AreEqual(PreferredTheme.Dark, preferredTheme_Dark);
        }

        [Test]
        public async Task VerifyChartSettingsOptions()
        {
            //Arrange

            //Assert
            Assert.AreEqual(TrendsChartOption.All, SettingsPage.CurrentTrendsChartOption);

            //Act
            await SettingsPage.SetTrendsChartOption(TrendsChartOption.JustUniques).ConfigureAwait(false);

            //Assert
            Assert.AreEqual(TrendsChartOption.JustUniques, SettingsPage.CurrentTrendsChartOption);

            //Act
            await SettingsPage.SetTrendsChartOption(TrendsChartOption.All).ConfigureAwait(false);

            //Assert
            Assert.AreEqual(TrendsChartOption.All, SettingsPage.CurrentTrendsChartOption);

            //Act
            await SettingsPage.SetTrendsChartOption(TrendsChartOption.NoUniques);

            //Assert
            Assert.AreEqual(TrendsChartOption.NoUniques, SettingsPage.CurrentTrendsChartOption);

            //Act
            SettingsPage.TapBackButton();

            await RepositoryPage.WaitForPageToLoad().ConfigureAwait(false);
            await RepositoryPage.WaitForNoPullToRefreshIndicator().ConfigureAwait(false);

            var visibleRepositories = RepositoryPage.VisibleCollection;
            RepositoryPage.TapRepository(visibleRepositories.First().Name);

            await TrendsPage.WaitForPageToLoad().ConfigureAwait(false);

            //Assert
            Assert.IsTrue(TrendsPage.IsSeriesVisible(TrendsChartConstants.TotalViewsTitle));
            Assert.IsTrue(TrendsPage.IsSeriesVisible(TrendsChartConstants.TotalClonesTitle));
            Assert.IsFalse(TrendsPage.IsSeriesVisible(TrendsChartConstants.UniqueViewsTitle));
            Assert.IsFalse(TrendsPage.IsSeriesVisible(TrendsChartConstants.UniqueClonesTitle));
        }

        void EnsureGitHubUserViewOpensBrowser_LoggedIn()
        {
            //Arrange
            var aliasLabelText = SettingsPage.GitHubAliasLabelText;
            var nameLabelText = SettingsPage.GitHubAliasNameText;

            //Act
            SettingsPage.TapGitHubUserView();

            //Assert
            Assert.AreEqual("@" + LoggedInUserConstants.Alias, aliasLabelText);
            Assert.AreEqual(LoggedInUserConstants.Name, nameLabelText);

            if (App is iOSApp)
            {
                SettingsPage.WaitForBrowserToOpen();
                Assert.IsTrue(SettingsPage.IsBrowserOpen);
            }
        }

        void EnsureGitHubUserViewOpensBrowser_Demo()
        {
            //Arrange
            string nameLabelText, aliasLabelText;

            //Act
            SettingsPage.WaitForGitHubLoginToComplete();

            aliasLabelText = SettingsPage.GitHubAliasLabelText;
            nameLabelText = SettingsPage.GitHubAliasNameText;

            SettingsPage.TapGitHubUserView();

            //Assert
            Assert.AreEqual("@" + DemoDataConstants.Alias, aliasLabelText);
            Assert.AreEqual(DemoDataConstants.Name, nameLabelText);

            if (App is iOSApp)
            {
                SettingsPage.WaitForBrowserToOpen();
                Assert.IsTrue(SettingsPage.IsBrowserOpen);
            }
        }
    }
}
