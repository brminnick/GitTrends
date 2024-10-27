using GitTrends.Common;

namespace GitTrends;

public class WelcomeViewModel(IDispatcher dispatcher,
								IAnalyticsService analyticsService,
								GitHubUserService gitHubUserService,
								DeepLinkingService deepLinkingService,
								GitHubAuthenticationService gitHubAuthenticationService) : GitHubAuthenticationViewModel(dispatcher, analyticsService, gitHubUserService, deepLinkingService, gitHubAuthenticationService)
{
	protected override async Task HandleDemoButtonTapped(string? buttonText, CancellationToken token)
	{
		await base.HandleDemoButtonTapped(buttonText, token).ConfigureAwait(false);

		await GitHubAuthenticationService.ActivateDemoUser(token).ConfigureAwait(false);
	}
}