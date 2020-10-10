using System.Collections.Generic;
using GitTrends.Mobile.Common;
using GitTrends.Shared;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;
using Xamarin.Forms.Markup;

namespace GitTrends
{
    class ViewsClonesTrendsPage : BaseTrendsContentPage
    {
        public ViewsClonesTrendsPage(IMainThread mainThread, IAnalyticsService analyticsService) : base(CreateViews(), analyticsService, mainThread)
        {

        }

        static IReadOnlyList<View> CreateViews() => new View[]
        {
            new ViewsClonesStatisticsGrid()
                .Row(Row.Statistics),

            new ViewClonesChart()
                .Row(Row.Chart),

            new EmptyDataView("EmptyInsightsChart", TrendsPageAutomationIds.EmptyDataView)
                .Row(Row.Chart)
                .Bind(IsVisibleProperty, nameof(TrendsViewModel.IsEmptyDataViewVisible))
                .Bind(EmptyDataView.TitleProperty, nameof(TrendsViewModel.EmptyDataViewTitle)),
        };
    }
}
