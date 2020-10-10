using GitTrends.Shared;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;

namespace GitTrends
{
    class ViewsClonesTrendsPage : BaseTrendsContentPage
    {
        public ViewsClonesTrendsPage(IMainThread mainThread, IAnalyticsService analyticsService) : base(mainThread, 1, analyticsService)
        {

        }

        protected override BaseChartView CreateChartView() => new ViewClonesChart();
        protected override Layout CreateStatisticsLayout() => new ViewsClonesStatisticsGrid();
    }
}
