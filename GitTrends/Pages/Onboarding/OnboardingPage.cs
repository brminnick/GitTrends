using GitTrends.Mobile.Common;
using GitTrends.Shared;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace GitTrends;

public class OnboardingPage : BaseCarouselViewPage<OnboardingViewModel>
{
	public OnboardingPage(
		IDeviceInfo deviceInfo,
		IDispatcher dispatcher,
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
		Content.ItemTemplate = new OnboardingCarouselDataTemplateSelector(deviceInfo, dispatcher, analyticsService);
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