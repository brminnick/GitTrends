using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using GitTrends.Mobile.Common;
using GitTrends.Shared;
using Xamarin.CommunityToolkit.Markup;
using Xamarin.Essentials;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;
using static Xamarin.CommunityToolkit.Markup.GridRowsColumns;

namespace GitTrends
{
    public class SettingsPage : BaseContentPage<SettingsViewModel>
    {
        readonly Grid _contentGrid;
        readonly DeepLinkingService _deepLinkingService;
        readonly OrganizationsCarouselFrame _organizationsCarouselFrame;
        readonly GitTrendsStatisticsService _gitTrendsStatisticsService;

        CancellationTokenSource _connectToGitHubCancellationTokenSource = new();

        public SettingsPage(IMainThread mainThread,
                            IAnalyticsService analyticsService,
                            SettingsViewModel settingsViewModel,
                            DeepLinkingService deepLinkingService,
                            GitTrendsStatisticsService gitTrendsStatisticsService) : base(settingsViewModel, analyticsService, mainThread, true)
        {
            const int separatorRowHeight = 1;
            const int settingsRowHeight = 38;

            SettingsViewModel.OrganizationsCarouselViewVisiblilityChanged += HandleOrganizationsCarouselViewVisiblilityChanged;

            _deepLinkingService = deepLinkingService;
            _gitTrendsStatisticsService = gitTrendsStatisticsService;

            var loginRowTapGesture = new TapGestureRecognizer();
            loginRowTapGesture.Tapped += HandleLoginRowTapped;

            var aboutRowTapGesture = new TapGestureRecognizer();
            aboutRowTapGesture.Tapped += HandleAboutRowTapped;

            Content = new ScrollView
            {
                Content = _contentGrid = new Grid
                {
                    RowSpacing = 8,
                    ColumnSpacing = 16.5,

                    Margin = new Thickness(28, 0),

                    RowDefinitions = Rows.Define(
                        (Row.GitHubUser, GitHubUserView.TotalHeight),
                        (Row.GitHubUserSeparator, separatorRowHeight),
                        (Row.About, settingsRowHeight),
                        (Row.AboutSeparator, separatorRowHeight),
                        (Row.Login, settingsRowHeight),
                        (Row.LoginSeparator, separatorRowHeight),
                        (Row.Organizations, settingsRowHeight),
                        (Row.OrganizationsSeparator, separatorRowHeight),
                        (Row.Notifications, settingsRowHeight),
                        (Row.NotificationsSeparator, separatorRowHeight),
                        (Row.Theme, settingsRowHeight),
                        (Row.ThemeSeparator, separatorRowHeight),
                        (Row.Language, settingsRowHeight),
                        (Row.LanguageSeparator, separatorRowHeight),
                        (Row.PreferredCharts, settingsRowHeight),
                        (Row.PreferredChartsSeparator, separatorRowHeight),
                        (Row.CopyrightPadding, 20),
                        (Row.Copyright, Star)),

                    ColumnDefinitions = Columns.Define(
                        (Column.Icon, 24),
                        (Column.Title, Star),
                        (Column.Button, 100)),

                    Children =
                    {
                        new GitHubUserView()
                            .Row(Row.GitHubUser).ColumnSpan(All<Column>()),

                        new Separator()
                            .Row(Row.GitHubUserSeparator).ColumnSpan(All<Column>()),

                        new AboutRowTappableView(aboutRowTapGesture)
                            .Row(Row.About).ColumnSpan(All<Column>()),
                        new AboutRowSvg("about.svg", getSVGIconColor)
                            .Row(Row.About).Column(Column.Icon),
                        new AboutTitleLabel(SettingsPageAutomationIds.AboutTitleLabel)
                            .Row(Row.About).Column(Column.Title)
                            .Bind(Label.TextProperty, nameof(SettingsViewModel.AboutLabelText)),
                        new AboutRowSvg("right_arrow.svg", getSVGIconColor).End()
                            .Row(Row.About).Column(Column.Button),

                        new Separator()
                            .Row(Row.AboutSeparator).ColumnSpan(All<Column>()),

                        new LoginRowTappableView(loginRowTapGesture)
                            .Row(Row.Login).ColumnSpan(All<Column>()),
                        new LoginRowSvg("logout.svg", getSVGIconColor)
                            .Row(Row.Login).Column(Column.Icon),
                        new LoginLabel()
                            .Row(Row.Login).Column(Column.Title),
                        new LoginRowSvg("right_arrow.svg", getSVGIconColor).End()
                            .Row(Row.Login).Column(Column.Button),

                        new Separator()
                            .Row(Row.LoginSeparator).ColumnSpan(All<Column>()),

                        new SvgImage("organization.svg", getSVGIconColor)
                            .Row(Row.Organizations).Column(Column.Icon),
                        new SettingsTitleLabel(SettingsPageAutomationIds.IncludeOrganizationsSwitch)
                            .Row(Row.Organizations).Column(Column.Title)
                            .Bind(Label.TextProperty, nameof(SettingsViewModel.ShouldIncludeOrganizationsLabelText)),
                        new IncludeOrganizationsSwitch()
                            .Row(Row.Organizations).Column(Column.Button),

                        new Separator()
                            .Row(Row.OrganizationsSeparator).ColumnSpan(All<Column>()),

                        new SvgImage("bell.svg", getSVGIconColor)
                            .Row(Row.Notifications).Column(Column.Icon),
                        new SettingsTitleLabel(SettingsPageAutomationIds.RegisterForNotificationsTitleLabel)
                            .Row(Row.Notifications).Column(Column.Title)
                            .Bind(Label.TextProperty, nameof(SettingsViewModel.RegisterForNotificationsLabelText)),
                        new EnableNotificationsSwitch()
                            .Row(Row.Notifications).Column(Column.Button),

                        new Separator()
                            .Row(Row.NotificationsSeparator).ColumnSpan(All<Column>()),

                        new SvgImage("theme.svg", getSVGIconColor)
                            .Row(Row.Theme).Column(Column.Icon),
                        new SettingsTitleLabel(SettingsPageAutomationIds.ThemeTitleLabel)
                            .Row(Row.Theme).Column(Column.Title)
                            .Bind(Label.TextProperty, nameof(SettingsViewModel.ThemeLabelText)),
                        new SettingsPicker(SettingsPageAutomationIds.ThemePicker, 70)
                            .Row(Row.Theme).Column(Column.Button)
                            .Bind(Picker.ItemsSourceProperty, nameof(SettingsViewModel.ThemePickerItemsSource))
                            .Bind(Picker.SelectedIndexProperty, nameof(SettingsViewModel.ThemePickerSelectedIndex)),

                        new Separator()
                            .Row(Row.ThemeSeparator).ColumnSpan(All<Column>()),

                        new SvgImage("language.svg", getSVGIconColor)
                            .Row(Row.Language).Column(Column.Icon),
                        new SettingsTitleLabel(SettingsPageAutomationIds.LanguageTitleLabel)
                            .Row(Row.Language).Column(Column.Title)
                            .Bind(Label.TextProperty, nameof(SettingsViewModel.LanguageLabelText)),
                        new SettingsPicker(SettingsPageAutomationIds.LanguagePicker, 100)
                            .Row(Row.Language).Column(Column.Button)
                            .Bind(Picker.ItemsSourceProperty, nameof(SettingsViewModel.LanguagePickerItemsSource))
                            .Bind(Picker.SelectedIndexProperty, nameof(SettingsViewModel.LanguagePickerSelectedIndex)),

                        new Separator()
                            .Row(Row.LanguageSeparator).ColumnSpan(All<Column>()),

                        new SvgImage("chart.svg", getSVGIconColor)
                            .Row(Row.PreferredCharts).Column(Column.Icon),
                        new SettingsTitleLabel(SettingsPageAutomationIds.PreferredChartTitleLabel)
                            .Row(Row.PreferredCharts).Column(Column.Title)
                            .Bind(Label.TextProperty, nameof(SettingsViewModel.PreferredChartsLabelText)),
                        new SettingsPicker(SettingsPageAutomationIds.PreferredChartsPicker, 100).Assign(out Picker preferredChartsPicker)
                            .Row(Row.PreferredCharts).Column(Column.Button)
                            .Bind(Picker.ItemsSourceProperty, nameof(SettingsViewModel.PreferredChartsItemsSource))
                            .Bind(Picker.SelectedIndexProperty, nameof(SettingsViewModel.PreferredChartsSelectedIndex)),

                        new Separator()
                            .Row(Row.PreferredChartsSeparator).ColumnSpan(All<Column>()),

                        new CopyrightLabel()
                            .Row(Row.Copyright).ColumnSpan(All<Column>()),

                        new OrganizationsCarouselFrame(analyticsService).Assign(out _organizationsCarouselFrame)
                            .Row(Row.GitHubUser).Column(Column.Icon)
                            .RowSpan(All<Row>()).ColumnSpan(All<Column>())
                    }
                }
            }.Paddings(bottom: 8);

            this.SetBinding(TitleProperty, nameof(SettingsViewModel.TitleText));

            static Color getSVGIconColor() => (Color)Application.Current.Resources[nameof(BaseTheme.IconColor)];
        }

