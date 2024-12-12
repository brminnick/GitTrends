using CommunityToolkit.Maui.Markup;
using GitTrends.Common;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using GitTrends.Resources;
using Syncfusion.Maui.Charts;

namespace GitTrends;

class ViewsClonesChart(TrendsViewModel trendsViewModel) : BaseChartView(new ViewsClonesTrendsChart(trendsViewModel))
{
    sealed class ViewsClonesTrendsChart : BaseTrendsChart
    {
        public ViewsClonesTrendsChart(TrendsViewModel trendsViewModel)
            : base(TrendsPageAutomationIds.ViewsClonesChart, new ViewsClonesPrimaryAxis(),
                new ViewsClonesSecondaryAxis(), trendsViewModel)
        {
            TotalViewsSeries = new TrendsAreaSeries(TrendsChartTitleConstants.TotalViewsTitle,
                    nameof(DailyViewsModel.LocalDay), nameof(DailyViewsModel.TotalViews),
                    nameof(BaseTheme.TotalViewsColor))
                .Bind(ChartSeries.ItemsSourceProperty,
                    getter: static (TrendsViewModel vm) => vm.DailyViewsList)
                .Bind(ChartSeries.IsVisibleProperty,
                    getter: static (TrendsViewModel vm) => vm.IsViewsSeriesVisible);

            TotalUniqueViewsSeries = new TrendsAreaSeries(TrendsChartTitleConstants.UniqueViewsTitle,
                    nameof(DailyViewsModel.LocalDay), nameof(DailyViewsModel.TotalUniqueViews),
                    nameof(BaseTheme.TotalUniqueViewsColor))
                .Bind(ChartSeries.ItemsSourceProperty,
                    getter: static (TrendsViewModel vm) => vm.DailyViewsList)
                .Bind(ChartSeries.IsVisibleProperty,
                    getter: static (TrendsViewModel vm) => vm.IsUniqueViewsSeriesVisible);

            TotalClonesSeries = new TrendsAreaSeries(TrendsChartTitleConstants.TotalClonesTitle,
                    nameof(DailyClonesModel.LocalDay), nameof(DailyClonesModel.TotalClones),
                    nameof(BaseTheme.TotalClonesColor))
                .Bind(ChartSeries.ItemsSourceProperty,
                    getter: static (TrendsViewModel vm) => vm.DailyClonesList)
                .Bind(ChartSeries.IsVisibleProperty,
                    getter: static (TrendsViewModel vm) => vm.IsClonesSeriesVisible);

            TotalUniqueClonesSeries = new TrendsAreaSeries(TrendsChartTitleConstants.UniqueClonesTitle,
                    nameof(DailyClonesModel.LocalDay), nameof(DailyClonesModel.TotalUniqueClones),
                    nameof(BaseTheme.TotalUniqueClonesColor))
                .Bind(ChartSeries.ItemsSourceProperty,
                    getter: static (TrendsViewModel vm) => vm.DailyClonesList)
                .Bind(ChartSeries.IsVisibleProperty,
                    getter: static (TrendsViewModel vm) => vm.IsUniqueClonesSeriesVisible);

            Series =
            [
                TotalViewsSeries,
                TotalUniqueViewsSeries,

                TotalClonesSeries,
                TotalUniqueClonesSeries
            ];

            this.Bind(IsVisibleProperty,
                getter: static (TrendsViewModel vm) => vm.IsViewsClonesChartVisible);
        }

        public AreaSeries TotalViewsSeries { get; }
        public AreaSeries TotalUniqueViewsSeries { get; }
        public AreaSeries TotalClonesSeries { get; }
        public AreaSeries TotalUniqueClonesSeries { get; }

        protected override void SetPaletteBrushColors()
        {
            PaletteBrushes =
            [
                AppResources.GetResource<Color>(nameof(BaseTheme.TotalViewsColor)),
                AppResources.GetResource<Color>(nameof(BaseTheme.TotalUniqueViewsColor)),

                AppResources.GetResource<Color>(nameof(BaseTheme.TotalClonesColor)),
                AppResources.GetResource<Color>(nameof(BaseTheme.TotalUniqueClonesColor))
            ];
        }

        sealed class ViewsClonesPrimaryAxis : DateTimeAxis
        {
            public ViewsClonesPrimaryAxis()
            {
                Interval = 1;
                IntervalType = DateTimeIntervalType.Days;
                RangePadding = DateTimeRangePadding.Round;
                AxisLineStyle = new AxisLineStyle();

                MajorTickStyle = new ChartAxisTickStyle
                {
                    Stroke = Colors.Transparent
                };
                ShowMajorGridLines = false;

                LabelStyle = new ChartAxisLabelStyle
                {
                    FontSize = 9,
                    FontFamily = FontFamilyConstants.RobotoRegular,
                    Margin = new Thickness(2, 4, 2, 0)
                }.DynamicResource(ChartLabelStyle.TextColorProperty, nameof(BaseTheme.ChartAxisTextColor));

                this.Bind(MinimumProperty,
                        getter: static (TrendsViewModel vm) => vm.MinViewsClonesDate)
                    .Bind(MaximumProperty,
                        getter: static (TrendsViewModel vm) => vm.MaxViewsClonesDate);
            }
        }

        sealed class ViewsClonesSecondaryAxis : NumericalAxis
        {
            public ViewsClonesSecondaryAxis()
            {
                ShowMajorGridLines = false;
                AxisLineStyle = new AxisLineStyle();
                MajorTickStyle = new ChartAxisTickStyle().DynamicResource(ChartAxisTickStyle.StrokeProperty,
                    nameof(BaseTheme.ChartAxisLineColor));

                LabelStyle = new ChartAxisLabelStyle
                {
                    FontSize = 12,
                    FontFamily = FontFamilyConstants.RobotoRegular,
                }.DynamicResource(ChartLabelStyle.TextColorProperty, nameof(BaseTheme.ChartAxisTextColor));

                this.Bind(MinimumProperty,
                        getter: static (TrendsViewModel vm) => vm.DailyViewsClonesMinValue)
                    .Bind(MaximumProperty,
                        getter: static (TrendsViewModel vm) => vm.DailyViewsClonesMaxValue)
                    .Bind(IntervalProperty,
                        getter: static (TrendsViewModel vm) => vm.ViewsClonesChartYAxisInterval);
            }
        }

        sealed class AxisLineStyle : ChartLineStyle
        {
            public AxisLineStyle()
            {
                StrokeWidth = 1.51;
                this.DynamicResource(StrokeProperty, nameof(BaseTheme.ChartAxisLineColor));
            }
        }
    }
}