using System;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Threading.Tasks;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using Xamarin.UITest;
using Xamarin.UITest.Android;
using Xamarin.UITest.iOS;
using Query = System.Func<Xamarin.UITest.Queries.AppQuery, Xamarin.UITest.Queries.AppQuery>;

namespace GitTrends.UITests
{
    class SettingsPage : BasePage
    {
        readonly Query _gitHubAvatarImage, _gitHubAliasLabel, _gitHubNameLabel,
            _gitHubSettingsViewActivityIndicator, _preferredChartSettingsLabel,
            _preferredChartSettingsControl, _tryDemoButton, _copyrightLabel,
            _registerForNotiicationsSwitch, _gitHubUserView, _themePicker, _themePickerContainer,
            _languagePicker, _languagePickerContainer, _registerForNotificationsTitleLabel, _themeTitleLabel,
            _loginTitleLabel, _languageTitleLabel;

        public SettingsPage(IApp app) : base(app, () => PageTitles.SettingsPage)
        {
            _gitHubUserView = GenerateMarkedQuery(SettingsPageAutomationIds.GitHubUserView);
            _gitHubNameLabel = GenerateMarkedQuery(SettingsPageAutomationIds.GitHubNameLabel);
            _gitHubAvatarImage = GenerateMarkedQuery(SettingsPageAutomationIds.GitHubAvatarImage);
            _gitHubAliasLabel = GenerateMarkedQuery(SettingsPageAutomationIds.GitHubAliasLabel);
            _tryDemoButton = GenerateMarkedQuery(SettingsPageAutomationIds.TryDemoButton);
            _gitHubSettingsViewActivityIndicator = GenerateMarkedQuery(SettingsPageAutomationIds.GitHubSettingsViewActivityIndicator);

            _loginTitleLabel = GenerateMarkedQuery(SettingsPageAutomationIds.LoginTitleLabel);

            _registerForNotificationsTitleLabel = GenerateMarkedQuery(SettingsPageAutomationIds.RegisterForNotificationsTitleLabel);
            _registerForNotiicationsSwitch = GenerateMarkedQuery(SettingsPageAutomationIds.RegisterForNotificationsSwitch);

            _themeTitleLabel = GenerateMarkedQuery(SettingsPageAutomationIds.ThemeTitleLabel);
            _themePicker = GenerateMarkedQuery(SettingsPageAutomationIds.ThemePicker);
            _themePickerContainer = GenerateMarkedQuery(SettingsPageAutomationIds.ThemePicker + "_Container");

            _languageTitleLabel = GenerateMarkedQuery(SettingsPageAutomationIds.LanguageTitleLabel);
            _languagePicker = GenerateMarkedQuery(SettingsPageAutomationIds.LanguagePicker);
            _languagePickerContainer = GenerateMarkedQuery(SettingsPageAutomationIds.LanguagePicker + "_Container");

            _preferredChartSettingsLabel = GenerateMarkedQuery(SettingsPageAutomationIds.PreferredChartSettingsLabel);
            _preferredChartSettingsControl = GenerateMarkedQuery(SettingsPageAutomationIds.PreferredChartSettingsControl);

            _copyrightLabel = GenerateMarkedQuery(SettingsPageAutomationIds.CopyrightLabel);
        }

        public string? PreferredLanguage => App.InvokeBackdoorMethod<string>(BackdoorMethodConstants.GetPreferredLanguage);

        public bool IsLoggedIn => App.Query(GitHubLoginButtonConstants.Disconnect).Any();

        public bool IsActivityIndicatorRunning => App.Query(_gitHubSettingsViewActivityIndicator).Any();

        public bool ShouldSendNotifications => App.InvokeBackdoorMethod<bool>(BackdoorMethodConstants.ShouldSendNotifications);

        public string GitHubNameLabelText => GetText(_gitHubNameLabel);
        public string GitHubAliasLabelText => GetText(_gitHubAliasLabel);

        public string LoginTitleText => GetText(_loginTitleLabel);

        public string TryDemoButtonText => GetText(_tryDemoButton);
        public string CopyrightLabelText => GetText(_copyrightLabel);
        public string ThemeTitleLabelText => GetText(_themeTitleLabel);
        public string LangageTitleLabelText => GetText(_languageTitleLabel);
        public string PreferredChartLabelText => GetText(_preferredChartSettingsLabel);
        public string RegisterForNotificationsTitleLabelText => GetText(_registerForNotificationsTitleLabel);

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

