﻿using GitTrends.Mobile.Common;
using GitTrends.Shared;
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

	//Disable the Hardware Back Button on Android
	protected override bool OnBackButtonPressed() => false;

	void HandleSkipButtonTapped(object? sender, EventArgs e)
	{
		AnalyticsService.Track("Skip Button Tapped");

		Content.ScrollTo(PageCount - 1);
	}

	void HandleDemoUserActivated(object? sender, EventArgs e) => Dispatcher.Dispatch(() => Shell.Current.GoToAsync(".."));
}