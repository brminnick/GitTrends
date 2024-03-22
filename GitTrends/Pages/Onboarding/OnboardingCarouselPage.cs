using System;
using System.Linq;
using GitTrends.Shared;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace GitTrends;

public class OnboardingCarouselPage : BaseCarouselPage<OnboardingViewModel>
{
	public OnboardingCarouselPage(IMainThread mainThread,
									IAnalyticsService analyticsService,
									OnboardingViewModel onboardingViewModel,
									ChartOnboardingPage chartOnboardingPage,
									GitTrendsOnboardingPage welcomeOnboardingPage,
									NotificationsOnboardingPage notificationsOnboardingPage,
									ConnectToGitHubOnboardingPage connectToGitHubOnboardingPage) : base(onboardingViewModel, mainThread, analyticsService)
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

	void HandleSkipButtonTapped(object sender, EventArgs e)
	{
		AnalyticsService.Track("Skip Button Tapped");

		MainThread.BeginInvokeOnMainThread(() => CurrentPage = Children.Last());
	}

	void HandleDemoUserActivated(object sender, EventArgs e) => MainThread.BeginInvokeOnMainThread(() => Navigation.PopModalAsync());
}