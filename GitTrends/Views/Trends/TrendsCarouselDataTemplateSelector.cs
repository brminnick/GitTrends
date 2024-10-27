using GitTrends.Common;

namespace GitTrends;

class TrendsCarouselDataTemplateSelector(IDeviceInfo deviceInfo, IAnalyticsService analyticsService, TrendsViewModel trendsViewModel) : DataTemplateSelector
{
	readonly DataTemplate _starsTrendsView = new StarsTrendsView(deviceInfo, analyticsService, trendsViewModel);
	readonly DataTemplate _viewsClonesTrendsView = new ViewsClonesTrendsView(deviceInfo, analyticsService, trendsViewModel);

	protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
	{
		var pageNumber = (int)item;

		return pageNumber switch
		{
			0 => _viewsClonesTrendsView,
			1 => _starsTrendsView,
			_ => throw new NotImplementedException()
		};
	}
}