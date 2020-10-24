using GitTrends.Mobile.Common;
using GitTrends.Shared;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;

namespace GitTrends
{
    class StarsTrendsPage : BaseTrendsContentPage
    {
        public StarsTrendsPage(IAnalyticsService analyticsService, IMainThread mainThread)
            : base(mainThread, new EmptyDataView("EmptyTrafficChart", TrendsPageAutomationIds.EmptyStarsDataView), 1, analyticsService)
        {

        }

        protected override BaseChartView CreateChartView() => new StarsChart(MainThread);
        protected override Layout CreateStatisticsLayout() => new StarsStatisticsGrid();
    }
}
