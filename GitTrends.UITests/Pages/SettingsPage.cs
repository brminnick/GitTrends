using System;
using System.Linq;
using System.Threading.Tasks;
using GitTrends.Mobile.Shared;
using Xamarin.UITest;
using Xamarin.UITest.Android;
using Xamarin.UITest.iOS;
using Query = System.Func<Xamarin.UITest.Queries.AppQuery, Xamarin.UITest.Queries.AppQuery>;

namespace GitTrends.UITests
{
    class SettingsPage : BasePage
    {
        readonly Query _gitHubAvatarImage, _gitHubAliasLabel, _gitHubNameLabel, _loginButton,
            _gitHubSettingsViewActivityIndicator, _trendsChartSettingsLabel,
            _trendsChartSettingsControl, _demoModeButton, _createdByLabel,
            _registerForNotiicationsSwitch, _gitHubUserView, _themePicker, _themePickerContainer;

        public SettingsPage(IApp app) : base(app, PageTitles.SettingsPage)
        {
            _gitHubUserView = GenerateMarkedQuery(SettingsPageAutomationIds.GitHubUserView);
            _gitHubNameLabel = GenerateMarkedQuery(SettingsPageAutomationIds.GitHubNameLabel);
            _gitHubAvatarImage = GenerateMarkedQuery(SettingsPageAutomationIds.GitHubAvatarImage);
            _gitHubAliasLabel = GenerateMarkedQuery(SettingsPageAutomationIds.GitHubAliasLabel);
            _loginButton = GenerateMarkedQuery(SettingsPageAutomationIds.GitHubLoginLabel);
            _demoModeButton = GenerateMarkedQuery(SettingsPageAutomationIds.DemoModeButton);
            _gitHubSettingsViewActivityIndicator = GenerateMarkedQuery(SettingsPageAutomationIds.GitHubSettingsViewActivityIndicator);

            _trendsChartSettingsLabel = GenerateMarkedQuery(SettingsPageAutomationIds.TrendsChartSettingsLabel);
            _trendsChartSettingsControl = GenerateMarkedQuery(SettingsPageAutomationIds.TrendsChartSettingsControl);

            _createdByLabel = GenerateMarkedQuery(SettingsPageAutomationIds.CopyrightLabel);

            _registerForNotiicationsSwitch = GenerateMarkedQuery(SettingsPageAutomationIds.RegisterForNotificationsSwitch);

            _themePicker = GenerateMarkedQuery(SettingsPageAutomationIds.ThemePicker);
            _themePickerContainer = GenerateMarkedQuery(SettingsPageAutomationIds.ThemePicker + "_Container");
        }

        public bool IsLoggedIn => App.Query(GitHubLoginButtonConstants.Disconnect).Any();

        public bool IsActivityIndicatorRunning => App.Query(_gitHubSettingsViewActivityIndicator).Any();

        public bool ShouldSendNotifications => App.InvokeBackdoorMethod<bool>(BackdoorMethodConstants.ShouldSendNotifications);

        public string GitHubAliasNameText => GetText(_gitHubNameLabel);

        public string GitHubAliasLabelText => GetText(_gitHubAliasLabel);

        public string GitHubButtonText => GetText(_loginButton);

        public string TrendsChartLabelText => GetText(_trendsChartSettingsLabel);

        public PreferredTheme PreferredTheme => App.InvokeBackdoorMethod<PreferredTheme>(BackdoorMethodConstants.GetPreferredTheme);

        public TrendsChartOption CurrentTrendsChartOption => App.InvokeBackdoorMethod<TrendsChartOption>(BackdoorMethodConstants.GetCurrentTrendsChartOption);

        public override async Task WaitForPageToLoad(TimeSpan? timeout = null)
        {
            await base.WaitForPageToLoad(timeout).ConfigureAwait(false);
            DismissSyncfusionLicensePopup();
        }

        public void WaitForNoOperatingSystemNotificationDiaglog(TimeSpan? timeout = null)
        {
            timeout ??= TimeSpan.FromSeconds(5);

            App.WaitForNoElement("Send You Notifications", timeout: timeout);
            App.Screenshot("Operating System Push Notification Dialog Disappeared");
        }

        public Task SelectTheme(PreferredTheme preferredTheme)
        {
            var rowNumber = (int)preferredTheme;
            var totalRows = Enum.GetNames(typeof(PreferredTheme)).Count();

            var rowOffset = App switch
            {
                iOSApp _ => rowNumber.Equals(totalRows - 1) ? -1 : 1,
                AndroidApp _ => 0,
                _ => throw new NotSupportedException()
            };

            try
            {
                App.Tap(_themePicker);
            }
            catch
            {
                App.Tap(_themePickerContainer);
            }

            scrollToRow(App, rowNumber, rowOffset, totalRows, preferredTheme);

            if (App is iOSApp)
                App.Tap(x => x.Marked("Done"));
            else if (App is AndroidApp)
                App.Tap(x => x.Marked("OK"));
            else
                throw new NotSupportedException();


            App.Screenshot($"Selected Row From Picker: {preferredTheme}");

            return WaitForPageToLoad();

            static void scrollToRow(in IApp app, int rowNumber, int rowOffset, int totalRows, in PreferredTheme preferredTheme)
            {
                switch (app)
                {
                    case iOSApp iosApp:
                        iosApp.WaitForElement(x => x.Class("UIPickerView"));
                        iosApp.Query(x => x.Class("UIPickerView").Invoke("selectRow", rowNumber + rowOffset, "inComponent", 0, "animated", true));

                        iosApp.Tap(preferredTheme.ToString());
                        break;

                    case AndroidApp androidApp:
                        androidApp.WaitForElement(x => x.Class("android.widget.ScrollView"));

                        while (rowNumber + rowOffset != getCurrentPickerRow(androidApp) && totalRows - 1 != getCurrentPickerRow(androidApp))
                        {
                            androidApp.Query(x => x.Class("android.widget.NumberPicker").Invoke("scrollBy", 0, -50));
                        }

                        while (getCurrentPickerRow(androidApp) != rowNumber)
                        {
                            androidApp.Query(x => x.Class("android.widget.NumberPicker").Invoke("scrollBy", 0, 50));
                        }

                        break;

                    default:
                        throw new NotSupportedException($"{app.GetType()} Not Supported");
                }

                app.Screenshot($"Scrolled To Row: {preferredTheme}");

                static int getCurrentPickerRow(AndroidApp app)
                {
                    var currentPickerRow = app.Query(x => x.Class("android.widget.NumberPicker").Invoke("getValue")).First().ToString();
                    return int.Parse(currentPickerRow);
                }
            }
        }

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

            static Task waitForSettingsToUpdate() => Task.Delay(TimeSpan.FromSeconds(1));
        }

        public void TapGitHubUserView()
        {
            App.Tap(_gitHubUserView);
            App.Screenshot("GitHubUserView Tapped");
        }

        public void ToggleRegisterForNotificationsSwitch()
        {
            App.Tap(_registerForNotiicationsSwitch);
            App.Screenshot("Register For Notifiations Button Tapped");
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

        public void TapLoginButton()
        {
            App.Tap(_loginButton);
            App.Screenshot("Login Button Tapped");
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
            App.WaitForElement(GitHubLoginButtonConstants.ConnectToGitHub);

            App.Screenshot("GitHub Logout Completed");
        }
    }
}