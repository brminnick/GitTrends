using GitTrends.Mobile.Common;
using GitTrends.Shared;
using CommunityToolkit.Maui.Markup;
using GitTrends.Resources;

namespace GitTrends;

class StarsTrendsView(IDeviceInfo deviceInfo, IAnalyticsService analyticsService) 
	: BaseTrendsContentView(AppResources.GetResource<Color>(nameof(BaseTheme.CardStarsStatsIconColor)), deviceInfo, 1, TrendsPageType.StarsTrendsPage, analyticsService)
{
	protected override Layout CreateHeaderView() => new StarsStatisticsGrid();
	
	protected override BaseChartView CreateChartView() => new StarsChart();
	
	protected override EmptyDataView CreateEmptyDataView() => new EmptyDataView(TrendsPageAutomationIds.StarsEmptyDataView)
		.Bind(IsVisibleProperty, nameof(TrendsViewModel.IsStarsEmptyDataViewVisible))
		.Bind(EmptyDataView.TitleProperty, nameof(TrendsViewModel.StarsEmptyDataViewTitleText))
		.Bind(EmptyDataView.ImageSourceProperty, nameof(TrendsViewModel.StarsEmptyDataViewImage))
		.Bind(EmptyDataView.DescriptionProperty, nameof(TrendsViewModel.StarsEmptyDataViewDescriptionText));
}