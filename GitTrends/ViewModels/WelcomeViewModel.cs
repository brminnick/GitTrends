using System.Threading.Tasks;
using GitTrends.Shared;
using Xamarin.Essentials.Interfaces;

namespace GitTrends
{
    public class WelcomeViewModel : GitHubAuthenticationViewModel
    {
        public WelcomeViewModel(GitHubAuthenticationService gitHubAuthenticationService,
                                    DeepLinkingService deepLinkingService,
                                    IAnalyticsService analyticsService,
                                    IMainThread mainThread,
                                    GitHubUserService gitHubUserService)
            : base(gitHubAuthenticationService, deepLinkingService, analyticsService, mainThread, gitHubUserService)
        {
        }

        protected override async Task ExecuteDemoButtonCommand(string? buttonText)
        {
            await base.ExecuteDemoButtonCommand(buttonText).ConfigureAwait(false);

            await GitHubAuthenticationService.ActivateDemoUser().ConfigureAwait(false);
        }
    }
}
