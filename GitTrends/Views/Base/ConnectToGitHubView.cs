using System.Threading;
using GitTrends.Mobile.Common.Constants;
using Xamarin.Forms;
using Xamarin.Forms.Markup;
using Xamarin.Forms.PancakeView;

namespace GitTrends
{
    class ConnectToGitHubView : PancakeView
    {
        public ConnectToGitHubView(in string automationId, CancellationToken cancellationToken, Xamarin.Essentials.BrowserLaunchOptions? browserLaunchOptions = null)
        {
            AutomationId = automationId;
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

            BackgroundColor = Color.FromHex("231F20");

            GestureRecognizers.Add(new TapGestureRecognizer { CommandParameter = (cancellationToken, browserLaunchOptions) }
                                    .Bind(TapGestureRecognizer.CommandProperty, nameof(OnboardingViewModel.ConnectToGitHubButtonCommand)));
        }

        class GitHubSvgImage : SvgImage
        {
            public GitHubSvgImage() : base("github.svg", () => Color.White)
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
                Text = GitHubLoginButtonConstants.ConnectToGitHub;
            }
        }
    }
}
