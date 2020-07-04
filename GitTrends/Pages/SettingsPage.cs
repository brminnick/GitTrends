using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GitTrends.Mobile.Common;
using GitTrends.Shared;
using Xamarin.Essentials;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;
using Xamarin.Forms.Markup;
using static GitTrends.MarkupExtensions;
using static Xamarin.Forms.Markup.GridRowsColumns;

namespace GitTrends
{
    public class SettingsPage : BaseContentPage<SettingsViewModel>
    {
        readonly CancellationTokenSource _connectToGitHubCancellationTokenSource = new CancellationTokenSource();
        readonly Grid _contentGrid;

        public SettingsPage(SettingsViewModel settingsViewModel,
                            IAnalyticsService analyticsService,
                            IMainThread mainThread) : base(settingsViewModel, analyticsService, mainThread, true)
        {
            const int separatorRowHeight = 1;
            const int settingsRowHeight = 38;

            var loginRowTapGesture = new TapGestureRecognizer();
            loginRowTapGesture.Tapped += HandleLoginRowTapped;

            Content = new ScrollView
            {
                Content = _contentGrid = new Grid
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
                        (Row.Language, AbsoluteGridLength(settingsRowHeight)),
                        (Row.LanguageSeparator, AbsoluteGridLength(separatorRowHeight)),
                        (Row.PreferredCharts, AbsoluteGridLength(80)),
                        (Row.CopyrightPadding, AbsoluteGridLength(20)),
                        (Row.Copyright, Star)),

                    ColumnDefinitions = Columns.Define(
                        (Column.Icon, AbsoluteGridLength(24)),
                        (Column.Title, StarGridLength(5)),
                        (Column.Button, StarGridLength(3))),

                    Children =
                    {
                        new GitHubUserView().Row(Row.GitHubUser).ColumnSpan(All<Column>()),

                        new Separator().Row(Row.GitHubUserSeparator).ColumnSpan(All<Column>()),

                        new LoginRowTappableView(loginRowTapGesture).Row(Row.Login).ColumnSpan(All<Column>()),
                        new LoginRowSvg("logout.svg", getSVGIconColor).Row(Row.Login).Column(Column.Icon),
                        new LoginLabel().Row(Row.Login).Column(Column.Title),
                        new LoginRowSvg("right_arrow.svg", getSVGIconColor).End().Row(Row.Login).Column(Column.Button),

                        new Separator().Row(Row.LoginSeparator).ColumnSpan(All<Column>()),

                        new SvgImage("bell.svg", getSVGIconColor).Row(Row.Notifications).Column(Column.Icon),
                        new SettingsTitleLabel(SettingsPageAutomationIds.RegisterForNotificationsTitleLabel).Row(Row.Notifications).Column(Column.Title)
                            .Bind(Label.TextProperty, nameof(SettingsViewModel.RegisterForNotificationsLabelText)),
                        new EnableNotificationsSwitch().Row(Row.Notifications).Column(Column.Button),

                        new Separator().Row(Row.NotificationsSeparator).ColumnSpan(All<Column>()),

                        new SvgImage("theme.svg", getSVGIconColor).Row(Row.Theme).Column(Column.Icon),
                        new SettingsTitleLabel(SettingsPageAutomationIds.ThemeTitleLabel).Row(Row.Theme).Column(Column.Title)
                            .Bind(Label.TextProperty, nameof(SettingsViewModel.ThemeLabelText)),
                        new SettingsPicker(SettingsPageAutomationIds.ThemePicker, 70).Row(Row.Theme).Column(Column.Button)
                            .Bind(Picker.ItemsSourceProperty, nameof(SettingsViewModel.ThemePickerItemsSource))
                            .Bind(Picker.SelectedIndexProperty, nameof(SettingsViewModel.ThemePickerSelectedIndex)),

                        new Separator().Row(Row.ThemeSeparator).ColumnSpan(All<Column>()),

                        new SvgImage("language.svg", getSVGIconColor).Row(Row.Language).Column(Column.Icon),
                        new SettingsTitleLabel(SettingsPageAutomationIds.LanguageTitleLabel).Row(Row.Language).Column(Column.Title)
                            .Bind(Label.TextProperty, nameof(SettingsViewModel.LanguageLabelText)),
                        new SettingsPicker(SettingsPageAutomationIds.LanguagePicker, 100).Row(Row.Language).Column(Column.Button)
                            .Bind(Picker.ItemsSourceProperty, nameof(SettingsViewModel.LanguagePickerItemsSource))
                            .Bind(Picker.SelectedIndexProperty, nameof(SettingsViewModel.LanguagePickerSelectedIndex)),

                        new Separator().Row(Row.LanguageSeparator).ColumnSpan(All<Column>()),

                        new PreferredChartsView(settingsViewModel, mainThread).Row(Row.PreferredCharts).ColumnSpan(All<Column>()),

                        new CopyrightLabel().Row(Row.Copyright).ColumnSpan(All<Column>())
                    }
                }
            };

