using System;
using GitTrends.Shared;
using Syncfusion.SfChart.XForms;
using Xamarin.Forms;

namespace GitTrends
{
    class TrendsPage : BaseContentPage<TrendsViewModel>
    {
        readonly string _owner, _repository;
        static readonly Lazy<GitHubTrendsChart> _trendsChartHolder = new Lazy<GitHubTrendsChart>();

        public TrendsPage(TrendsViewModel trendsViewModel, TrendsChartSettingsService trendsChartSettingsService, Repository repository) : base(repository.Name, trendsViewModel)
        {
            _owner = repository.OwnerLogin;
            _repository = repository.Name;

            TrendsChart.TotalViewsSeries.IsVisible = trendsChartSettingsService.ShouldShowViewsByDefault;
            TrendsChart.TotalUniqueViewsSeries.IsVisible = trendsChartSettingsService.ShouldShowUniqueViewsByDefault;
            TrendsChart.TotalClonesSeries.IsVisible = trendsChartSettingsService.ShouldShowClonesByDefault;
            TrendsChart.TotalUniqueClonesSeries.IsVisible = trendsChartSettingsService.ShouldShowUniqueClonesByDefault;

            var activityIndicator = new ActivityIndicator { Color = ColorConstants.DarkBlue };
            activityIndicator.SetBinding(IsVisibleProperty, nameof(TrendsViewModel.IsFetchingData));
            activityIndicator.SetBinding(ActivityIndicator.IsRunningProperty, nameof(TrendsViewModel.IsFetchingData));

            var absoluteLayout = new AbsoluteLayout();
            absoluteLayout.Children.Add(activityIndicator, new Rectangle(.5, .5, -1, -1), AbsoluteLayoutFlags.PositionProportional);
            absoluteLayout.Children.Add(TrendsChart, new Rectangle(0, 0, 1, 1), AbsoluteLayoutFlags.All);

            Content = absoluteLayout;
        }

        static GitHubTrendsChart TrendsChart => _trendsChartHolder.Value;

        protected override void OnAppearing()
        {
            base.OnAppearing();

            ViewModel.FetchDataCommand.Execute((_owner, _repository));
        }

        class GitHubTrendsChart : SfChart
        {
            public GitHubTrendsChart()
            {
                TotalViewsSeries = new TrendsAreaSeries("Views", nameof(DailyViewsModel.LocalDay), nameof(DailyViewsModel.TotalViews), ColorConstants.DarkestBlue);
                TotalViewsSeries.SetBinding(ChartSeries.ItemsSourceProperty, nameof(TrendsViewModel.DailyViewsList));

                TotalUniqueViewsSeries = new TrendsAreaSeries("Unique Views", nameof(DailyViewsModel.LocalDay), nameof(DailyViewsModel.TotalUniqueViews), ColorConstants.MediumBlue);
                TotalUniqueViewsSeries.SetBinding(ChartSeries.ItemsSourceProperty, nameof(TrendsViewModel.DailyViewsList));

                TotalClonesSeries = new TrendsAreaSeries("Clones", nameof(DailyClonesModel.LocalDay), nameof(DailyClonesModel.TotalClones), ColorConstants.DarkNavyBlue);
                TotalClonesSeries.SetBinding(ChartSeries.ItemsSourceProperty, nameof(TrendsViewModel.DailyClonesList));

                TotalUniqueClonesSeries = new TrendsAreaSeries("Unique Clones", nameof(DailyClonesModel.LocalDay), nameof(DailyClonesModel.TotalUniqueClones), ColorConstants.LightNavyBlue);
                TotalUniqueClonesSeries.SetBinding(ChartSeries.ItemsSourceProperty, nameof(TrendsViewModel.DailyClonesList));

                this.SetBinding(IsVisibleProperty, nameof(TrendsViewModel.IsChartVisible));

                ChartBehaviors = new ChartBehaviorCollection
                {
                    new ChartZoomPanBehavior(),
                    new ChartTrackballBehavior()
                };

                Series = new ChartSeriesCollection
                {
                    TotalViewsSeries,
                    TotalUniqueViewsSeries,
                    TotalClonesSeries,
                    TotalUniqueClonesSeries
                };

                Legend = new ChartLegend
                {
                    DockPosition = LegendPlacement.Bottom,
                    ToggleSeriesVisibility = true,
                    IconWidth = 20,
                    IconHeight = 20,
                    LabelStyle = new ChartLegendLabelStyle { TextColor = ColorConstants.DarkNavyBlue }
                };

                var axisLabelStyle = new ChartAxisLabelStyle
                {
                    TextColor = ColorConstants.DarkNavyBlue,
                    FontSize = 14
                };
                var axisLineStyle = new ChartLineStyle { StrokeColor = ColorConstants.LightNavyBlue };

                PrimaryAxis = new DateTimeAxis
                {
                    IntervalType = DateTimeIntervalType.Days,
                    Interval = 1,
                    RangePadding = DateTimeRangePadding.Round,
                    LabelStyle = axisLabelStyle,
                    AxisLineStyle = axisLineStyle,
                    MajorTickStyle = new ChartAxisTickStyle { StrokeColor = Color.Transparent },
                    ShowMajorGridLines = false
                };
                PrimaryAxis.SetBinding(DateTimeAxis.MinimumProperty, nameof(TrendsViewModel.MinDateValue));
                PrimaryAxis.SetBinding(DateTimeAxis.MaximumProperty, nameof(TrendsViewModel.MaxDateValue));

                SecondaryAxis = new NumericalAxis
                {
                    LabelStyle = axisLabelStyle,
                    AxisLineStyle = axisLineStyle,
                    MajorTickStyle = new ChartAxisTickStyle { StrokeColor = ColorConstants.LightNavyBlue },
                    ShowMajorGridLines = false
                };
                SecondaryAxis.SetBinding(NumericalAxis.MinimumProperty, nameof(TrendsViewModel.DailyViewsClonesMinValue));
                SecondaryAxis.SetBinding(NumericalAxis.MaximumProperty, nameof(TrendsViewModel.DailyViewsClonesMaxValue));

                BackgroundColor = Color.Transparent;

                ChartPadding = 0;
                Margin = Device.RuntimePlatform is Device.iOS ? new Thickness(0, 5, 0, 15) : new Thickness(0, 5, 0, 0);
            }

            public AreaSeries TotalViewsSeries { get; }
            public AreaSeries TotalUniqueViewsSeries { get; }
            public AreaSeries TotalClonesSeries { get; }
            public AreaSeries TotalUniqueClonesSeries { get; }

            class TrendsAreaSeries : AreaSeries
            {
                public TrendsAreaSeries(in string title, in string xDataTitle, in string yDataTitle, in Color color)
                {
                    Opacity = 0.9;
                    Label = title;
                    XBindingPath = xDataTitle;
                    YBindingPath = yDataTitle;
                    LegendIcon = ChartLegendIcon.SeriesType;
                    Color = color;
                }
            }
        }
    }
}