        enum Row { GitHubUser, GitHubUserSeparator, About, AboutSeparator, Login, LoginSeparator, Organizations, OrganizationsSeparator, Notifications, NotificationsSeparator, Theme, ThemeSeparator, Language, LanguageSeparator, PreferredCharts, PreferredChartsSeparator, CopyrightPadding, Copyright }
        enum Column { Icon, Title, Button }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            _connectToGitHubCancellationTokenSource = new();
        }

        protected override void OnDisappearing()
        {
            _connectToGitHubCancellationTokenSource.Cancel();

            base.OnDisappearing();
        }

        async void HandleAboutRowTapped(object sender, EventArgs e)
        {
            AnalyticsService.Track("About Button Tapped");

            var aboutPage = ContainerService.Container.Resolve<AboutPage>();
            await Navigation.PushAsync(aboutPage);
        }

        async void HandleLoginRowTapped(object sender, EventArgs e)
        {
            AnalyticsService.Track("Login Button Tapped", nameof(ViewModel.IsNotAuthenticating), ViewModel.IsNotAuthenticating.ToString());

            if (ViewModel.IsNotAuthenticating)
            {
                var loginRowViews = _contentGrid.Children.OfType<ILoginRowView>().Cast<View>();

                await Task.WhenAll(loginRowViews.Select(x => x.FadeTo(0.3, 75)));

                ViewModel.ConnectToGitHubButtonCommand.Execute((_connectToGitHubCancellationTokenSource.Token, (BrowserLaunchOptions?)null));

                await Task.WhenAll(loginRowViews.Select(x => x.FadeTo(1, 350, Easing.CubicOut)));
            }
        }

