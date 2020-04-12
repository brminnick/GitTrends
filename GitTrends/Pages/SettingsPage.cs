using GitTrends.Mobile.Shared;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Markup;
using static GitTrends.XamarinFormsService;
using static Xamarin.Forms.Markup.GridRowsColumns;

namespace GitTrends
{
    public class SettingsPage : BaseContentPage<SettingsViewModel>
    {
        public SettingsPage(SettingsViewModel settingsViewModel,
                            NotificationService notificationService,
                            TrendsChartSettingsService trendsChartSettingsService,
                            AnalyticsService analyticsService) : base(settingsViewModel, analyticsService, PageTitles.SettingsPage, true)
        {
            const int separatorHeight = 1;
            const int settingsHeight = 24;

            notificationService.RegisterForNotificationsCompleted += HandleRegisterForNotificationsCompleted;

            Content = new Grid
            {
                RowSpacing = 16.5,

                Margin = new Thickness(30, 0, 30, 5),

                RowDefinitions = Rows.Define(
                    (Row.GitHubUser, AbsoluteGridLength(GitHubUserView.TotalHeight)),
                    (Row.GitHubUserSeparator, AbsoluteGridLength(separatorHeight)),
                    (Row.Logout, AbsoluteGridLength(settingsHeight)),
                    (Row.LogoutSeparator, AbsoluteGridLength(separatorHeight)),
                    (Row.Notifications, AbsoluteGridLength(settingsHeight)),
                    (Row.NotificationsSeparator, AbsoluteGridLength(separatorHeight)),
                    (Row.Theme, AbsoluteGridLength(settingsHeight + 8)),
                    (Row.ThemeSeparator, AbsoluteGridLength(separatorHeight)),
                    (Row.PreferredCharts, AbsoluteGridLength(80)),
                    (Row.Copyright, Star)),

                ColumnDefinitions = Columns.Define(
                    (Column.Icon, AbsoluteGridLength(24)),
                    (Column.Title, StarGridLength(3)),
                    (Column.Button, StarGridLength(1))),

                Children =
                {
                    new GitHubUserView().Row(Row.GitHubUser).ColumnSpan(All<Column>()),

                    new Separator().Row(Row.GitHubUserSeparator).ColumnSpan(All<Column>()),

                    new SvgImage("logout.svg", getSVGIconColor).Row(Row.Logout).Column(Column.Icon),
                    new LogoutLabel().Row(Row.Logout).Column(Column.Title),
                    new SvgImage("right_arrow.svg", getSVGIconColor).End().Row(Row.Logout).Column(Column.Button),
                    new ConnectToGitHubTappableArea().Row(Row.Logout).ColumnSpan(All<Column>()),

                    new Separator().Row(Row.LogoutSeparator).ColumnSpan(All<Column>()),

                    new SvgImage("bell.svg", getSVGIconColor).Row(Row.Notifications).Column(Column.Icon),
                    new RegisterForNotificationsLabel().Row(Row.Notifications).Column(Column.Title),
                    new EnableNotificationsSwitch().Row(Row.Notifications).Column(Column.Button),

                    new Separator().Row(Row.NotificationsSeparator).ColumnSpan(All<Column>()),

                    new SvgImage("theme.svg", getSVGIconColor).Row(Row.Theme).Column(Column.Icon),
                    new ThemeLabel().Row(Row.Theme).Column(Column.Title),
                    new ThemePicker().Row(Row.Theme).Column(Column.Button),

                    new Separator().Row(Row.ThemeSeparator).ColumnSpan(All<Column>()),

                    new PreferredChartsView(trendsChartSettingsService).Row(Row.PreferredCharts).ColumnSpan(All<Column>()),

                    new CopyrightLabel().Row(Row.Copyright).ColumnSpan(All<Column>())
                }
            };

            static Color getSVGIconColor() => (Color)Application.Current.Resources[nameof(BaseTheme.IconColor)];
        }

        enum Row { GitHubUser, GitHubUserSeparator, Logout, LogoutSeparator, Notifications, NotificationsSeparator, Theme, ThemeSeparator, PreferredCharts, Copyright }
        enum Column { Icon, Title, Button }

        void HandleRegisterForNotificationsCompleted(object sender, (bool IsSuccessful, string ErrorMessage) result)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (result.IsSuccessful)
                    await DisplayAlert("Success", "Device Registered for Notificaitons", "OK");
                else
                    await DisplayAlert("Registration Failed", result.ErrorMessage, "OK");
            });
        }

        class ConnectToGitHubTappableArea : View
        {
            public ConnectToGitHubTappableArea()
            {
                Opacity = 0;

                this.BindTapGesture(nameof(SettingsViewModel.ConnectToGitHubButtonCommand));

                var tapGesture = new TapGestureRecognizer
                {
                    Command = new Command(async () =>
                    {
                        await this.FadeTo(0.7, 75);
                        await this.FadeTo(0, 100);
                    })
                };

                GestureRecognizers.Add(tapGesture);

                SetDynamicResource(BackgroundColorProperty, nameof(BaseTheme.PageBackgroundColor));
            }
        }

        class LogoutLabel : SettingsTitleLabel
        {
            public LogoutLabel() => this.SetBinding(TextProperty, nameof(SettingsViewModel.LoginButtonText));
        }

        class RegisterForNotificationsLabel : SettingsTitleLabel
        {
            public RegisterForNotificationsLabel() => Text = "Register for Notifications";
        }

        class ThemeLabel : SettingsTitleLabel
        {
            public ThemeLabel() => Text = "Theme";
        }

        class DarkModeLabel : SettingsTitleLabel
        {
            public DarkModeLabel() => Text = "Dark Mode";
        }

        class Separator : BoxView
        {
            public Separator() => SetDynamicResource(ColorProperty, nameof(BaseTheme.SeparatorColor));
        }

        class EnableNotificationsSwitch : SettingsSwitch
        {
            public EnableNotificationsSwitch()
            {
                this.SetBinding(IsToggledProperty, nameof(SettingsViewModel.IsRegisterForNotificationsSwitchToggled));
                this.SetBinding(IsEnabledProperty, nameof(SettingsViewModel.IsRegisterForNotificationsSwitchEnabled));
            }
        }

        class ThemePicker : Picker
        {
            public ThemePicker()
            {
                FontSize = 14;
                FontFamily = FontFamilyConstants.RobotoRegular;

                WidthRequest = 70;

                HorizontalOptions = LayoutOptions.End;

                SetDynamicResource(BackgroundColorProperty, nameof(BaseTheme.PageBackgroundColor));

                this.SetBinding(ItemsSourceProperty, nameof(SettingsViewModel.ThemePickerItemsSource));
                this.SetBinding(SelectedIndexProperty, nameof(SettingsViewModel.ThemePickerSelectedThemeIndex));
            }
        }
    }
}
