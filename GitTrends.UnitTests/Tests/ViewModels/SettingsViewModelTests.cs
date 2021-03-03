using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using GitTrends.Shared;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shiny;
using Xamarin.Essentials.Interfaces;

namespace GitTrends.UnitTests
{
    class SettingsViewModelTests : BaseTest
    {
        [Test]
        public async Task RegisterForNotificationsTest()
        {
            //Arrange
            bool shouldSendNotifications_Initial, shouldSendNotifications_Final;
            bool isRegisterForNotificationsSwitchEnabled_Initial, isRegisterForNotificationsSwitchEnabled_Final;
            bool isRegisterForNotificationsSwitchToggled_Initial, isRegisterForNotificationsSwitchToggled_Final;

            bool didSetNotificationsPreferenceCompletedFire = false;
            var setNotificationsPreferenceCompletedTCS = new TaskCompletionSource<AccessState?>();

            var settingsViewModel = ServiceCollection.ServiceProvider.GetRequiredService<SettingsViewModel>();
            SettingsViewModel.SetNotificationsPreferenceCompleted += HandleSetNotificationsPreferenceCompleted;

            var notificationService = ServiceCollection.ServiceProvider.GetRequiredService<NotificationService>();

            //Act
            shouldSendNotifications_Initial = notificationService.ShouldSendNotifications;

            isRegisterForNotificationsSwitchEnabled_Initial = settingsViewModel.IsRegisterForNotificationsSwitchEnabled;
            isRegisterForNotificationsSwitchToggled_Initial = settingsViewModel.IsRegisterForNotificationsSwitchToggled;

            settingsViewModel.IsRegisterForNotificationsSwitchToggled = true;
            var accessState = await setNotificationsPreferenceCompletedTCS.Task.ConfigureAwait(false);

            shouldSendNotifications_Final = notificationService.ShouldSendNotifications;

            isRegisterForNotificationsSwitchEnabled_Final = settingsViewModel.IsRegisterForNotificationsSwitchEnabled;
            isRegisterForNotificationsSwitchToggled_Final = settingsViewModel.IsRegisterForNotificationsSwitchToggled;

            //Assert
            Assert.IsTrue(didSetNotificationsPreferenceCompletedFire);

            Assert.IsFalse(shouldSendNotifications_Initial);
            Assert.IsTrue(isRegisterForNotificationsSwitchEnabled_Initial);
            Assert.IsFalse(isRegisterForNotificationsSwitchToggled_Initial);

            Assert.IsTrue(shouldSendNotifications_Final);
            Assert.IsTrue(isRegisterForNotificationsSwitchEnabled_Final);
            Assert.IsTrue(isRegisterForNotificationsSwitchToggled_Final);

            Assert.AreEqual(AccessState.Available, accessState);

            void HandleSetNotificationsPreferenceCompleted(object? sender, AccessState? e)
            {
                SettingsViewModel.SetNotificationsPreferenceCompleted -= HandleSetNotificationsPreferenceCompleted;

                didSetNotificationsPreferenceCompletedFire = true;
                setNotificationsPreferenceCompletedTCS.SetResult(e);
            }
        }

        [Test]
        public void PreferredChartsTest()
        {
            //Arrange
            int preferredChartsIndex_Initial, preferredChartsIndex_AfterJustUniques, preferredChartsIndex_AfterNoUniques, preferredChartsIndex_AfterAll;
            TrendsChartOption currentTrendsChartOption_Initial, currentTrendsChartOption_AfterJustUniques, currentTrendsChartOption_AfterNoUniques, currentTrendsChartOption_AfterAll;

            var settingsViewModel = ServiceCollection.ServiceProvider.GetRequiredService<SettingsViewModel>();
            var trendsChartSettingsService = ServiceCollection.ServiceProvider.GetRequiredService<TrendsChartSettingsService>();

            //Act
            currentTrendsChartOption_Initial = trendsChartSettingsService.CurrentTrendsChartOption;
            preferredChartsIndex_Initial = settingsViewModel.PreferredChartsSelectedIndex;

            settingsViewModel.PreferredChartsSelectedIndex = (int)TrendsChartOption.JustUniques;

            currentTrendsChartOption_AfterJustUniques = trendsChartSettingsService.CurrentTrendsChartOption;
            preferredChartsIndex_AfterJustUniques = settingsViewModel.PreferredChartsSelectedIndex;

            settingsViewModel.PreferredChartsSelectedIndex = (int)TrendsChartOption.NoUniques;

            currentTrendsChartOption_AfterNoUniques = trendsChartSettingsService.CurrentTrendsChartOption;
            preferredChartsIndex_AfterNoUniques = settingsViewModel.PreferredChartsSelectedIndex;

            settingsViewModel.PreferredChartsSelectedIndex = (int)TrendsChartOption.All;

            currentTrendsChartOption_AfterAll = trendsChartSettingsService.CurrentTrendsChartOption;
            preferredChartsIndex_AfterAll = settingsViewModel.PreferredChartsSelectedIndex;

            //Assert
            Assert.AreEqual(TrendsChartOption.All, currentTrendsChartOption_Initial);
            Assert.AreEqual(TrendsChartOption.All, (TrendsChartOption)preferredChartsIndex_Initial);

            Assert.AreEqual(TrendsChartOption.JustUniques, currentTrendsChartOption_AfterJustUniques);
            Assert.AreEqual(TrendsChartOption.JustUniques, (TrendsChartOption)preferredChartsIndex_AfterJustUniques);

            Assert.AreEqual(TrendsChartOption.NoUniques, currentTrendsChartOption_AfterNoUniques);
            Assert.AreEqual(TrendsChartOption.NoUniques, (TrendsChartOption)preferredChartsIndex_AfterNoUniques);

            Assert.AreEqual(TrendsChartOption.All, currentTrendsChartOption_AfterAll);
            Assert.AreEqual(TrendsChartOption.All, (TrendsChartOption)preferredChartsIndex_AfterAll);
        }

