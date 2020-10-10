using System.Collections.Generic;
using GitTrends.Mobile.Common;
using GitTrends.Shared;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;
using Xamarin.Forms.Markup;

namespace GitTrends
{
    class StarsTrendsPage : BaseTrendsContentPage
    {
        public StarsTrendsPage(IAnalyticsService analyticsService, IMainThread mainThread) : base(CreateViews(), analyticsService, mainThread)
        {

        }

        static IReadOnlyList<View> CreateViews() => new View[]
        {
            new StarsStatisticsGrid()
                .Row(Row.Statistics),

            new StarsChart()
                .Row(Row.Chart),

            new EmptyDataView("EmptyInsightsChart", TrendsPageAutomationIds.EmptyDataView)
                .Row(Row.Chart)
                .Bind(IsVisibleProperty, nameof(TrendsViewModel.IsEmptyDataViewVisible))
                .Bind(EmptyDataView.TitleProperty, nameof(TrendsViewModel.EmptyDataViewTitle)),
        };
    }
}
