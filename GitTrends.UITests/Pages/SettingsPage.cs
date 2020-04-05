using System;
using System.Linq;
using System.Threading.Tasks;
using GitTrends.Mobile.Shared;
using Xamarin.UITest;
using Xamarin.UITest.iOS;
using Query = System.Func<Xamarin.UITest.Queries.AppQuery, Xamarin.UITest.Queries.AppQuery>;

namespace GitTrends.UITests
{
    class SettingsPage : BasePage
    {
        readonly Query _gitHubAvatarImage, _gitHubAliasLabel, _gitHubLoginButton,
            _gitHubSettingsViewActivityIndicator, _trendsChartSettingsLabel,
            _trendsChartSettingsControl, _demoModeButton, _createdByLabel,
            _registerForNotificationsLabel, _registerForNotificationsButton;

        public SettingsPage(IApp app) : base(app, PageTitles.SettingsPage)
        {
            _gitHubAvatarImage = GenerateMarkedQuery(SettingsPageAutomationIds.GitHubAvatarImage);
            _gitHubAliasLabel = GenerateMarkedQuery(SettingsPageAutomationIds.GitHubAliasLabel);
            _gitHubLoginButton = GenerateMarkedQuery(SettingsPageAutomationIds.GitHubLoginButton);
            _demoModeButton = GenerateMarkedQuery(SettingsPageAutomationIds.DemoModeButton);
            _gitHubSettingsViewActivityIndicator = GenerateMarkedQuery(SettingsPageAutomationIds.GitHubSettingsViewActivityIndicator);

            _trendsChartSettingsLabel = GenerateMarkedQuery(SettingsPageAutomationIds.TrendsChartSettingsLabel);
            _trendsChartSettingsControl = GenerateMarkedQuery(SettingsPageAutomationIds.TrendsChartSettingsControl);

            _createdByLabel = GenerateMarkedQuery(SettingsPageAutomationIds.CreatedByLabel);

            _registerForNotificationsLabel = GenerateMarkedQuery(SettingsPageAutomationIds.RegisterForNotificationsLabel);
            _registerForNotificationsButton = GenerateMarkedQuery(SettingsPageAutomationIds.RegisterForNotificationsButton);
        }

        public bool IsLoggedIn => App.Query(GitHubLoginButtonConstants.Disconnect).Any();

        public bool IsActivityIndicatorRunning => App.Query(_gitHubSettingsViewActivityIndicator).Any();

        public string GitHubAliasLabelText => GetText(_gitHubAliasLabel);

        public string RegisterForNotificationsLabelText => GetText(_registerForNotificationsLabel);

        public string GitHubButtonText => GetText(_gitHubLoginButton);

        public string TrendsChartLabelText => GetText(_trendsChartSettingsLabel);

        public TrendsChartOption CurrentTrendsChartOption => App.InvokeBackdoorMethod<TrendsChartOption>(BackdoorMethodConstants.GetCurrentTrendsChartOption);

        public bool IsBrowserOpen => App switch
        {
            iOSApp iOSApp => iOSApp.Query(x => x.Class("SFSafariView")).Any(),
            _ => throw new NotSupportedException("Browser Can Only Be Verified on iOS")
        };

        public async Task SetTrendsChartOption(TrendsChartOption trendsChartOption)
        {
            const int margin = 10;

            var trendsChartQuery = App.Query(_trendsChartSettingsControl).First();

            switch (trendsChartOption)
            {
                case TrendsChartOption.All:
                    App.TapCoordinates(trendsChartQuery.Rect.X + margin, trendsChartQuery.Rect.CenterY);
                    break;

                case TrendsChartOption.NoUniques:
                    App.TapCoordinates(trendsChartQuery.Rect.CenterX, trendsChartQuery.Rect.CenterY);
                    break;

                case TrendsChartOption.JustUniques:
                    App.TapCoordinates(trendsChartQuery.Rect.X + trendsChartQuery.Rect.Width - margin, trendsChartQuery.Rect.CenterY);
                    break;

                default:
                    throw new NotSupportedException();
            }

            await waitForSettingsToUpdate().ConfigureAwait(false);

            App.Screenshot($"Trends Chart Option Changed to {trendsChartOption}");

            static Task waitForSettingsToUpdate() => Task.Delay(1000);
        }

        public void TapRegisterForNotificationsButton()
        {
            App.Tap(_registerForNotificationsButton);
            App.Screenshot("Register For Notifiations Button Tapped");
        }

        public void WaitForBrowserToOpen()
        {
            if (App is iOSApp iOSApp)
                iOSApp.WaitForElement(x => x.Class("SFSafariView"));
            else
                throw new NotSupportedException("Browser Can Only Be Verified on iOS");

            App.Screenshot("Browser Opened");
        }

        public void TapDemoModeButton()
        {
            App.Tap(_demoModeButton);
            App.Screenshot("Demo Mode Button Tapped");
        }

        public void TapCreatedByLabel()
        {
            App.Tap(_createdByLabel);
            App.Screenshot("Created By Label Tapped");
        }

        public void TapGitHubButton()
        {
            App.Tap(_gitHubLoginButton);
            App.Screenshot("GitHub Button Tapped");
        }

        public void WaitForActivityIndicator()
        {
            App.WaitForElement(_gitHubSettingsViewActivityIndicator);
            App.Screenshot("Activity Indicator Appeared");
        }

        public void WaitForNoActivityIndicator()
        {
            App.WaitForNoElement(_gitHubSettingsViewActivityIndicator);
            App.Screenshot("Activity Indicator Disappeared");
        }

        public void TapBackButton()
        {
            App.Back();
            App.Screenshot("Back Button Tapped");
        }

        public void WaitForGitHubLoginToComplete()
        {
            App.WaitForNoElement(_gitHubSettingsViewActivityIndicator);
            App.WaitForElement(GitHubLoginButtonConstants.Disconnect);

            App.Screenshot("GitHub Login Completed");
        }

        public void WaitForGitHubLogoutToComplete()
        {
            App.WaitForNoElement(_gitHubSettingsViewActivityIndicator);
            App.WaitForElement(GitHubLoginButtonConstants.ConnectWithGitHub);

            App.Screenshot("GitHub Logout Completed");
        }
    }
}