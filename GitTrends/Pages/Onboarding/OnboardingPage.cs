using GitTrends.Shared;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace GitTrends;

public class OnboardingPage : BaseCarouselViewPage<OnboardingViewModel>
{
	public OnboardingPage(
		IAnalyticsService analyticsService,
		OnboardingViewModel onboardingViewModel,
		ChartOnboardingView chartOnboardingView,
		GitTrendsOnboardingView welcomeOnboardingView,
		NotificationsOnboardingView notificationsOnboardingView,
		ConnectToGitHubOnboardingView connectToGitHubOnboardingView) : base(onboardingViewModel, analyticsService)
	{
		Shell.SetPresentationMode(this, PresentationMode.ModalAnimated);
		
		On<iOS>().SetModalPresentationStyle(UIModalPresentationStyle.OverFullScreen);

		OnboardingViewModel.SkipButtonTapped += HandleSkipButtonTapped;
		GitHubAuthenticationService.DemoUserActivated += HandleDemoUserActivated;

		Children.Add(welcomeOnboardingView);
		Children.Add(chartOnboardingView);
		Children.Add(notificationsOnboardingView);
		Children.Add(connectToGitHubOnboardingView);
	}

	//Disable the Hardware Back Button on Android
	protected override bool OnBackButtonPressed() => false;

	async void HandleSkipButtonTapped(object? sender, EventArgs e)
	{
		AnalyticsService.Track("Skip Button Tapped");

		await Dispatcher.DispatchAsync(() => Content.Position = Children.Count - 1);
	}

	void HandleDemoUserActivated(object? sender, EventArgs e) => Dispatcher.Dispatch(() => Shell.Current.GoToAsync(".."));
}