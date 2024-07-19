using GitTrends.Shared;
namespace GitTrends;

public class OnboardingCarouselDataTemplateSelector(IDeviceInfo deviceInfo, IDispatcher dispatcher, IAnalyticsService analyticsService) : DataTemplateSelector
{
	readonly DataTemplate _chartOnboardingView = new ChartOnboardingView(deviceInfo, analyticsService);
	readonly DataTemplate _gitTrendsOnboardingView = new GitTrendsOnboardingView(deviceInfo, analyticsService);
	readonly DataTemplate _notificationsOnboardingView = new NotificationsOnboardingView(deviceInfo, analyticsService);
	readonly DataTemplate _connectToGitHubOnboardingView = new ConnectToGitHubOnboardingView(deviceInfo, dispatcher, analyticsService);

	protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
	{
		var page = (int)item;

		return page switch
		{
			0 => _gitTrendsOnboardingView,
			1 => _chartOnboardingView,
			2 => _notificationsOnboardingView,
			3 => _connectToGitHubOnboardingView,
			_ => throw new NotImplementedException()
		};
	}
}