        public async Task SetTrendsChartOption(TrendsChartOption trendsChartOption)
        {
            const int margin = 10;

            var trendsChartQuery = App.Query(_preferredChartSettingsControl).First();

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
            ScrollTo(_gitHubUserView);
            App.Tap(_gitHubUserView);

            App.Screenshot("GitHubUserView Tapped");
        }

        public void ToggleRegisterForNotificationsSwitch()
        {
            ScrollTo(_registerForNotiicationsSwitch);
            App.Tap(_registerForNotiicationsSwitch);

            App.Screenshot("Register For Notifiations Button Tapped");
        }

        public void TapTryDemoButton()
        {
            ScrollTo(_tryDemoButton);
            App.Tap(_tryDemoButton);

            App.Screenshot("Demo Mode Button Tapped");
        }

        public void TapCopyrightLabel()
        {
            ScrollTo(_copyrightLabel);
            App.Tap(_copyrightLabel);

            App.Screenshot("Created By Label Tapped");
        }

        public void TapLoginButton()
        {
            ScrollTo(_loginTitleLabel);
            App.Tap(_loginTitleLabel);

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

        public Task SelectLanguage(string? languagePickerOption)
        {
            var languageIndex = CultureConstants.CulturePickerOptions.Values.ToList().IndexOf(languagePickerOption ?? string.Empty);
            var totalRows = CultureConstants.CulturePickerOptions.Values.Count;

            var culture = CultureConstants.CulturePickerOptions.Where(x => x.Value == languagePickerOption).First().Key;

            CultureInfo.CurrentCulture = getCultureInfo(culture);
            CultureInfo.CurrentUICulture = getCultureInfo(culture);

            CultureInfo.DefaultThreadCurrentCulture = getCultureInfo(culture);
            CultureInfo.DefaultThreadCurrentUICulture = getCultureInfo(culture);

            return SelectFromPicker(languageIndex, totalRows, languagePickerOption ?? string.Empty, _languagePicker, _languagePickerContainer);

            static CultureInfo? getCultureInfo(in string? language) => language switch
            {
                null => null,
                _ => new CultureInfo(language, false)
            };
        }

        public Task SelectTheme(PreferredTheme preferredTheme)
        {
            var index = (int)preferredTheme;
            var totalRows = Enum.GetNames(typeof(PreferredTheme)).Count();

            return SelectFromPicker(index, totalRows, preferredTheme.ToString(), _themePicker, _themePickerContainer);
        }

        Task SelectFromPicker(int rowNumber, int totalRows, string selection, Query picker, Query pickerContainer)
        {
            var rowOffset = App switch
            {
                iOSApp _ => rowNumber.Equals(totalRows - 1) ? -1 : 1,
                AndroidApp _ => 0,
                _ => throw new NotSupportedException()
            };

            if (App is AndroidApp)
                ScrollTo(pickerContainer);
            else
                ScrollTo(picker);

            try
            {
                App.Tap(picker);
            }
            catch
            {
                App.Tap(pickerContainer);
            }

            scrollToRow(App, rowNumber, rowOffset, totalRows, selection);

            if (App is iOSApp)
                App.Tap(x => x.Marked("Done"));
            else if (App is AndroidApp)
                App.Tap(x => x.Marked("OK"));
            else
                throw new NotSupportedException();

            App.Screenshot($"Selected Row From Picker: {selection}");

            return WaitForPageToLoad();

            static void scrollToRow(in IApp app, int rowNumber, int rowOffset, int totalRows, in string selection)
            {
                switch (app)
                {
                    case iOSApp iosApp:
                        iosApp.WaitForElement(x => x.Class("UIPickerView"));
                        iosApp.Query(x => x.Class("UIPickerView").Invoke("selectRow", rowNumber + rowOffset, "inComponent", 0, "animated", true));

                        iosApp.Tap(selection.ToString());
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

                app.Screenshot($"Scrolled To Row: {selection}");

                static int getCurrentPickerRow(AndroidApp app)
                {
                    var currentPickerRow = app.Query(x => x.Class("android.widget.NumberPicker").Invoke("getValue")).First().ToString();
                    return int.Parse(currentPickerRow);
                }
            }
        }
    }
}