            this.SetBinding(TitleProperty, nameof(SettingsViewModel.TitleText));

            static Color getSVGIconColor() => (Color)Application.Current.Resources[nameof(BaseTheme.IconColor)];
        }

        enum Row { GitHubUser, GitHubUserSeparator, Login, LoginSeparator, Notifications, NotificationsSeparator, Theme, ThemeSeparator, Language, LanguageSeparator, PreferredCharts, CopyrightPadding, Copyright }
        enum Column { Icon, Title, Button }

        protected override void OnDisappearing()
        {
            _connectToGitHubCancellationTokenSource.Cancel();

            base.OnDisappearing();
        }

        async void HandleLoginRowTapped(object sender, EventArgs e)
        {
            if (ViewModel.IsNotAuthenticating)
            {
                var loginRowViews = _contentGrid.Children.OfType<ILoginRowView>().Cast<View>();

                await Task.WhenAll(loginRowViews.Select(x => x.FadeTo(0.3, 75)));

                ViewModel.ConnectToGitHubButtonCommand.Execute((_connectToGitHubCancellationTokenSource.Token, (BrowserLaunchOptions?)null));

                await Task.WhenAll(loginRowViews.Select(x => x.FadeTo(1, 350, Easing.CubicOut)));
            }
        }

        class LoginRowTappableView : View, ILoginRowView
        {
            public LoginRowTappableView(TapGestureRecognizer tapGestureRecognizer) => GestureRecognizers.Add(tapGestureRecognizer);
        }

        class LoginRowSvg : SvgImage, ILoginRowView
        {
            public LoginRowSvg(in string svgFileName, in Func<Color> getTextColor) : base(svgFileName, getTextColor)
            {
                //Allow LoginRowTappableView to handle taps
                InputTransparent = true;
            }
        }

        class LoginLabel : TitleLabel, ILoginRowView
        {
            public LoginLabel()
            {
                this.FillExpand();
                HorizontalTextAlignment = TextAlignment.Start;

                AutomationId = SettingsPageAutomationIds.LoginTitleLabel;

                //Allow LoginRowTappableView to handle taps
                InputTransparent = true;

                this.SetBinding(TextProperty, nameof(SettingsViewModel.LoginLabelText));
            }
        }

        class SettingsTitleLabel : TitleLabel
        {
            public SettingsTitleLabel(in string automationId)=> AutomationId = automationId;
        }

        class Separator : BoxView
        {
            public Separator() => this.DynamicResource(ColorProperty, nameof(BaseTheme.SeparatorColor));
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

        class SettingsPicker : Picker
        {
            public SettingsPicker(in string automationId, in int widthRequest)
            {
                FontSize = 12;
                WidthRequest = widthRequest;
                AutomationId = automationId;
                FontFamily = FontFamilyConstants.RobotoMedium;

                this.EndExpand();

                this.DynamicResources((TextColorProperty, nameof(BaseTheme.SettingsLabelTextColor)), 
                                        (BackgroundColorProperty, nameof(BaseTheme.PageBackgroundColor)));
            }
        }

        interface ILoginRowView
        {

        }
    }
}
