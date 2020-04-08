using System;
using System.Threading.Tasks;

namespace GitTrends
{
    public class WelcomeViewModel : GitHubAuthenticationViewModel
    {
        public WelcomeViewModel(GitHubAuthenticationService gitHubAuthenticationService,
                                                    DeepLinkingService deepLinkingService,
                                                    AnalyticsService analyticsService)
            : base(gitHubAuthenticationService, deepLinkingService, analyticsService)
        {
        }

        protected override async Task ExecuteDemoButtonCommand(string buttonText)
        {
            await base.ExecuteDemoButtonCommand(buttonText).ConfigureAwait(false);

            await GitHubAuthenticationService.ActivateDemoUser().ConfigureAwait(false);
        }
    }
}