        [Test]
        public async Task DemoButtonCommandTest()
        {
            //Arrange
            bool isDemoButtonVisible_Initial, isDemoButtonVisible_Final;

            string loginLabelText_Initial, loginLabelText_Final;
            string gitHubNameLabelText_Initial, gitHubNameLabelText_Final;
            string gitHubAliasLabelText_Initial, gitHubAliasLabelText_Final;
            string gitHubAvatarImageSource_Initial, gitHubAvatarImageSource_Final;

            var settingsViewModel = ServiceCollection.ServiceProvider.GetRequiredService<SettingsViewModel>();
            var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();

            //Act
            loginLabelText_Initial = settingsViewModel.LoginLabelText;
            isDemoButtonVisible_Initial = settingsViewModel.IsDemoButtonVisible;
            gitHubNameLabelText_Initial = settingsViewModel.GitHubNameLabelText;
            gitHubAliasLabelText_Initial = settingsViewModel.GitHubAliasLabelText;
            gitHubAvatarImageSource_Initial = settingsViewModel.GitHubAvatarImageSource;

            await settingsViewModel.DemoButtonCommand.ExecuteAsync(null).ConfigureAwait(false);

            loginLabelText_Final = settingsViewModel.LoginLabelText;
            isDemoButtonVisible_Final = settingsViewModel.IsDemoButtonVisible;
            gitHubNameLabelText_Final = settingsViewModel.GitHubNameLabelText;
            gitHubAliasLabelText_Final = settingsViewModel.GitHubAliasLabelText;
            gitHubAvatarImageSource_Final = settingsViewModel.GitHubAvatarImageSource;

            //Assert
            Assert.IsTrue(gitHubUserService.IsDemoUser);

            Assert.IsTrue(isDemoButtonVisible_Initial);
            Assert.IsFalse(isDemoButtonVisible_Final);

            Assert.AreEqual(string.Empty, gitHubAliasLabelText_Initial);
            Assert.AreEqual("@" + DemoUserConstants.Alias, gitHubAliasLabelText_Final);
            Assert.AreEqual(gitHubAliasLabelText_Final, "@" + gitHubUserService.Alias);

            Assert.AreEqual(GitHubLoginButtonConstants.NotLoggedIn, gitHubNameLabelText_Initial);
            Assert.AreEqual(DemoUserConstants.Name, gitHubNameLabelText_Final);
            Assert.AreEqual(gitHubNameLabelText_Final, gitHubUserService.Name);

            Assert.AreEqual("DefaultProfileImage", gitHubAvatarImageSource_Initial);
            Assert.AreEqual("GitTrends", gitHubAvatarImageSource_Final);
            Assert.AreEqual(gitHubAvatarImageSource_Final, BaseTheme.GetGitTrendsImageSource());

            Assert.AreEqual(loginLabelText_Initial, GitHubLoginButtonConstants.ConnectToGitHub);
            Assert.AreEqual(loginLabelText_Final, GitHubLoginButtonConstants.Disconnect);
        }

