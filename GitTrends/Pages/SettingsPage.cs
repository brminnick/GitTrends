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
        readonly OrganizationsCarouselOverlay _organizationsCarouselOverlay;

        CancellationTokenSource _connectToGitHubCancellationTokenSource = new();

        public SettingsPage(IMainThread mainThread,
                            IAnalyticsService analyticsService,
                            SettingsViewModel settingsViewModel,
                            MediaElementService mediaElementService) : base(settingsViewModel, analyticsService, mainThread)
        {
            const int separatorRowHeight = 1;
            const int settingsRowHeight = 38;

            SettingsViewModel.OrganizationsCarouselViewVisiblilityChanged += HandleOrganizationsCarouselViewVisiblilityChanged;

            var loginRowTapGesture = new TapGestureRecognizer();
            loginRowTapGesture.Tapped += HandleLoginRowTapped;

            var aboutRowTapGesture = new TapGestureRecognizer();
            aboutRowTapGesture.Tapped += HandleAboutRowTapped;

            Content = new Grid
            {
                Children =
                {
                    new ScrollView
                    {
                        Content = new Grid
                        {
                            RowSpacing = 8,
                            ColumnSpacing = 16.5,

                            RowDefinitions = Rows.Define(
                                (SettingsRow.GitHubUser, GitHubUserView.TotalHeight),
                                (SettingsRow.GitHubUserSeparator, separatorRowHeight),
                                (SettingsRow.About, settingsRowHeight),
                                (SettingsRow.AboutSeparator, separatorRowHeight),
                                (SettingsRow.Login, settingsRowHeight),
                                (SettingsRow.LoginSeparator, separatorRowHeight),
                                (SettingsRow.Organizations, settingsRowHeight),
                                (SettingsRow.OrganizationsSeparator, separatorRowHeight),
                                (SettingsRow.Notifications, settingsRowHeight),
                                (SettingsRow.NotificationsSeparator, separatorRowHeight),
                                (SettingsRow.Theme, settingsRowHeight),
                                (SettingsRow.ThemeSeparator, separatorRowHeight),
                                (SettingsRow.Language, settingsRowHeight),
                                (SettingsRow.LanguageSeparator, separatorRowHeight),
                                (SettingsRow.PreferredCharts, settingsRowHeight),
                                (SettingsRow.PreferredChartsSeparator, separatorRowHeight),
                                (SettingsRow.CopyrightPadding, 20),
                                (SettingsRow.Copyright, Star)),

                            ColumnDefinitions = Columns.Define(
                                (SettingsColumn.Icon, 24),
                                (SettingsColumn.Title, Star),
                                (SettingsColumn.Button, 100)),

                            Children =
                            {
                                new GitHubUserView()
                                    .Row(SettingsRow.GitHubUser).ColumnSpan(All<SettingsColumn>()),

                                new Separator()
                                    .Row(SettingsRow.GitHubUserSeparator).ColumnSpan(All<SettingsColumn>()),

                                new AboutRowTappableView(aboutRowTapGesture)
                                    .Row(SettingsRow.About).ColumnSpan(All<SettingsColumn>()),
                                new AboutRowSvg("about.svg", getSVGIconColor)
                                    .Row(SettingsRow.About).Column(SettingsColumn.Icon),
                                new AboutTitleLabel(SettingsPageAutomationIds.AboutTitleLabel)
                                    .Row(SettingsRow.About).Column(SettingsColumn.Title)
                                    .Bind(Label.TextProperty, nameof(SettingsViewModel.AboutLabelText)),
                                new AboutRowSvg("right_arrow.svg", getSVGIconColor).End()
                                    .Row(SettingsRow.About).Column(SettingsColumn.Button),

                                new Separator()
                                    .Row(SettingsRow.AboutSeparator).ColumnSpan(All<SettingsColumn>()),

                                new LoginRowTappableView(loginRowTapGesture)
                                    .Row(SettingsRow.Login).ColumnSpan(All<SettingsColumn>()),
                                new LoginRowSvg("logout.svg", getSVGIconColor)
                                    .Row(SettingsRow.Login).Column(SettingsColumn.Icon),
                                new LoginLabel()
                                    .Row(SettingsRow.Login).Column(SettingsColumn.Title),
                                new LoginRowSvg("right_arrow.svg", getSVGIconColor).End()
                                    .Row(SettingsRow.Login).Column(SettingsColumn.Button),

                                new Separator()
                                    .Row(SettingsRow.LoginSeparator).ColumnSpan(All<SettingsColumn>()),

                                new SvgImage("organization.svg", getSVGIconColor)
                                    .Row(SettingsRow.Organizations).Column(SettingsColumn.Icon),
                                new SettingsTitleLabel(SettingsPageAutomationIds.IncludeOrganizationsSwitch)
                                    .Row(SettingsRow.Organizations).Column(SettingsColumn.Title)
                                    .Bind(Label.TextProperty, nameof(SettingsViewModel.ShouldIncludeOrganizationsLabelText)),
                                new IncludeOrganizationsSwitch()
                                    .Row(SettingsRow.Organizations).Column(SettingsColumn.Button),

                                new Separator()
                                    .Row(SettingsRow.OrganizationsSeparator).ColumnSpan(All<SettingsColumn>()),

                                new SvgImage("bell.svg", getSVGIconColor)
                                    .Row(SettingsRow.Notifications).Column(SettingsColumn.Icon),
                                new SettingsTitleLabel(SettingsPageAutomationIds.RegisterForNotificationsTitleLabel)
                                    .Row(SettingsRow.Notifications).Column(SettingsColumn.Title)
                                    .Bind(Label.TextProperty, nameof(SettingsViewModel.RegisterForNotificationsLabelText)),
                                new EnableNotificationsSwitch()
                                    .Row(SettingsRow.Notifications).Column(SettingsColumn.Button),

                                new Separator()
                                    .Row(SettingsRow.NotificationsSeparator).ColumnSpan(All<SettingsColumn>()),

                                new SvgImage("theme.svg", getSVGIconColor)
                                    .Row(SettingsRow.Theme).Column(SettingsColumn.Icon),
                                new SettingsTitleLabel(SettingsPageAutomationIds.ThemeTitleLabel)
                                    .Row(SettingsRow.Theme).Column(SettingsColumn.Title)
                                    .Bind(Label.TextProperty, nameof(SettingsViewModel.ThemeLabelText)),
                                new SettingsPicker(SettingsPageAutomationIds.ThemePicker, 70)
                                    .Row(SettingsRow.Theme).Column(SettingsColumn.Button)
                                    .Bind(Picker.ItemsSourceProperty, nameof(SettingsViewModel.ThemePickerItemsSource))
                                    .Bind(Picker.SelectedIndexProperty, nameof(SettingsViewModel.ThemePickerSelectedIndex)),

                                new Separator()
                                    .Row(SettingsRow.ThemeSeparator).ColumnSpan(All<SettingsColumn>()),

                                new SvgImage("language.svg", getSVGIconColor)
                                    .Row(SettingsRow.Language).Column(SettingsColumn.Icon),
                                new SettingsTitleLabel(SettingsPageAutomationIds.LanguageTitleLabel)
                                    .Row(SettingsRow.Language).Column(SettingsColumn.Title)
                                    .Bind(Label.TextProperty, nameof(SettingsViewModel.LanguageLabelText)),
                                new SettingsPicker(SettingsPageAutomationIds.LanguagePicker, 100)
                                    .Row(SettingsRow.Language).Column(SettingsColumn.Button)
                                    .Bind(Picker.ItemsSourceProperty, nameof(SettingsViewModel.LanguagePickerItemsSource))
                                    .Bind(Picker.SelectedIndexProperty, nameof(SettingsViewModel.LanguagePickerSelectedIndex)),

                                new Separator()
                                    .Row(SettingsRow.LanguageSeparator).ColumnSpan(All<SettingsColumn>()),

                                new SvgImage("chart.svg", getSVGIconColor)
                                    .Row(SettingsRow.PreferredCharts).Column(SettingsColumn.Icon),
                                new SettingsTitleLabel(SettingsPageAutomationIds.PreferredChartTitleLabel)
                                    .Row(SettingsRow.PreferredCharts).Column(SettingsColumn.Title)
                                    .Bind(Label.TextProperty, nameof(SettingsViewModel.PreferredChartsLabelText)),
                                new SettingsPicker(SettingsPageAutomationIds.PreferredChartsPicker, 100)
                                    .Row(SettingsRow.PreferredCharts).Column(SettingsColumn.Button)
                                    .Bind(Picker.ItemsSourceProperty, nameof(SettingsViewModel.PreferredChartsItemsSource))
                                    .Bind(Picker.SelectedIndexProperty, nameof(SettingsViewModel.PreferredChartsSelectedIndex)),

                                new Separator()
                                    .Row(SettingsRow.PreferredChartsSeparator).ColumnSpan(All<SettingsColumn>()),

                                new CopyrightLabel()
                                    .Row(SettingsRow.Copyright).ColumnSpan(All<SettingsColumn>()),
                            }
                        }.Assign(out _contentGrid)
                    }.Paddings(left: 28, right: 28, bottom: 8),

                    new OrganizationsCarouselOverlay(mainThread, analyticsService, mediaElementService).Assign(out _organizationsCarouselOverlay),
                }
            };

            this.SetBinding(TitleProperty, nameof(SettingsViewModel.TitleText));

            static Color getSVGIconColor() => (Color)Application.Current.Resources[nameof(BaseTheme.IconColor)];
        }

        enum SettingsRow { GitHubUser, GitHubUserSeparator, About, AboutSeparator, Login, LoginSeparator, Organizations, OrganizationsSeparator, Notifications, NotificationsSeparator, Theme, ThemeSeparator, Language, LanguageSeparator, PreferredCharts, PreferredChartsSeparator, CopyrightPadding, Copyright }
        enum SettingsColumn { Icon, Title, Button }

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

        async void HandleOrganizationsCarouselViewVisiblilityChanged(object sender, bool isVisible) => await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            if (isVisible)
                await _organizationsCarouselOverlay.Reveal(true);
            else
                await _organizationsCarouselOverlay.Dismiss(true);
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
