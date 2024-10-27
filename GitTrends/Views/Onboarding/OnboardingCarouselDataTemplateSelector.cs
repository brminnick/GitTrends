namespace GitTrends;

public class OnboardingCarouselDataTemplateSelector(
	ChartOnboardingView chartOnboardingView,
	GitTrendsOnboardingView gitTrendsOnboardingView,
	NotificationsOnboardingView notificationsOnboardingView,
	ConnectToGitHubOnboardingView connectToGitHubOnboardingView) : DataTemplateSelector
{
	readonly DataTemplate _chartOnboardingView = chartOnboardingView;
	readonly DataTemplate _gitTrendsOnboardingView = gitTrendsOnboardingView;
	readonly DataTemplate _notificationsOnboardingView = notificationsOnboardingView;
	readonly DataTemplate _connectToGitHubOnboardingView = connectToGitHubOnboardingView;

	protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
	{
		var pageNumber = (int)item;

		return pageNumber switch
		{
			0 => _gitTrendsOnboardingView,
			1 => _chartOnboardingView,
			2 => _notificationsOnboardingView,
			3 => _connectToGitHubOnboardingView,
			_ => throw new NotImplementedException()
		};
	}
}