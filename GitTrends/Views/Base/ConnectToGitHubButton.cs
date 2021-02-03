using System.Threading;
using GitTrends.Mobile.Common.Constants;
using Xamarin.CommunityToolkit.Markup;
using Xamarin.Forms;

namespace GitTrends
{
    class ConnectToGitHubButton : SvgTextLabel
    {
        public ConnectToGitHubButton(in string automationId,
                                    CancellationToken cancellationToken,
                                    Xamarin.Essentials.BrowserLaunchOptions? browserLaunchOptions = null)
            : base("github.svg", GitHubLoginButtonConstants.ConnectToGitHub, automationId, 18, FontFamilyConstants.RobotoRegular, 16)
        {
            BackgroundColor = Color.FromHex("231F20");

            GestureRecognizers.Add(new TapGestureRecognizer { CommandParameter = (cancellationToken, browserLaunchOptions) }
                .Bind(TapGestureRecognizer.CommandProperty, nameof(OnboardingViewModel.ConnectToGitHubButtonCommand)));
        }
    }
}
