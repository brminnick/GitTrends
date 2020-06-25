using System;
using System.Linq;
using System.Threading.Tasks;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
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
        string _repositoryPageTitle_BeforeEachTest = string.Empty;

        public SettingsTests(Platform platform, UserType userType) : base(platform, userType)
        {
        }

        public override async Task BeforeEachTest()
        {
            await base.BeforeEachTest().ConfigureAwait(false);

            _repositoryPageTitle_BeforeEachTest = RepositoryPage.PageTitle;

            RepositoryPage.TapSettingsButton();
            await SettingsPage.WaitForPageToLoad().ConfigureAwait(false);
        }

        [Test]
        public void EnsureCreatedByLabelOpensBrowser()
        {
            //Arrange

            //Act
            SettingsPage.TapCopyrightLabel();

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

            await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
            SettingsPage.WaitForNoOperatingSystemNotificationDiaglog();

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
        public async Task VerifyLanguagePicker()
        {
            //Arrange
            string? preferredLanguage_Initial, preferredLanguage_Final;
            string themeTitleLabelText_Initial, themeTitleLabelText_Final, languageTitleLabelText_Initial, languageTitleLabelText_Final,
                settingsPageTitle_Initial, settingsPageTitle_Final, copyrightLabelTitleLabelText_Initial, copyrightLabelTitleLabelText_Final,
                gitHubNameText_Initial, gitHubNameText_Final, gitHubAliasText_Initial, gitHubAliasText_Final, tryDemoButtonText_Initial, tryDemoButtonText_Final,
                loginTitleLabelText_Initial, loginTitleLabelText_Final, registerForNotificationsTitleLabelText_Initial, registerForNotificationsTitleLabelText_Final,
                preferredChartsTitleTitleLabelText_Initial, preferredChartsTitleTitleLabelText_Final, preferredChartsAllTitleLabelText_Initial, preferredChartsAllTitleLabelText_Final,
                preferredChartsNoUniquesTitleLabelText_Initial, preferredChartsNoUniquesTitleLabelText_Final, preferredChartsOnlyUniquesTitleLabelText_Initial, preferredChartsOnlyUniquesTitleLabelText_Final;

            var previousLanguageRepositoryPageTitle = _repositoryPageTitle_BeforeEachTest;

            //Act
            foreach (var preferredLanguageKeyValuePair in CultureConstants.CulturePickerOptions.Reverse())
            {
                settingsPageTitle_Initial = SettingsPage.PageTitle;
                gitHubNameText_Initial = SettingsPage.GitHubNameLabelText;
                gitHubAliasText_Initial = SettingsPage.GitHubAliasLabelText;
                loginTitleLabelText_Initial = SettingsPage.LoginTitleText;
                preferredLanguage_Initial = SettingsPage.PreferredLanguage;
                themeTitleLabelText_Initial = SettingsPage.ThemeTitleLabelText;
                languageTitleLabelText_Initial = SettingsPage.LangageTitleLabelText;
                copyrightLabelTitleLabelText_Initial = SettingsPage.CopyrightLabelText;
                preferredChartsTitleTitleLabelText_Initial = SettingsPage.PreferredChartLabelText;
                registerForNotificationsTitleLabelText_Initial = SettingsPage.RegisterForNotificationsTitleLabelText;

                preferredChartsAllTitleLabelText_Initial = TrendsChartConstants.TrendsChartTitles[TrendsChartOption.All];
                preferredChartsNoUniquesTitleLabelText_Initial = TrendsChartConstants.TrendsChartTitles[TrendsChartOption.NoUniques];
                preferredChartsOnlyUniquesTitleLabelText_Initial = TrendsChartConstants.TrendsChartTitles[TrendsChartOption.JustUniques];

                SettingsPage.TapLoginButton();
                tryDemoButtonText_Initial = SettingsPage.TryDemoButtonText;
                SettingsPage.TapTryDemoButton();

                await SettingsPage.SelectLanguage(preferredLanguageKeyValuePair.Value).ConfigureAwait(false);

                settingsPageTitle_Final = SettingsPage.PageTitle;
                gitHubNameText_Final = SettingsPage.GitHubNameLabelText;
                gitHubAliasText_Final = SettingsPage.GitHubAliasLabelText;
                loginTitleLabelText_Final = SettingsPage.LoginTitleText;
                preferredLanguage_Final = SettingsPage.PreferredLanguage;
                themeTitleLabelText_Final = SettingsPage.ThemeTitleLabelText;
                languageTitleLabelText_Final = SettingsPage.LangageTitleLabelText;
                copyrightLabelTitleLabelText_Final = SettingsPage.CopyrightLabelText;
                preferredChartsTitleTitleLabelText_Final = SettingsPage.PreferredChartLabelText;
                registerForNotificationsTitleLabelText_Final = SettingsPage.RegisterForNotificationsTitleLabelText;

                preferredChartsAllTitleLabelText_Final = TrendsChartConstants.TrendsChartTitles[TrendsChartOption.All];
                preferredChartsNoUniquesTitleLabelText_Final = TrendsChartConstants.TrendsChartTitles[TrendsChartOption.NoUniques];
                preferredChartsOnlyUniquesTitleLabelText_Final = TrendsChartConstants.TrendsChartTitles[TrendsChartOption.JustUniques];

                SettingsPage.TapLoginButton();
                tryDemoButtonText_Final = SettingsPage.TryDemoButtonText;
                SettingsPage.TapTryDemoButton();

                SettingsPage.TapBackButton();

                //Assert
                Assert.AreNotEqual(RepositoryPage.PageTitle, previousLanguageRepositoryPageTitle);

                Assert.AreEqual(preferredLanguage_Final, string.IsNullOrWhiteSpace(preferredLanguageKeyValuePair.Key) ? null : preferredLanguageKeyValuePair.Key);

                if (UserType is UserType.Demo)
                {
                    Assert.AreNotEqual(gitHubNameText_Final, gitHubNameText_Initial);
                }
                else
                {
                    Assert.AreEqual(gitHubAliasText_Final, gitHubAliasText_Initial);
                    Assert.AreEqual(gitHubNameText_Final, gitHubNameText_Initial);
                }

                Assert.AreNotEqual(settingsPageTitle_Final, settingsPageTitle_Initial);
                Assert.AreNotEqual(preferredLanguage_Final, preferredLanguage_Initial);
                Assert.AreNotEqual(tryDemoButtonText_Final, tryDemoButtonText_Initial);
                Assert.AreNotEqual(loginTitleLabelText_Final, loginTitleLabelText_Initial);
                Assert.AreNotEqual(themeTitleLabelText_Final, themeTitleLabelText_Initial);
                Assert.AreNotEqual(languageTitleLabelText_Final, languageTitleLabelText_Initial);
                Assert.AreNotEqual(copyrightLabelTitleLabelText_Final, copyrightLabelTitleLabelText_Initial);
                Assert.AreNotEqual(preferredChartsTitleTitleLabelText_Final, preferredChartsTitleTitleLabelText_Initial);
                Assert.AreNotEqual(registerForNotificationsTitleLabelText_Final, registerForNotificationsTitleLabelText_Initial);

                Assert.AreNotEqual(preferredChartsAllTitleLabelText_Final, registerForNotificationsTitleLabelText_Initial);
                Assert.AreNotEqual(preferredChartsNoUniquesTitleLabelText_Final, preferredChartsNoUniquesTitleLabelText_Initial);
                Assert.AreNotEqual(preferredChartsOnlyUniquesTitleLabelText_Final, preferredChartsOnlyUniquesTitleLabelText_Initial);

                //Act
                previousLanguageRepositoryPageTitle = RepositoryPage.PageTitle;

                RepositoryPage.TapSettingsButton();
                await SettingsPage.WaitForPageToLoad().ConfigureAwait(false);
            }
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
            Assert.IsTrue(TrendsPage.IsSeriesVisible(TrendsChartTitleConstants.TotalViewsTitle));
            Assert.IsTrue(TrendsPage.IsSeriesVisible(TrendsChartTitleConstants.TotalClonesTitle));
            Assert.IsFalse(TrendsPage.IsSeriesVisible(TrendsChartTitleConstants.UniqueViewsTitle));
            Assert.IsFalse(TrendsPage.IsSeriesVisible(TrendsChartTitleConstants.UniqueClonesTitle));
        }

        void EnsureGitHubUserViewOpensBrowser_LoggedIn()
        {
            //Arrange
            var aliasLabelText = SettingsPage.GitHubAliasLabelText;
            var nameLabelText = SettingsPage.GitHubNameLabelText;

            //Act
            SettingsPage.TapGitHubUserView();

            //Assert
            Assert.AreEqual("@" + LoggedInUserAlias, aliasLabelText);
            Assert.AreEqual(LoggedInUserName, nameLabelText);

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
            nameLabelText = SettingsPage.GitHubNameLabelText;

            SettingsPage.TapGitHubUserView();

            //Assert
            Assert.AreEqual("@" + DemoUserConstants.Alias, aliasLabelText);
            Assert.AreEqual(DemoUserConstants.Name, nameLabelText);

            if (App is iOSApp)
            {
                SettingsPage.WaitForBrowserToOpen();
                Assert.IsTrue(SettingsPage.IsBrowserOpen);
            }
        }
    }
}
