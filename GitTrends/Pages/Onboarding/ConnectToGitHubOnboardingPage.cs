using System.Threading;
using GitTrends.Mobile.Shared;
using GitTrends.Shared;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;
using Xamarin.Forms.Markup;
using static GitTrends.XamarinFormsService;
using static Xamarin.Forms.Markup.GridRowsColumns;

namespace GitTrends
{
    public class ConnectToGitHubOnboardingPage : BaseOnboardingContentPage
    {
        public ConnectToGitHubOnboardingPage(GitHubAuthenticationService gitHubAuthenticationService,
                                                IAnalyticsService analyticsService,
                                                IMainThread mainthread)
                : base(analyticsService, mainthread, Color.FromHex(BaseTheme.CoralColorHex), OnboardingConstants.TryDemoText, 3)
        {
            gitHubAuthenticationService.AuthorizeSessionCompleted += HandleAuthorizeSessionCompleted;
        }

        enum Row { Description, Button, ActivityIndicator }

        protected override View CreateImageView() => new Image
        {
            Source = "ConnectToGitHubOnboarding",
            HorizontalOptions = LayoutOptions.CenterAndExpand,
            VerticalOptions = LayoutOptions.CenterAndExpand,
            Aspect = Aspect.AspectFit
        };

        protected override TitleLabel CreateDescriptionTitleLabel() => new TitleLabel(OnboardingConstants.ConnectToGitHubPageTitle);

        protected override View CreateDescriptionBodyView() => new Grid
        {
            RowSpacing = 16,

            RowDefinitions = Rows.Define(
                (Row.Description, AbsoluteGridLength(65)),
                (Row.Button, AbsoluteGridLength(42)),
                (Row.ActivityIndicator, AbsoluteGridLength(42))),

            Children =
            {
                new BodyLabel("Get started by connecting your GitHub account, or try demo mode to tour GitTrends!").Row(Row.Description),
                new ConnectToGitHubView(OnboardingAutomationIds.ConnectToGitHubButton, CancellationToken.None, new Xamarin.Essentials.BrowserLaunchOptions
                {
                    PreferredControlColor = Color.White,
                    PreferredToolbarColor = Color.FromHex(BaseTheme.CoralColorHex).MultiplyAlpha(0.75),
                    Flags = Xamarin.Essentials.BrowserLaunchFlags.PresentAsFormSheet,
                }).Row(Row.Button),
                new IsAuthenticatingIndicator().Row(Row.ActivityIndicator)
            }
        };

        void HandleAuthorizeSessionCompleted(object sender, AuthorizeSessionCompletedEventArgs e)
        {
            if (e.IsSessionAuthorized)
                MainThread.BeginInvokeOnMainThread(() => Navigation.PopModalAsync());
        }

        class IsAuthenticatingIndicator : ActivityIndicator
        {
            public IsAuthenticatingIndicator()
            {
                Color = Color.White;

                AutomationId = OnboardingAutomationIds.IsAuthenticatingActivityIndicator;

                this.SetBinding(IsVisibleProperty, nameof(GitHubAuthenticationViewModel.IsAuthenticating));
                this.SetBinding(IsRunningProperty, nameof(GitHubAuthenticationViewModel.IsAuthenticating));
            }
        }
    }
}
