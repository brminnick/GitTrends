using System.Threading;
using GitTrends.Mobile.Common.Constants;
using Xamarin.CommunityToolkit.Markup;
using Xamarin.Forms;
using Xamarin.Forms.PancakeView;

namespace GitTrends
{
    class ConnectToGitHubView : GitHubView
    {
        public ConnectToGitHubView(in string automationId,
                                    CancellationToken cancellationToken,
                                    Xamarin.Essentials.BrowserLaunchOptions? browserLaunchOptions = null)
            : base(GitHubLoginButtonConstants.ConnectToGitHub, automationId, 18, FontFamilyConstants.RobotoRegular)
        {
            GestureRecognizers.Add(new TapGestureRecognizer { CommandParameter = (cancellationToken, browserLaunchOptions) }
                .Bind(TapGestureRecognizer.CommandProperty, nameof(OnboardingViewModel.ConnectToGitHubButtonCommand)));
        }
    }

    class GitHubView : PancakeView
    {
        public GitHubView(in string text, in string automationId, in int fontSize, in string fontFamily)
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
                    new GitHubLabel(text, fontSize,fontFamily)
                }
            };

            BackgroundColor = Color.FromHex("231F20");
        }

        class GitHubSvgImage : SvgImage
        {
            public GitHubSvgImage() : base("github.svg", () => Color.White)
            {
            }
        }

        class GitHubLabel : Label
        {
            public GitHubLabel(in string text, in int fontSize = 18, in string fontFamily = FontFamilyConstants.RobotoRegular)
            {
                Text = text;
                FontSize = fontSize;
                FontFamily = fontFamily;

                HorizontalTextAlignment = TextAlignment.Center;
                VerticalTextAlignment = TextAlignment.Center;
                VerticalOptions = LayoutOptions.CenterAndExpand;
                TextColor = Color.White;
            }
        }
    }
}
