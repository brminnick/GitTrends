using System;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Markup;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace GitTrends
{
    class WelcomePage : BaseContentPage<WelcomeViewModel>
    {
        public WelcomePage(GitHubAuthenticationService gitHubAuthenticationService, AnalyticsService analyticsService, WelcomeViewModel welcomeViewModel)
            : base(welcomeViewModel, analyticsService, shouldUseSafeArea: true)
        {
            On<iOS>().SetModalPresentationStyle(UIModalPresentationStyle.OverFullScreen);

            gitHubAuthenticationService.DemoUserActivated += HandleDemoUserActivated;
            gitHubAuthenticationService.AuthorizeSessionCompleted += HandleAuthorizeSessionCompleted;

            Content = new StackLayout
            {
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Children =
                {
                    new Label { Text = "Temporary Welcome Page" },
                    new Button { Text = "Connect to GtHub" }
                        .Bind(Button.CommandProperty, nameof(WelcomeViewModel.ConnectToGitHubButtonCommand)),
                    new Button { Text = "Demo Mode" }
                        .Bind(Button.CommandProperty, nameof(WelcomeViewModel.DemoButtonCommand))
                }
            };
        }

        async void HandleAuthorizeSessionCompleted(object sender, AuthorizeSessionCompletedEventArgs e)
        {
            if (e.IsSessionAuthorized)
                await PopPage();
        }

        async void HandleDemoUserActivated(object sender, EventArgs e) => await PopPage();

        Task PopPage() => MainThread.InvokeOnMainThreadAsync(Navigation.PopModalAsync);
    }
}
