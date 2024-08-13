using GitTrends.Shared;

namespace GitTrends;

public class TrendsCarouselDataTemplateSelector(IDeviceInfo deviceInfo, IAnalyticsService analyticsService) : DataTemplateSelector
{
	readonly DataTemplate _starsTrendsView = new StarsTrendsView(deviceInfo, analyticsService);
	readonly DataTemplate _viewsClonesTrendsView = new ViewsClonesTrendsView(deviceInfo, analyticsService);
	
	protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
	{
		var page = (int)item;

		return page switch
		{
			0 => _viewsClonesTrendsView,
			1 => _starsTrendsView,
			_ => throw new NotImplementedException()
		};
	}
}