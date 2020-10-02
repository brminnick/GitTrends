using System;
using System.Collections.Generic;
using GitTrends.Mobile.Common;
using Syncfusion.SfChart.XForms;
using Xamarin.Forms;
using Xamarin.Forms.Markup;

namespace GitTrends
{
    class StarsChart : BaseChartView
    {
        public StarsChart() : base(new StarsTrendsChart())
        {
        }

        class StarsTrendsChart : BaseTrendsChart
        {
            public StarsTrendsChart() : base(TrendsPageAutomationIds.ViewsClonesChart)
            {
                StarsSeries = new TrendsAreaSeries("Star History", nameof(DailyStarsModel.LocalDay), nameof(DailyStarsModel.TotalStars), nameof(BaseTheme.CardStarsStatsIconColor));
                StarsSeries.SetBinding(ChartSeries.ItemsSourceProperty, nameof(TrendsViewModel.DailyStarsList));
                StarsSeries.SetBinding(ChartSeries.IsVisibleProperty, nameof(TrendsViewModel.IsViewsSeriesVisible));

                Series = new ChartSeriesCollection
                {
                    StarsSeries
                };

                var primaryAxisLabelStyle = new ChartAxisLabelStyle
                {
                    FontSize = 9,
                    FontFamily = FontFamilyConstants.RobotoRegular,
                    Margin = new Thickness(2, 4, 2, 0)
                }.DynamicResource(ChartLabelStyle.TextColorProperty, nameof(BaseTheme.ChartAxisTextColor));

                var axisLineStyle = new ChartLineStyle()
                {
                    StrokeWidth = 1.51
                }.DynamicResource(ChartLineStyle.StrokeColorProperty, nameof(BaseTheme.ChartAxisLineColor));

                PrimaryAxis = new DateTimeAxis
                {
                    IntervalType = DateTimeIntervalType.Days,
                    Interval = 1,
                    RangePadding = DateTimeRangePadding.Round,
                    LabelStyle = primaryAxisLabelStyle,
                    AxisLineStyle = axisLineStyle,
                    MajorTickStyle = new ChartAxisTickStyle { StrokeColor = Color.Transparent },
                    ShowMajorGridLines = false,
                };
                PrimaryAxis.SetBinding(DateTimeAxis.MinimumProperty, nameof(TrendsViewModel.MinDailyStarsDate));
                PrimaryAxis.SetBinding(DateTimeAxis.MaximumProperty, nameof(TrendsViewModel.MaxDailyStarsDate));

                var secondaryAxisMajorTickStyle = new ChartAxisTickStyle().DynamicResource(ChartAxisTickStyle.StrokeColorProperty, nameof(BaseTheme.ChartAxisLineColor));

                var secondaryAxisLabelStyle = new ChartAxisLabelStyle
                {
                    FontSize = 12,
                    FontFamily = FontFamilyConstants.RobotoRegular,
                }.DynamicResource(ChartLabelStyle.TextColorProperty, nameof(BaseTheme.ChartAxisTextColor));

                SecondaryAxis = new NumericalAxis
                {
                    LabelStyle = secondaryAxisLabelStyle,
                    AxisLineStyle = axisLineStyle,
                    MajorTickStyle = secondaryAxisMajorTickStyle,
                    ShowMajorGridLines = false
                }.Bind(NumericalAxis.MinimumProperty, nameof(TrendsViewModel.DailyStarsMinValue))
                 .Bind(NumericalAxis.MaximumProperty, nameof(TrendsViewModel.DailyStarsMaxValue));
            }

            public AreaSeries StarsSeries { get; }
        }
    }
}