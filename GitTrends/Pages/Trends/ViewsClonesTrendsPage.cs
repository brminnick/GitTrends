using GitTrends.Mobile.Common;
using GitTrends.Shared;
using Xamarin.CommunityToolkit.Markup;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;

namespace GitTrends
{
	class ViewsClonesTrendsPage : BaseTrendsContentPage
	{
		public ViewsClonesTrendsPage(IMainThread mainThread, IAnalyticsService analyticsService)
			: base((Color)Application.Current.Resources[nameof(BaseTheme.CardViewsStatsIconColor)], mainThread, 0, analyticsService)
		{

		}

		protected override Layout CreateHeaderView() => new ViewsClonesStatisticsGrid();
		protected override BaseChartView CreateChartView() => new ViewsClonesChart(MainThread);
		protected override EmptyDataView CreateEmptyDataView() => new EmptyDataView(TrendsPageAutomationIds.ViewsClonesEmptyDataView)
																		.Bind(IsVisibleProperty, nameof(TrendsViewModel.IsViewsClonesEmptyDataViewVisible))
																		.Bind(EmptyDataView.TitleProperty, nameof(TrendsViewModel.ViewsClonesEmptyDataViewTitleText))
																		.Bind(EmptyDataView.ImageSourceProperty, nameof(TrendsViewModel.ViewsClonesEmptyDataViewImage));
	}
}