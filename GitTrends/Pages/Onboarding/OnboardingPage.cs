using GitTrends.Common;
using GitTrends.Mobile.Common;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace GitTrends;

public class OnboardingPage : BaseCarouselViewPage<OnboardingViewModel>
{
	public OnboardingPage(
		ChartOnboardingView chartOnboardingView,
		GitTrendsOnboardingView gitTrendsOnboardingView,
		NotificationsOnboardingView notificationsOnboardingView,
		ConnectToGitHubOnboardingView connectToGitHubOnboardingView,
		IAnalyticsService analyticsService,
		OnboardingViewModel onboardingViewModel) : base(onboardingViewModel, analyticsService)
	{
		//Don't Use BaseTheme.PageBackgroundColor
		RemoveDynamicResource(BackgroundColorProperty);

		AutomationId = OnboardingAutomationIds.OnboardingPage;

		Shell.SetPresentationMode(this, PresentationMode.ModalAnimated);

		On<iOS>().SetModalPresentationStyle(UIModalPresentationStyle.OverFullScreen);

		OnboardingViewModel.SkipButtonTapped += HandleSkipButtonTapped;
		GitHubAuthenticationService.DemoUserActivated += HandleDemoUserActivated;

		Content.ItemsSource = Enumerable.Range(0, 4);
		Content.ItemTemplate = new OnboardingCarouselDataTemplateSelector(chartOnboardingView, gitTrendsOnboardingView, notificationsOnboardingView, connectToGitHubOnboardingView);
	}

	// Disable the Hardware Back Button on Android to prevent the user from dismissing the Modal Page
	protected override bool OnBackButtonPressed() => false;

	void HandleSkipButtonTapped(object? sender, EventArgs e)
	{
		AnalyticsService.Track("Skip Button Tapped");

		// Animated scrolling is preferred but is broken on iOS
#if IOS || MACCATALYST
		Content.ScrollTo(PageCount - 1, animate: false);
#else
		Content.ScrollTo(PageCount - 1);
#endif
	}

	void HandleDemoUserActivated(object? sender, EventArgs e) => Dispatcher.Dispatch(() => Shell.Current.GoToAsync(".."));
}