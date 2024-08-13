using GitTrends.Mobile.Common;
using GitTrends.Shared;
using CommunityToolkit.Maui.Markup;
using GitTrends.Resources;

namespace GitTrends;

class ViewsClonesTrendsView(IDeviceInfo deviceInfo, IAnalyticsService analyticsService) 
	: BaseTrendsDataTemplate(
		AppResources.GetResource<Color>(nameof(BaseTheme.CardViewsStatsIconColor)), 
		deviceInfo, 
		0, 
		TrendsPageType.ViewsClonesTrendsPage, 
		analyticsService,
		() => new ViewsClonesStatisticsGrid(deviceInfo),
		() => new ViewsClonesChart(),
		CreateEmptyDataView)
{
	static EmptyDataView CreateEmptyDataView() => new EmptyDataView(TrendsPageAutomationIds.ViewsClonesEmptyDataView)
		.Bind(EmptyDataView.IsVisibleProperty, 
			nameof(TrendsViewModel.IsViewsClonesEmptyDataViewVisible),
			source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext, typeof(TrendsViewModel)))
		.Bind(EmptyDataView.TitleProperty, 
			nameof(TrendsViewModel.ViewsClonesEmptyDataViewTitleText),
			source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext, typeof(TrendsViewModel)))
		.Bind(EmptyDataView.ImageSourceProperty, 
			nameof(TrendsViewModel.ViewsClonesEmptyDataViewImage),
			source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext, typeof(TrendsViewModel)));
}