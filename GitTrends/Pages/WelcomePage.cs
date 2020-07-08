using System;
using System.Threading;
using System.Threading.Tasks;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using GitTrends.Shared;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;
using Xamarin.Forms.Markup;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;
using static GitTrends.MarkupExtensions;
using static Xamarin.Forms.Markup.GridRowsColumns;

namespace GitTrends
{
    public class WelcomePage : BaseContentPage<WelcomeViewModel>
    {
        const int _demoLabelFontSize = 16;

        readonly CancellationTokenSource _connectToGitHubCancellationTokenSource = new CancellationTokenSource();
        readonly IAppInfo _appInfo;

        public WelcomePage(IAppInfo appInfo,
                            IMainThread mainThread,
                            WelcomeViewModel welcomeViewModel,
                            IAnalyticsService analyticsService)
            : base(welcomeViewModel, analyticsService, mainThread, shouldUseSafeArea: true)
        {
            _appInfo = appInfo;

            RemoveDynamicResource(BackgroundColorProperty);
            On<iOS>().SetModalPresentationStyle(UIModalPresentationStyle.OverFullScreen);

            var pageBackgroundColor = Color.FromHex(BaseTheme.LightTealColorHex);
            BackgroundColor = pageBackgroundColor;

            GitHubAuthenticationService.DemoUserActivated += HandleDemoUserActivated;
            GitHubAuthenticationService.AuthorizeSessionCompleted += HandleAuthorizeSessionCompleted;

            var browserLaunchOptions = new Xamarin.Essentials.BrowserLaunchOptions
            {
                PreferredToolbarColor = pageBackgroundColor.MultiplyAlpha(0.75),
                PreferredControlColor = Color.White,
                Flags = Xamarin.Essentials.BrowserLaunchFlags.PresentAsFormSheet
            };

            Content = new Grid
            {
                RowSpacing = 24,

                RowDefinitions = Rows.Define(
                    (Row.WelcomeLabel, StarGridLength(2)),
                    (Row.Image, StarGridLength(4)),
                    (Row.GitHubButton, StarGridLength(2)),
                    (Row.DemoButton, StarGridLength(1)),
                    (Row.VersionLabel, StarGridLength(1))),

                Children =
                {
                    new WelcomeLabel()
                        .Row(Row.WelcomeLabel),
                    new Image { Source = "WelcomeImage" }.Center()
                        .Row(Row.Image),
                    new ConnectToGitHubButton(_connectToGitHubCancellationTokenSource.Token, browserLaunchOptions)
                        .Row(Row.GitHubButton),
                    new DemoLabel()
                        .Row(Row.DemoButton),
                    new ConnectToGitHubActivityIndicator()
                        .Row(Row.DemoButton),
                    new VersionNumberLabel(_appInfo)
                        .Row(Row.VersionLabel)
                }
            };
        }

        enum Row { WelcomeLabel, Image, GitHubButton, DemoButton, VersionLabel }

        protected override void OnDisappearing()
        {
            _connectToGitHubCancellationTokenSource.Cancel();

            base.OnDisappearing();
        }

        async void HandleAuthorizeSessionCompleted(object sender, AuthorizeSessionCompletedEventArgs e)
        {
            if (e.IsSessionAuthorized)
                await PopPage();
        }

        async void HandleDemoUserActivated(object sender, EventArgs e)
        {
            var minimumActivityIndicatorTime = Task.Delay(TimeSpan.FromMilliseconds(1500));

            await minimumActivityIndicatorTime;
            await PopPage();
        }

        Task PopPage() => MainThread.InvokeOnMainThreadAsync(Navigation.PopModalAsync);

        class ConnectToGitHubActivityIndicator : ActivityIndicator
        {
            public ConnectToGitHubActivityIndicator()
            {
                Color = Color.White;

                AutomationId = WelcomePageAutomationIds.IsAuthenticatingActivityIndicator;

                HorizontalOptions = LayoutOptions.Center;
                VerticalOptions = LayoutOptions.Start;

                HeightRequest = WidthRequest = _demoLabelFontSize;

                this.Bind(IsVisibleProperty, nameof(WelcomeViewModel.IsAuthenticating));
                this.Bind(IsRunningProperty, nameof(WelcomeViewModel.IsAuthenticating));
            }
        }

        class ConnectToGitHubButton : ConnectToGitHubView
        {
            public ConnectToGitHubButton(CancellationToken cancellationToken, Xamarin.Essentials.BrowserLaunchOptions browserLaunchOptions) : base(WelcomePageAutomationIds.ConnectToGitHubButton, cancellationToken, browserLaunchOptions)
            {
                HorizontalOptions = LayoutOptions.Center;
                VerticalOptions = LayoutOptions.End;
            }
        }

        class VersionNumberLabel : Label
        {
            public VersionNumberLabel(IAppInfo appInfo)
            {
                Text = $"v{appInfo.Version}";
                TextColor = Color.White;

                FontSize = 12;
                FontFamily = FontFamilyConstants.RobotoBold;

                this.Center();

                HorizontalTextAlignment = TextAlignment.Center;
                VerticalTextAlignment = TextAlignment.End;
            }
        }

        class DemoLabel : Label
        {
            public DemoLabel()
            {
                Text = WelcomePageConstants.TryDemo;
                TextColor = Color.White;

                FontSize = _demoLabelFontSize;
                FontFamily = FontFamilyConstants.RobotoRegular;

                HorizontalOptions = LayoutOptions.Center;
                VerticalOptions = LayoutOptions.Start;

                HorizontalTextAlignment = TextAlignment.Center;
                VerticalTextAlignment = TextAlignment.Start;

                Opacity = 0.75;

                AutomationId = WelcomePageAutomationIds.DemoModeButton;

                this.BindTapGesture(nameof(WelcomeViewModel.DemoButtonCommand));
                this.Bind(IsVisibleProperty, nameof(WelcomeViewModel.IsDemoButtonVisible));
            }
        }

        class WelcomeLabel : Label
        {
            public WelcomeLabel()
            {
                HorizontalTextAlignment = TextAlignment.Center;
                VerticalTextAlignment = TextAlignment.Center;

                TextColor = Color.White;
                FormattedText = new FormattedString
                {
                    Spans =
                    {
                        new Span
                        {
                            FontSize = 32,
                            FontFamily = FontFamilyConstants.RobotoBold,
                            Text = WelcomePageConstants.WelcomeToGitTrends,
                        },
                        new Span
                        {
                            Text = "\n"
                        },
                        new Span
                        {
                            FontSize = 16,
                            FontFamily = FontFamilyConstants.RobotoRegular,
                            Text = WelcomePageConstants.MonitorYourRepos
                        }
                    }
                };
            }
        }
    }
}
