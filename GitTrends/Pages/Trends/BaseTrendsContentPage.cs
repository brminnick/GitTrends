using GitTrends.Mobile.Common;
using GitTrends.Shared;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;
using Xamarin.Forms.Markup;
using static GitTrends.MarkupExtensions;
using static Xamarin.Forms.Markup.GridRowsColumns;

namespace GitTrends
{
    abstract class BaseTrendsContentPage : BaseContentPage
    {
        public BaseTrendsContentPage(in IMainThread mainThread,
                                        in int carouselPositionIndex,
                                        in IAnalyticsService analyticsService) : base(analyticsService, mainThread, false)
        {
            Content = new Grid
            {
                ColumnSpacing = 8,
                RowSpacing = 12,
                Padding = new Thickness(0, 16),

                RowDefinitions = Rows.Define(
                    (Row.Statistics, AbsoluteGridLength(ViewsClonesStatisticsGrid.StatisticsGridHeight)),
                    (Row.Chart, Star),
                    (Row.Indicator, AbsoluteGridLength(44))),

                Children =
                {
                    CreateStatisticsLayout()
                        .Row(Row.Statistics),

                    new EmptyDataView("EmptyInsightsChart", TrendsPageAutomationIds.EmptyDataView)
                        .Row(Row.Chart)
                        .Bind(IsVisibleProperty, nameof(TrendsViewModel.IsEmptyDataViewVisible))
                        .Bind(EmptyDataView.TitleProperty, nameof(TrendsViewModel.EmptyDataViewTitle)),

                    CreateChartView()
                        .Row(Row.Chart),

                    new TrendsChartActivityIndicator()
                        .Row(Row.Chart),

                    new TrendsIndicatorView(carouselPositionIndex).FillExpand()
                        .Row(Row.Indicator)
                }
            };
        }

        protected enum Row { Statistics, Chart, Indicator }

        protected abstract BaseChartView CreateChartView();
        protected abstract Layout CreateStatisticsLayout();

        class TrendsIndicatorView : IndicatorView
        {
            public TrendsIndicatorView(in int position)
            {
                Position = position;

                var selectedIndicatorColor = (Color)Application.Current.Resources[nameof(BaseTheme.NavigationBarBackgroundColor)];

                SelectedIndicatorColor = selectedIndicatorColor;
                IndicatorColor = selectedIndicatorColor.MultiplyAlpha(0.25);
                AutomationId = TrendsPageAutomationIds.IndicatorView;

                SetBinding(CountProperty, new Binding(nameof(TrendsCarouselPage.PageCount),
                                                        source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestor, typeof(TrendsCarouselPage))));
            }
        }

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