        [Test]
        public async Task ConnectToGitHubButtonCommandTest()
        {
            //Arrange
            string openedUrl;
            bool isAuthenticating_BeforeCommand, isAuthenticating_DuringCommand, isAuthenticating_AfterCommand;
            bool isNotAuthenticating_BeforeCommand, isNotAuthenticating_DuringCommand, isNotAuthenticating_AfterCommand;
            bool isDemoButtonVisible_BeforeCommand, isDemoButtonVisible_DuringCommand, isDemoButtonVisible_AfterCommand;

            bool didOpenAsyncFire = false;
            var openAsyncExecutedTCS = new TaskCompletionSource<Uri>();

            MockBrowser.OpenAsyncExecuted += HandleOpenAsyncExecuted;

            var settingsViewModel = ServiceCollection.ServiceProvider.GetRequiredService<SettingsViewModel>();

            //Act
            isAuthenticating_BeforeCommand = settingsViewModel.IsAuthenticating;
            isNotAuthenticating_BeforeCommand = settingsViewModel.IsNotAuthenticating;
            isDemoButtonVisible_BeforeCommand = settingsViewModel.IsDemoButtonVisible;

            var connectToGitHubButtonCommandTask = settingsViewModel.ConnectToGitHubButtonCommand.ExecuteAsync((CancellationToken.None, null));
            isAuthenticating_DuringCommand = settingsViewModel.IsAuthenticating;
            isNotAuthenticating_DuringCommand = settingsViewModel.IsNotAuthenticating;
            isDemoButtonVisible_DuringCommand = settingsViewModel.IsDemoButtonVisible;

            await connectToGitHubButtonCommandTask.ConfigureAwait(false);
            var openedUri = await openAsyncExecutedTCS.Task.ConfigureAwait(false);
            openedUrl = openedUri.AbsoluteUri;

            isAuthenticating_AfterCommand = settingsViewModel.IsAuthenticating;
            isNotAuthenticating_AfterCommand = settingsViewModel.IsNotAuthenticating;
            isDemoButtonVisible_AfterCommand = settingsViewModel.IsDemoButtonVisible;

            //Assert
            Assert.IsTrue(didOpenAsyncFire);

            Assert.IsFalse(isAuthenticating_BeforeCommand);
            Assert.True(isNotAuthenticating_BeforeCommand);
            Assert.True(isDemoButtonVisible_BeforeCommand);

            Assert.IsTrue(isAuthenticating_DuringCommand);
            Assert.IsFalse(isNotAuthenticating_DuringCommand);
            Assert.False(isDemoButtonVisible_DuringCommand);

            Assert.IsFalse(isAuthenticating_AfterCommand);
            Assert.True(isNotAuthenticating_AfterCommand);
            Assert.True(isDemoButtonVisible_AfterCommand);

            Assert.IsTrue(openedUrl.Contains($"{GitHubConstants.GitHubBaseUrl}/login/oauth/authorize?client_id="));
            Assert.IsTrue(openedUrl.Contains($"&scope={GitHubConstants.OAuthScope}&state="));

            void HandleOpenAsyncExecuted(object? sender, Uri e)
            {
                MockBrowser.OpenAsyncExecuted -= HandleOpenAsyncExecuted;
                didOpenAsyncFire = true;

                openAsyncExecutedTCS.SetResult(e);
            }
        }

        [Test]
        public void ThemePickerItemsSourceTest()
        {
            //Arrange
            int themePickerIndex_Initial, themePickerIndex_AfterDarkTheme, themePickerIndex_AfterLightTheme, themePickerIndex_AfterDefaultTheme;
            IReadOnlyList<string> themePickerItemSource_Initial, themePickerItemSource_Final;

            var settingsViewModel = ServiceCollection.ServiceProvider.GetRequiredService<SettingsViewModel>();

            //Act
            themePickerItemSource_Initial = settingsViewModel.ThemePickerItemsSource;
            themePickerIndex_Initial = settingsViewModel.ThemePickerSelectedIndex;

            settingsViewModel.ThemePickerSelectedIndex = (int)PreferredTheme.Dark;
            themePickerIndex_AfterDarkTheme = settingsViewModel.ThemePickerSelectedIndex;

            settingsViewModel.ThemePickerSelectedIndex = (int)PreferredTheme.Light;
            themePickerIndex_AfterLightTheme = settingsViewModel.ThemePickerSelectedIndex;

            settingsViewModel.ThemePickerSelectedIndex = (int)PreferredTheme.Default;
            themePickerIndex_AfterDefaultTheme = settingsViewModel.ThemePickerSelectedIndex;
            themePickerItemSource_Final = settingsViewModel.ThemePickerItemsSource;

            //Assert
            Assert.AreEqual(Enum.GetNames(typeof(PreferredTheme)), themePickerItemSource_Initial);
            Assert.AreEqual(themePickerItemSource_Initial, themePickerItemSource_Final);

            Assert.AreEqual(PreferredTheme.Dark, (PreferredTheme)themePickerIndex_AfterDarkTheme);
            Assert.AreEqual(PreferredTheme.Light, (PreferredTheme)themePickerIndex_AfterLightTheme);
            Assert.AreEqual(PreferredTheme.Default, (PreferredTheme)themePickerIndex_AfterDefaultTheme);
        }


        [Test]
        public async Task CopyrightLabelTappedCommandTest()
        {
            //Arrange
            bool didOpenAsyncFire = false;
            var openAsyncTCS = new TaskCompletionSource<object?>();

            var settingsViewModel = ServiceCollection.ServiceProvider.GetRequiredService<SettingsViewModel>();

            MockLauncher.OpenAsyncExecuted += HandleOpenAsyncExecuted;

            //Act
            settingsViewModel.CopyrightLabelTappedCommand.Execute(null);
            await openAsyncTCS.Task.ConfigureAwait(false);

            //Assert
            Assert.IsTrue(didOpenAsyncFire);

            void HandleOpenAsyncExecuted(object? sender, EventArgs e)
            {
                MockLauncher.OpenAsyncExecuted -= HandleOpenAsyncExecuted;

                didOpenAsyncFire = true;
                openAsyncTCS.SetResult(null);
            }
        }
    }
}
