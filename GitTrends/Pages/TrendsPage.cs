using GitTrends.Shared;
using Syncfusion.SfChart.XForms;
using Xamarin.Forms;

namespace GitTrends
{
    class TrendsPage : BaseContentPage<TrendsViewModel>
    {
        public TrendsPage(string owner, string repository) : base("Trends")
        {
            var activityIndicator = new ActivityIndicator { Color = ColorConstants.ActivityIndicatorColor };
            activityIndicator.SetBinding(IsVisibleProperty, nameof(ViewModel.IsFetchingData));
            activityIndicator.SetBinding(ActivityIndicator.IsRunningProperty, nameof(ViewModel.IsFetchingData));

            var totalViewsSeries = new AreaSeries
            {
                Label = "Total Views",
                XBindingPath = nameof(DailyViewsModel.LocalDay),
                YBindingPath = nameof(DailyViewsModel.TotalViews),
                LegendIcon = ChartLegendIcon.SeriesType
            };
            totalViewsSeries.SetBinding(AreaSeries.ItemsSourceProperty, nameof(ViewModel.DailyViewsList));

            var totalUniqueViewsSeries = new AreaSeries
            {
                Label = "Total Unique Views",
                XBindingPath = nameof(DailyViewsModel.LocalDay),
                YBindingPath = nameof(DailyViewsModel.TotalUniqueViews),
                LegendIcon = ChartLegendIcon.SeriesType
            };
            totalUniqueViewsSeries.SetBinding(AreaSeries.ItemsSourceProperty, nameof(ViewModel.DailyViewsList));

            var clonesChart = new SfChart
            {
                Margin = new Thickness(10),
                Series = { totalViewsSeries, totalUniqueViewsSeries },
                Title = new ChartTitle { Text = repository },
                PrimaryAxis = new DateTimeAxis(),
                SecondaryAxis = new NumericalAxis(),
                Legend = new ChartLegend { DockPosition = LegendPlacement.Bottom, ToggleSeriesVisibility = true, IconWidth = 14, IconHeight = 14 }
            };
            clonesChart.SetBinding(IsVisibleProperty, nameof(ViewModel.IsChartVisible));
            clonesChart.PrimaryAxis.SetBinding(DateTimeAxis.MinimumProperty, nameof(ViewModel.MinDateValue));
            clonesChart.PrimaryAxis.SetBinding(DateTimeAxis.MaximumProperty, nameof(ViewModel.MaxDateValue));
            clonesChart.SecondaryAxis.SetBinding(NumericalAxis.MinimumProperty, nameof(ViewModel.DailyViewsMinValue));
            clonesChart.SecondaryAxis.SetBinding(NumericalAxis.MaximumProperty, nameof(ViewModel.DailyViewsMaxValue));

            var absoluteLayout = new AbsoluteLayout();
            absoluteLayout.Children.Add(activityIndicator, new Rectangle(.5, .5, -1, -1), AbsoluteLayoutFlags.PositionProportional);
            absoluteLayout.Children.Add(clonesChart, new Rectangle(0, 0, 1, 1), AbsoluteLayoutFlags.All);

            Content = absoluteLayout;

            ViewModel.FetchDataCommand?.Execute((owner, repository));
        }
    }
}
