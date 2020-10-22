using GitTrends.Shared;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;

namespace GitTrends
{
    class ViewsClonesTrendsPage : BaseTrendsContentPage
    {
        public ViewsClonesTrendsPage(IMainThread mainThread, IAnalyticsService analyticsService) : base(mainThread, 0, analyticsService)
        {

        }

        protected override BaseChartView CreateChartView() => new ViewClonesChart(MainThread);
        protected override Layout CreateStatisticsLayout() => new ViewsClonesStatisticsGrid();
    }
}
