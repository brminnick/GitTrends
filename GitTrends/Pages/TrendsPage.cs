using GitTrends.Shared;
using Syncfusion.SfChart.XForms;
using Xamarin.Forms;

namespace GitTrends
{
    class TrendsPage : BaseContentPage<TrendsViewModel>
    {
        readonly string _owner, _repository;

        public TrendsPage(string owner, string repository) : base("Trends")
        {
            _owner = owner;
            _repository = repository;

            var activityIndicator = new ActivityIndicator { Color = ColorConstants.ActivityIndicatorColor };
            activityIndicator.SetBinding(IsVisibleProperty, nameof(ViewModel.IsFetchingData));
            activityIndicator.SetBinding(ActivityIndicator.IsRunningProperty, nameof(ViewModel.IsFetchingData));

            var totalViewsSeries = new TrendsAreaSeries("Views", nameof(DailyViewsModel.LocalDay), nameof(DailyViewsModel.TotalViews));
            totalViewsSeries.SetBinding(ChartSeries.ItemsSourceProperty, nameof(ViewModel.DailyViewsList));

            var totalUniqueViewsSeries = new TrendsAreaSeries("Unique Views", nameof(DailyViewsModel.LocalDay), nameof(DailyViewsModel.TotalUniqueViews));
            totalUniqueViewsSeries.SetBinding(ChartSeries.ItemsSourceProperty, nameof(ViewModel.DailyViewsList));

            var totalClonesSeries = new TrendsAreaSeries("Clones", nameof(DailyClonesModel.LocalDay), nameof(DailyClonesModel.TotalClones));
            totalClonesSeries.SetBinding(ChartSeries.ItemsSourceProperty, nameof(ViewModel.DailyClonesList));

            var totalUniqueClonesSeries = new TrendsAreaSeries("Unique Clones", nameof(DailyClonesModel.LocalDay), nameof(DailyClonesModel.TotalUniqueClones));
            totalUniqueClonesSeries.SetBinding(ChartSeries.ItemsSourceProperty, nameof(ViewModel.DailyClonesList));

            var clonesChart = new SfChart
            {
                ChartBehaviors = { new ChartZoomPanBehavior() },
                Margin = new Thickness(10),
                Series = { totalViewsSeries, totalUniqueViewsSeries, totalClonesSeries, totalUniqueClonesSeries },
                Title = new ChartTitle { Text = repository },
                PrimaryAxis = new DateTimeAxis(),
                SecondaryAxis = new NumericalAxis(),
                Legend = new ChartLegend { DockPosition = LegendPlacement.Bottom, ToggleSeriesVisibility = true, IconWidth = 8, IconHeight = 8 }
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
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            ViewModel.FetchDataCommand?.Execute((_owner, _repository));
        }

        class TrendsAreaSeries : AreaSeries
        {
            public TrendsAreaSeries(string title, string xDataTitle, string yDataTitle)
            {
                Opacity = 0.5;
                Label = title;
                XBindingPath = xDataTitle;
                YBindingPath = yDataTitle;
                LegendIcon = ChartLegendIcon.SeriesType;
            }
        }
    }
}
