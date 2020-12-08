using GitTrends.Mobile.Common;
using GitTrends.Shared;
using Xamarin.CommunityToolkit.Markup;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;

namespace GitTrends
{
    class StarsTrendsPage : BaseTrendsContentPage
    {
        public StarsTrendsPage(IAnalyticsService analyticsService, IMainThread mainThread)
            : base((Color)Application.Current.Resources[nameof(BaseTheme.CardStarsStatsIconColor)], mainThread, 1, analyticsService)
        {

        }

        protected override Layout CreateHeaderView() => new StarsStatisticsGrid();
        protected override BaseChartView CreateChartView() => new StarsChart(MainThread);
        protected override EmptyDataView CreateEmptyDataView() => new EmptyDataView(TrendsPageAutomationIds.StarsEmptyDataView)
                                                                        .Bind(IsVisibleProperty, nameof(TrendsViewModel.IsStarsEmptyDataViewVisible))
                                                                        .Bind(EmptyDataView.TitleProperty, nameof(TrendsViewModel.StarsEmptyDataViewTitleText))
                                                                        .Bind(EmptyDataView.ImageSourceProperty, nameof(TrendsViewModel.StarsEmptyDataViewImage))
                                                                        .Bind(EmptyDataView.DescriptionProperty, nameof(TrendsViewModel.StarsEmptyDataViewDescriptionText));
    }
}
