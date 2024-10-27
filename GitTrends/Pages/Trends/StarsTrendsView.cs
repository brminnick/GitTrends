using CommunityToolkit.Maui.Markup;
using GitTrends.Common;
using GitTrends.Mobile.Common;
using GitTrends.Resources;

namespace GitTrends;

class StarsTrendsView(IDeviceInfo deviceInfo, IAnalyticsService analyticsService, TrendsViewModel trendsViewModel)
	: BaseTrendsDataTemplate(
		AppResources.GetResource<Color>(nameof(BaseTheme.CardStarsStatsIconColor)),
		deviceInfo,
		1,
		() => new StarsStatisticsGrid(deviceInfo),
		analyticsService,
		() => new StarsChart(trendsViewModel),
		CreateEmptyDataView,
		TrendsPageType.StarsTrendsPage)
{

	static EmptyDataView CreateEmptyDataView() => new EmptyDataView(TrendsPageAutomationIds.StarsEmptyDataView)
		.Bind(EmptyDataView.IsVisibleProperty,
			nameof(TrendsViewModel.IsStarsEmptyDataViewVisible),
			source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext, typeof(TrendsViewModel)))
		.Bind(EmptyDataView.TitleProperty,
			nameof(TrendsViewModel.StarsEmptyDataViewTitleText),
			source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext, typeof(TrendsViewModel)))
		.Bind(EmptyDataView.ImageSourceProperty,
			nameof(TrendsViewModel.StarsEmptyDataViewImage),
			source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext, typeof(TrendsViewModel)))
		.Bind(EmptyDataView.DescriptionProperty,
			nameof(TrendsViewModel.StarsEmptyDataViewDescriptionText),
			source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext, typeof(TrendsViewModel)));
}