        async void HandleLaunchOrganzationsButtonTapped(object sender, EventArgs e)
        {
            if (_gitTrendsStatisticsService.EnableOrganizationsUri is null)
                throw new InvalidOperationException($"{nameof(GitTrendsStatisticsService)}.{nameof(GitTrendsStatisticsService.EnableOrganizationsUri)} Must Be Initialized");

            await _deepLinkingService.OpenBrowser(_gitTrendsStatisticsService.EnableOrganizationsUri);
        }

        async void HandleOrganizationsCarouselViewVisiblilityChanged(object sender, bool isVisible) => await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            if (isVisible)
            {
                await _organizationsCarouselFrame.FadeTo(1, 250);
                AnalyticsService.Track($"OrganizationsCarouselView Page 1 Appeared");
            }
            else
            {
                await _organizationsCarouselFrame.FadeTo(0, 250);
                AnalyticsService.Track($"OrganizationsCarouselView Dismissed");
            }
        });

        class AboutRowTappableView : View
        {
            public AboutRowTappableView(TapGestureRecognizer tapGestureRecognizer) => GestureRecognizers.Add(tapGestureRecognizer);
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

        class AboutTitleLabel : SettingsTitleLabel
        {
            public AboutTitleLabel(in string automationId) : base(automationId)
            {
                //Allow AboutRowTappableView to handle taps
                InputTransparent = true;
            }
        }

        class AboutRowSvg : SvgImage
        {
            public AboutRowSvg(in string svgFileName, in Func<Color> getTextColor) : base(svgFileName, getTextColor)
            {
                //Allow AboutRowTappableView to handle taps
                InputTransparent = true;
                AutomationId = SettingsPageAutomationIds.AboutButton;
            }
        }

        class SettingsTitleLabel : TitleLabel
        {
            public SettingsTitleLabel(in string automationId) => AutomationId = automationId;
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

        class IncludeOrganizationsSwitch : SettingsSwitch
        {
            public IncludeOrganizationsSwitch()
            {
                AutomationId = SettingsPageAutomationIds.IncludeOrganizationsSwitch;

                this.SetBinding(IsToggledProperty, nameof(SettingsViewModel.IsShouldIncludeOrganizationsSwitchToggled));
                this.SetBinding(IsEnabledProperty, nameof(SettingsViewModel.IsShouldIncludeOrganizationsSwitchEnabled));
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
