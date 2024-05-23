using GitTrends.Shared;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace GitTrends;

public class OnboardingCarouselPage : BaseCarouselPage<OnboardingViewModel>
{
	public OnboardingCarouselPage(
		IAnalyticsService analyticsService,
		OnboardingViewModel onboardingViewModel,
		ChartOnboardingPage chartOnboardingPage,
		GitTrendsOnboardingPage welcomeOnboardingPage,
		NotificationsOnboardingPage notificationsOnboardingPage,
		ConnectToGitHubOnboardingPage connectToGitHubOnboardingPage) : base(onboardingViewModel, analyticsService)
	{
		On<iOS>().SetModalPresentationStyle(UIModalPresentationStyle.OverFullScreen);

		OnboardingViewModel.SkipButtonTapped += HandleSkipButtonTapped;
		GitHubAuthenticationService.DemoUserActivated += HandleDemoUserActivated;

		Children.Add(welcomeOnboardingPage);
		Children.Add(chartOnboardingPage);
		Children.Add(notificationsOnboardingPage);
		Children.Add(connectToGitHubOnboardingPage);
	}

	//Disable the Hardware Back Button on Android
	protected override bool OnBackButtonPressed() => false;

	void HandleSkipButtonTapped(object? sender, EventArgs e)
	{
		AnalyticsService.Track("Skip Button Tapped");

		Dispatcher.Dispatch(() => CurrentPage = Children.Last());
	}

	void HandleDemoUserActivated(object? sender, EventArgs e) => Dispatcher.Dispatch(() => Navigation.PopModalAsync());
}