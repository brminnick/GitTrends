using GitTrends.Mobile.Common;
using GitTrends.Shared;
using CommunityToolkit.Maui.Markup;
using GitTrends.Resources;

namespace GitTrends;

class ViewsClonesTrendsPage(IDeviceInfo deviceInfo, IAnalyticsService analyticsService) 
	: BaseTrendsContentPage(AppResources.GetResource<Color>(nameof(BaseTheme.CardViewsStatsIconColor)), deviceInfo, 0, TrendsPageType.ViewsClonesTrendsPage, analyticsService)
{

	protected override Layout CreateHeaderView() => new ViewsClonesStatisticsGrid();
	
	protected override BaseChartView CreateChartView() => new ViewsClonesChart();
	
	protected override EmptyDataView CreateEmptyDataView() => new EmptyDataView(TrendsPageAutomationIds.ViewsClonesEmptyDataView)
		.Bind(IsVisibleProperty, nameof(TrendsViewModel.IsViewsClonesEmptyDataViewVisible))
		.Bind(EmptyDataView.TitleProperty, nameof(TrendsViewModel.ViewsClonesEmptyDataViewTitleText))
		.Bind(EmptyDataView.ImageSourceProperty, nameof(TrendsViewModel.ViewsClonesEmptyDataViewImage));
}