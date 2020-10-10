using System.Collections.Generic;
using GitTrends.Mobile.Common;
using GitTrends.Shared;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;
using Xamarin.Forms.Markup;
using static GitTrends.MarkupExtensions;
using static Xamarin.Forms.Markup.GridRowsColumns;

namespace GitTrends
{
    class BaseTrendsContentPage : BaseContentPage
    {
        public BaseTrendsContentPage(in IReadOnlyList<View> gridChildren, in IAnalyticsService analyticsService, in IMainThread mainThread)
            : base(analyticsService, mainThread, true)
        {
            var grid = new Grid
            {
                ColumnSpacing = 8,
                RowSpacing = 12,
                Padding = new Thickness(0, 16),

                RowDefinitions = Rows.Define(
                    (Row.Statistics, AbsoluteGridLength(ViewsClonesStatisticsGrid.StatisticsGridHeight)),
                    (Row.Chart, Star)),
            };

            foreach (var view in gridChildren)
            {
                grid.Children.Add(view);
            }

            grid.Children.Add(new TrendsChartActivityIndicator().Row(Row.Chart));

            Content = grid;
        }

        protected enum Row { Statistics, Chart }

        class TrendsChartActivityIndicator : ActivityIndicator
        {
            public TrendsChartActivityIndicator()
            {
                //The size of UIActivityIndicator is fixed by iOS, so we'll use Xamarin.Forms.VisualElement.Scale to increase its size
                //https://stackoverflow.com/a/2638224/5953643
                if (Device.RuntimePlatform is Device.iOS)
                    Scale = 2;

                AutomationId = TrendsPageAutomationIds.ActivityIndicator;

                this.CenterExpand()
                    .DynamicResource(ColorProperty, nameof(BaseTheme.ActivityIndicatorColor))
                    .Bind(IsVisibleProperty, nameof(TrendsViewModel.IsFetchingData))
                    .Bind(IsRunningProperty, nameof(TrendsViewModel.IsFetchingData));
            }
        }
    }
}
