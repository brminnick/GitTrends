using GitTrends.Common;

namespace GitTrends;

class BaseCarouselView : CarouselView
{
	public BaseCarouselView(IAnalyticsService analyticsService)
	{
		AnalyticsService = analyticsService;

		Loop = false;
		ItemsLayout.SnapPointsAlignment = SnapPointsAlignment.Center;
		ItemsLayout.SnapPointsType = SnapPointsType.MandatorySingle;
	}

	protected IAnalyticsService AnalyticsService { get; }
}