using System;
using GitTrends.Mobile.Shared;
using Xamarin.Forms;
using Xamarin.Forms.Markup;
using static GitTrends.XamarinFormsService;
using static Xamarin.Forms.Markup.GridRowsColumns;

namespace GitTrends
{
    public class SettingsPage : BaseContentPage<SettingsViewModel>
    {
        public SettingsPage(SettingsViewModel settingsViewModel,
                            TrendsChartSettingsService trendsChartSettingsService,
                            AnalyticsService analyticsService) : base(settingsViewModel, analyticsService, PageTitles.SettingsPage, true)
        {
            const int separatorRowHeight = 1;
            const int settingsRowHeight = 38;

            Content = new Grid
            {
                RowSpacing = 8,
                ColumnSpacing = 16.5,

                Margin = new Thickness(30, 0, 30, 5),

                RowDefinitions = Rows.Define(
                    (Row.GitHubUser, AbsoluteGridLength(GitHubUserView.TotalHeight)),
                    (Row.GitHubUserSeparator, AbsoluteGridLength(separatorRowHeight)),
                    (Row.Login, AbsoluteGridLength(settingsRowHeight)),
                    (Row.LoginSeparator, AbsoluteGridLength(separatorRowHeight)),
                    (Row.Notifications, AbsoluteGridLength(settingsRowHeight)),
                    (Row.NotificationsSeparator, AbsoluteGridLength(separatorRowHeight)),
                    (Row.Theme, AbsoluteGridLength(settingsRowHeight)),
                    (Row.ThemeSeparator, AbsoluteGridLength(separatorRowHeight)),
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

                    new SvgImage("logout.svg", getSVGIconColor).Row(Row.Login).Column(Column.Icon),
                    new LoginLabel().Row(Row.Login).Column(Column.Title),
                    new SvgImage("right_arrow.svg", getSVGIconColor).End().Row(Row.Login).Column(Column.Button),
                    new ConnectToGitHubTappableArea().Row(Row.Login).ColumnSpan(All<Column>()),

                    new Separator().Row(Row.LoginSeparator).ColumnSpan(All<Column>()),

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

        enum Row { GitHubUser, GitHubUserSeparator, Login, LoginSeparator, Notifications, NotificationsSeparator, Theme, ThemeSeparator, PreferredCharts, Copyright }
        enum Column { Icon, Title, Button }

        class ConnectToGitHubTappableArea : View
        {
            public ConnectToGitHubTappableArea()
            {
                Opacity = 0;

                AutomationId = SettingsPageAutomationIds.GitHubLoginLabel;

                var tapGesture = new TapGestureRecognizer();
                tapGesture.Tapped += HandleTapped;

                GestureRecognizers.Add(tapGesture);

                SetDynamicResource(BackgroundColorProperty, nameof(BaseTheme.PageBackgroundColor));
            }

            async void HandleTapped(object sender, EventArgs e)
            {
                if (BindingContext is SettingsViewModel settingsViewModel
                        && settingsViewModel.IsNotAuthenticating)
                {
                    Opacity = 0.7;

                    settingsViewModel.ConnectToGitHubButtonCommand.Execute(null);

                    await this.FadeTo(0, 350, Easing.CubicOut);
                }
            }
        }

        class LoginLabel : SettingsTitleLabel
        {
            public LoginLabel()
            {
                AutomationId = SettingsPageAutomationIds.GitHubLoginLabel;
                this.SetBinding(TextProperty, nameof(SettingsViewModel.LoginLabelText));
            }
        }

        class RegisterForNotificationsLabel : SettingsTitleLabel
        {
            public RegisterForNotificationsLabel() => Text = "Register for Notifications";
        }

        class ThemeLabel : SettingsTitleLabel
        {
            public ThemeLabel() => Text = "Theme";
        }

        class Separator : BoxView
        {
            public Separator() => SetDynamicResource(ColorProperty, nameof(BaseTheme.SeparatorColor));
        }

        class EnableNotificationsSwitch : SettingsSwitch
        {
            public EnableNotificationsSwitch()
            {
                AutomationId = SettingsPageAutomationIds.RegisterForNotificationsSwitch;

                this.SetBinding(IsToggledProperty, nameof(SettingsViewModel.IsRegisterForNotificationsSwitchToggled));
                this.SetBinding(IsEnabledProperty, nameof(SettingsViewModel.IsRegisterForNotificationsSwitchEnabled));
            }
        }

        class ThemePicker : Picker
        {
            public ThemePicker()
            {
                FontSize = 12;
                FontFamily = FontFamilyConstants.RobotoRegular;

                WidthRequest = 70;

                HorizontalOptions = LayoutOptions.EndAndExpand;
                VerticalOptions = LayoutOptions.EndAndExpand;

                AutomationId = SettingsPageAutomationIds.ThemePicker;

                SetDynamicResource(TextColorProperty, nameof(BaseTheme.SettingsLabelTextColor));
                SetDynamicResource(BackgroundColorProperty, nameof(BaseTheme.PageBackgroundColor));

                this.SetBinding(ItemsSourceProperty, nameof(SettingsViewModel.ThemePickerItemsSource));
                this.SetBinding(SelectedIndexProperty, nameof(SettingsViewModel.ThemePickerSelectedThemeIndex));
            }
        }
    }
}
