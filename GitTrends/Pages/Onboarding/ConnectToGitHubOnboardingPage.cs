using System.Threading;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using GitTrends.Shared;
using Xamarin.CommunityToolkit.Markup;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;
using static GitTrends.MarkupExtensions;
using static Xamarin.CommunityToolkit.Markup.GridRowsColumns;

namespace GitTrends
{
    public class ConnectToGitHubOnboardingPage : BaseOnboardingContentPage
    {
        public ConnectToGitHubOnboardingPage(IMainThread mainthread, IAnalyticsService analyticsService)
                : base(analyticsService, mainthread, Color.FromHex(BaseTheme.CoralColorHex), OnboardingConstants.TryDemoText, 3)
        {
            GitHubAuthenticationService.AuthorizeSessionCompleted += HandleAuthorizeSessionCompleted;
        }

        enum Row { Description, Button, ActivityIndicator }

        protected override View CreateImageView() => new Image
        {
            Source = "ConnectToGitHubOnboarding",
            HorizontalOptions = LayoutOptions.CenterAndExpand,
            VerticalOptions = LayoutOptions.CenterAndExpand,
            Aspect = Aspect.AspectFit
        };

        protected override TitleLabel CreateDescriptionTitleLabel() => new TitleLabel(OnboardingConstants.ConnectToGitHubPage_Title);

        protected override View CreateDescriptionBodyView() => new ScrollView
        {
            Content = new Grid
            {
                RowSpacing = 16,

                RowDefinitions = Rows.Define(
                        (Row.Description, 65),
                        (Row.Button, 42),
                        (Row.ActivityIndicator, 42)),

                Children =
                {
                    new BodyLabel(OnboardingConstants.ConnectToGitHubPage_Body_GetStarted).Row(Row.Description),
                    new ConnectToGitHubView(OnboardingAutomationIds.ConnectToGitHubButton, CancellationToken.None, new Xamarin.Essentials.BrowserLaunchOptions
                    {
                        PreferredControlColor = Color.White,
                        PreferredToolbarColor = Color.FromHex(BaseTheme.CoralColorHex).MultiplyAlpha(0.75),
                        Flags = Xamarin.Essentials.BrowserLaunchFlags.PresentAsFormSheet,
                    }).Row(Row.Button),
                    new IsAuthenticatingIndicator().Row(Row.ActivityIndicator)
                }
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
