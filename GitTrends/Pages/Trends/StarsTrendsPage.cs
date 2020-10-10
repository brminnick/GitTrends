using GitTrends.Shared;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;

namespace GitTrends
{
    class StarsTrendsPage : BaseTrendsContentPage
    {
        public StarsTrendsPage(IAnalyticsService analyticsService, IMainThread mainThread) : base(mainThread, 1, analyticsService)
        {

        }

        protected override BaseChartView CreateChartView() => new StarsChart();
        protected override Layout CreateStatisticsLayout() => new StarsStatisticsGrid();
    }
}
