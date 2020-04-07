using GitTrends.Mobile.Shared;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Markup;
using Xamarin.Forms.PancakeView;
using static GitTrends.XamarinFormsService;
using static Xamarin.Forms.Markup.GridRowsColumns;

namespace GitTrends
{
    public class ConnectToGitHubOnboardingPage : BaseOnboardingContentPage
    {
        public ConnectToGitHubOnboardingPage(GitHubAuthenticationService gitHubAuthenticationService)
                : base(CoralBackgroundColorHex, OnboardingConstants.TryDemoText, 3)
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
                new ConnectToGitHubView().Row(Row.Button),
                new IsAuthenticatingIndicator().Row(Row.ActivityIndicator)
            }
        };

        void HandleAuthorizeSessionCompleted(object sender, AuthorizeSessionCompletedEventArgs e)
        {
            if (e.IsSessionAuthorized)
                MainThread.BeginInvokeOnMainThread(() => Navigation.PopModalAsync());
        }

        class ConnectToGitHubView : PancakeView
        {
            public ConnectToGitHubView()
            {
                AutomationId = OnboardingAutomationIds.ConnectToGitHubButton;
                HorizontalOptions = LayoutOptions.CenterAndExpand;
                VerticalOptions = LayoutOptions.CenterAndExpand;
                Padding = new Thickness(16, 10);
                CornerRadius = 4;

                Content = new StackLayout
                {
                    Orientation = StackOrientation.Horizontal,
                    Spacing = 16,
                    Children =
                    {
                        new GitHubSvgImage(),
                        new ConnectToGitHubLabel()
                    }
                };

                BackgroundColor = Color.FromHex("#231F20");
                //SetDynamicResource(BackgroundColorProperty, nameof(BaseTheme.NavigationBarBackgroundColor));

                this.BindTapGesture(nameof(OnboardingViewModel.ConnectToGitHubButtonCommand));
            }

            class GitHubSvgImage : SvgImage
            {
                public GitHubSvgImage() : base("github.svg", () => Color.White, 24, 24)
                {
                }
            }

            class ConnectToGitHubLabel : Label
            {
                public ConnectToGitHubLabel()
                {
                    HorizontalTextAlignment = TextAlignment.Center;
                    VerticalTextAlignment = TextAlignment.Center;
                    VerticalOptions = LayoutOptions.CenterAndExpand;
                    TextColor = Color.White;
                    FontSize = 18;
                    FontFamily = FontFamilyConstants.RobotoRegular;
                    Text = "Connect to GitHub";
                }
            }
        }

        class IsAuthenticatingIndicator : ActivityIndicator
        {
            public IsAuthenticatingIndicator()
            {
                Color = Color.White;
                this.SetBinding(IsVisibleProperty, nameof(GitHubAuthenticationViewModel.IsAuthenticating));
                this.SetBinding(IsRunningProperty, nameof(GitHubAuthenticationViewModel.IsAuthenticating));
            }
        }
    }
}
