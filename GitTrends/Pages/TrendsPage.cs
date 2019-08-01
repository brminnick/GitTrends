using System;
using GitTrends.Shared;
using Syncfusion.SfChart.XForms;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace GitTrends
{
    class TrendsPage : BaseContentPage<TrendsViewModel>
    {
        #region Constant Fields
        readonly string _owner, _repository;
        static readonly Lazy<GitHubTrendsChart> _gitHubTrendsChartHolder = new Lazy<GitHubTrendsChart>();
        #endregion

        #region Constructors
        public TrendsPage(string owner, string repository) : base(repository)
        {
            On<iOS>().SetPrefersHomeIndicatorAutoHidden(true);

            _owner = owner;
            _repository = repository;

            var activityIndicator = new ActivityIndicator { Color = ColorConstants.DarkNavyBlue };
            activityIndicator.SetBinding(IsVisibleProperty, nameof(TrendsViewModel.IsFetchingData));
            activityIndicator.SetBinding(ActivityIndicator.IsRunningProperty, nameof(TrendsViewModel.IsFetchingData));

            var absoluteLayout = new AbsoluteLayout();
            absoluteLayout.Children.Add(activityIndicator, new Rectangle(.5, .5, -1, -1), AbsoluteLayoutFlags.PositionProportional);
            absoluteLayout.Children.Add(_gitHubTrendsChartHolder.Value, new Rectangle(0, 0, 1, 1), AbsoluteLayoutFlags.All);

            Content = absoluteLayout;
        }
        #endregion

        #region Methods
        protected override void OnAppearing()
        {
            base.OnAppearing();

            ViewModel.FetchDataCommand?.Execute((_owner, _repository));
        }
        #endregion

        #region Classes
        class GitHubTrendsChart : SfChart
        {
            public GitHubTrendsChart()
            {
                var totalViewsSeries = new TrendsAreaSeries("Views", nameof(DailyViewsModel.LocalDay), nameof(DailyViewsModel.TotalViews), ColorConstants.DarkestBlue);
                totalViewsSeries.SetBinding(ChartSeries.ItemsSourceProperty, nameof(TrendsViewModel.DailyViewsList));

                var totalUniqueViewsSeries = new TrendsAreaSeries("Unique Views", nameof(DailyViewsModel.LocalDay), nameof(DailyViewsModel.TotalUniqueViews), ColorConstants.MediumBlue);
                totalUniqueViewsSeries.SetBinding(ChartSeries.ItemsSourceProperty, nameof(TrendsViewModel.DailyViewsList));

                var totalClonesSeries = new TrendsAreaSeries("Clones", nameof(DailyClonesModel.LocalDay), nameof(DailyClonesModel.TotalClones), ColorConstants.DarkNavyBlue);
                totalClonesSeries.SetBinding(ChartSeries.ItemsSourceProperty, nameof(TrendsViewModel.DailyClonesList));

                var totalUniqueClonesSeries = new TrendsAreaSeries("Unique Clones", nameof(DailyClonesModel.LocalDay), nameof(DailyClonesModel.TotalUniqueClones), ColorConstants.LightNavyBlue);
                totalUniqueClonesSeries.SetBinding(ChartSeries.ItemsSourceProperty, nameof(TrendsViewModel.DailyClonesList));

                this.SetBinding(IsVisibleProperty, nameof(TrendsViewModel.IsChartVisible));

                ChartBehaviors = new ChartBehaviorCollection
                {
                    new ChartZoomPanBehavior(),
                    new ChartTrackballBehavior()
                };

                Series = new ChartSeriesCollection
                {
                    totalViewsSeries,
                    totalUniqueViewsSeries,
                    totalClonesSeries,
                    totalUniqueClonesSeries
                };

                Legend = new ChartLegend
                {
                    DockPosition = LegendPlacement.Bottom,
                    ToggleSeriesVisibility = true,
                    IconWidth = 8,
                    IconHeight = 8,
                    LabelStyle = new ChartLegendLabelStyle { TextColor = ColorConstants.DarkNavyBlue }
                };

                var chartAxisLabelStyle = new ChartAxisLabelStyle { TextColor = ColorConstants.DarkNavyBlue };
                var axisLineStyle = new ChartLineStyle { StrokeColor = ColorConstants.LightNavyBlue };

                PrimaryAxis = new DateTimeAxis
                {
                    IntervalType = DateTimeIntervalType.Days,
                    Interval = 1,
                    RangePadding = DateTimeRangePadding.Round,
                    LabelStyle = chartAxisLabelStyle,
                    AxisLineStyle = axisLineStyle,
                    MajorTickStyle = new ChartAxisTickStyle { StrokeColor = Color.Transparent },
                    ShowMajorGridLines = false
                };
                PrimaryAxis.SetBinding(DateTimeAxis.MinimumProperty, nameof(TrendsViewModel.MinDateValue));
                PrimaryAxis.SetBinding(DateTimeAxis.MaximumProperty, nameof(TrendsViewModel.MaxDateValue));

                SecondaryAxis = new NumericalAxis
                {
                    LabelStyle = chartAxisLabelStyle,
                    AxisLineStyle = axisLineStyle,
                    MajorTickStyle = new ChartAxisTickStyle { StrokeColor = ColorConstants.LightNavyBlue },
                    ShowMajorGridLines = false
                };
                SecondaryAxis.SetBinding(NumericalAxis.MinimumProperty, nameof(TrendsViewModel.DailyViewsClonesMinValue));
                SecondaryAxis.SetBinding(NumericalAxis.MaximumProperty, nameof(TrendsViewModel.DailyViewsClonesMaxValue));

                BackgroundColor = Color.Transparent;

                Margin = Device.RuntimePlatform is Device.iOS ? new Thickness(0, 5) : new Thickness(0, 5, 0, 0);
            }

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
        #endregion
    }
}
