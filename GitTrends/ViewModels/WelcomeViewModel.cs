using System.Threading.Tasks;
using GitTrends.Shared;
using Xamarin.Essentials.Interfaces;

namespace GitTrends
{
    public class WelcomeViewModel : GitHubAuthenticationViewModel
    {
        public WelcomeViewModel(IMainThread mainThread,
                                    IAnalyticsService analyticsService,
                                    GitHubUserService gitHubUserService,
                                    DeepLinkingService deepLinkingService,
                                    GitHubAuthenticationService gitHubAuthenticationService)
            : base(mainThread, analyticsService, gitHubUserService, deepLinkingService, gitHubAuthenticationService)
        {
        }

        protected override async Task ExecuteDemoButtonCommand(string? buttonText)
        {
            await base.ExecuteDemoButtonCommand(buttonText).ConfigureAwait(false);

            await GitHubAuthenticationService.ActivateDemoUser().ConfigureAwait(false);
        }
    }
}
