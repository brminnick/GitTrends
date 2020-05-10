using GitTrends.Mobile.Shared;
using GitTrends.Shared;
using Sharpnado.MaterialFrame;
using Syncfusion.SfChart.XForms;
using Xamarin.Forms;
using Xamarin.Forms.Markup;

namespace GitTrends
{
    class TrendsChart : MaterialFrame
    {
        const int _topPadding = 16;

        public TrendsChart()
        {
            CornerRadius = 4;
            Elevation = 4;
            Content = new Chart().Bind(IsVisibleProperty, nameof(TrendsViewModel.IsChartVisible), BindingMode.OneWay);

            Margin = new Thickness(16, 0);
            Padding = new Thickness(4, _topPadding, 0, 4);

            SetDynamicResource(MaterialThemeProperty, nameof(BaseTheme.MaterialFrameTheme));
        }

        class Chart : SfChart
        {
            public Chart()
            {
                Margin = 0;
                ChartPadding = new Thickness(0, _topPadding, 0, 4);

                BackgroundColor = Color.Transparent;

                AutomationId = TrendsPageAutomationIds.TrendsChart;

                TotalViewsSeries = new TrendsAreaSeries(TrendsChartConstants.TotalViewsTitle, nameof(DailyViewsModel.LocalDay), nameof(DailyViewsModel.TotalViews), nameof(BaseTheme.TotalViewsColor));
                TotalViewsSeries.SetBinding(ChartSeries.ItemsSourceProperty, nameof(TrendsViewModel.DailyViewsList));
                TotalViewsSeries.SetBinding(ChartSeries.IsVisibleProperty, nameof(TrendsViewModel.IsViewsSeriesVisible));

                TotalUniqueViewsSeries = new TrendsAreaSeries(TrendsChartConstants.UniqueViewsTitle, nameof(DailyViewsModel.LocalDay), nameof(DailyViewsModel.TotalUniqueViews), nameof(BaseTheme.TotalUniqueViewsColor));
                TotalUniqueViewsSeries.SetBinding(ChartSeries.ItemsSourceProperty, nameof(TrendsViewModel.DailyViewsList));
                TotalUniqueViewsSeries.SetBinding(ChartSeries.IsVisibleProperty, nameof(TrendsViewModel.IsUniqueViewsSeriesVisible));

                TotalClonesSeries = new TrendsAreaSeries(TrendsChartConstants.TotalClonesTitle, nameof(DailyClonesModel.LocalDay), nameof(DailyClonesModel.TotalClones), nameof(BaseTheme.TotalClonesColor));
                TotalClonesSeries.SetBinding(ChartSeries.ItemsSourceProperty, nameof(TrendsViewModel.DailyClonesList));
                TotalClonesSeries.SetBinding(ChartSeries.IsVisibleProperty, nameof(TrendsViewModel.IsClonesSeriesVisible));

                TotalUniqueClonesSeries = new TrendsAreaSeries(TrendsChartConstants.UniqueClonesTitle, nameof(DailyClonesModel.LocalDay), nameof(DailyClonesModel.TotalUniqueClones), nameof(BaseTheme.TotalUniqueClonesColor));
                TotalUniqueClonesSeries.SetBinding(ChartSeries.ItemsSourceProperty, nameof(TrendsViewModel.DailyClonesList));
                TotalUniqueClonesSeries.SetBinding(ChartSeries.IsVisibleProperty, nameof(TrendsViewModel.IsUniqueClonesSeriesVisible));

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
                    TotalUniqueClonesSeries,
                };

                var chartLegendLabelStyle = new ChartLegendLabelStyle()
                {
                    FontSize = 11,
                    FontFamily = FontFamilyConstants.RobotoRegular,
                };
                chartLegendLabelStyle.SetDynamicResource(ChartLegendLabelStyle.TextColorProperty, nameof(BaseTheme.ChartAxisTextColor));

                Legend = new ChartLegend
                {
                    AutomationId = TrendsPageAutomationIds.TrendsChartLegend,
                    DockPosition = LegendPlacement.Bottom,
                    ToggleSeriesVisibility = true,
                    Margin = new Thickness(4, 8, 0, 4),
                    IconWidth = 20,
                    IconHeight = 20,
                    LabelStyle = chartLegendLabelStyle,
                    ItemMargin = new Thickness(2, 0)
                };

                var axisLabelStyle = new ChartAxisLabelStyle
                {
                    FontSize = 12,
                    FontFamily = FontFamilyConstants.RobotoRegular,
                    Margin = new Thickness(2, 4, 2, 0)
                };
                axisLabelStyle.SetDynamicResource(ChartLabelStyle.TextColorProperty, nameof(BaseTheme.ChartAxisTextColor));

                var axisLineStyle = new ChartLineStyle()
                {
                    StrokeWidth = 1.51
                };
                axisLineStyle.SetDynamicResource(ChartLineStyle.StrokeColorProperty, nameof(BaseTheme.ChartAxisLineColor));

                PrimaryAxis = new DateTimeAxis
                {
                    AutomationId = TrendsPageAutomationIds.TrendsChartPrimaryAxis,
                    IntervalType = DateTimeIntervalType.Days,
                    Interval = 1,
                    RangePadding = DateTimeRangePadding.Round,
                    LabelStyle = axisLabelStyle,
                    AxisLineStyle = axisLineStyle,
                    MajorTickStyle = new ChartAxisTickStyle { StrokeColor = Color.Transparent },
                    ShowMajorGridLines = false,
                };
                PrimaryAxis.SetBinding(DateTimeAxis.MinimumProperty, nameof(TrendsViewModel.MinDateValue));
                PrimaryAxis.SetBinding(DateTimeAxis.MaximumProperty, nameof(TrendsViewModel.MaxDateValue));

                var secondaryAxisMajorTickStyle = new ChartAxisTickStyle();
                secondaryAxisMajorTickStyle.SetDynamicResource(ChartAxisTickStyle.StrokeColorProperty, nameof(BaseTheme.ChartAxisLineColor));

                SecondaryAxis = new NumericalAxis
                {
                    AutomationId = TrendsPageAutomationIds.TrendsChartSecondaryAxis,
                    LabelStyle = axisLabelStyle,
                    AxisLineStyle = axisLineStyle,
                    MajorTickStyle = secondaryAxisMajorTickStyle,
                    ShowMajorGridLines = false
                };
                SecondaryAxis.SetBinding(NumericalAxis.MinimumProperty, nameof(TrendsViewModel.DailyViewsClonesMinValue));
                SecondaryAxis.SetBinding(NumericalAxis.MaximumProperty, nameof(TrendsViewModel.DailyViewsClonesMaxValue));
            }

            public AreaSeries TotalViewsSeries { get; }
            public AreaSeries TotalUniqueViewsSeries { get; }
            public AreaSeries TotalClonesSeries { get; }
            public AreaSeries TotalUniqueClonesSeries { get; }

            class TrendsAreaSeries : AreaSeries
            {
                public TrendsAreaSeries(in string title, in string xDataTitle, in string yDataTitle, in string colorResource)
                {
                    Opacity = 0.9;
                    Label = title;
                    XBindingPath = xDataTitle;
                    YBindingPath = yDataTitle;
                    LegendIcon = ChartLegendIcon.SeriesType;

                    SetDynamicResource(ColorProperty, colorResource);
                }
            }
        }
    }
}