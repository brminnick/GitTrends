using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using GitTrends.Shared;
using CommunityToolkit.Maui.Markup;
using Syncfusion.Maui.Charts;

namespace GitTrends;

class ViewsClonesChart() : BaseChartView(new ViewsClonesTrendsChart())
{
	sealed class ViewsClonesTrendsChart : BaseTrendsChart
	{
		public ViewsClonesTrendsChart()
			: base(TrendsPageAutomationIds.ViewsClonesChart, new ViewsClonesPrimaryAxis(), new ViewsClonesSecondaryAxis())
		{
			TotalViewsSeries = new TrendsAreaSeries(TrendsChartTitleConstants.TotalViewsTitle, nameof(DailyViewsModel.LocalDay), nameof(DailyViewsModel.TotalViews), nameof(BaseTheme.TotalViewsColor));
			TotalViewsSeries.SetBinding(ChartSeries.ItemsSourceProperty, nameof(TrendsViewModel.DailyViewsList));
			TotalViewsSeries.SetBinding(ChartSeries.IsVisibleProperty, nameof(TrendsViewModel.IsViewsSeriesVisible));

			TotalUniqueViewsSeries = new TrendsAreaSeries(TrendsChartTitleConstants.UniqueViewsTitle, nameof(DailyViewsModel.LocalDay), nameof(DailyViewsModel.TotalUniqueViews), nameof(BaseTheme.TotalUniqueViewsColor));
			TotalUniqueViewsSeries.SetBinding(ChartSeries.ItemsSourceProperty, nameof(TrendsViewModel.DailyViewsList));
			TotalUniqueViewsSeries.SetBinding(ChartSeries.IsVisibleProperty, nameof(TrendsViewModel.IsUniqueViewsSeriesVisible));

			TotalClonesSeries = new TrendsAreaSeries(TrendsChartTitleConstants.TotalClonesTitle, nameof(DailyClonesModel.LocalDay), nameof(DailyClonesModel.TotalClones), nameof(BaseTheme.TotalClonesColor));
			TotalClonesSeries.SetBinding(ChartSeries.ItemsSourceProperty, nameof(TrendsViewModel.DailyClonesList));
			TotalClonesSeries.SetBinding(ChartSeries.IsVisibleProperty, nameof(TrendsViewModel.IsClonesSeriesVisible));

			TotalUniqueClonesSeries = new TrendsAreaSeries(TrendsChartTitleConstants.UniqueClonesTitle, nameof(DailyClonesModel.LocalDay), nameof(DailyClonesModel.TotalUniqueClones), nameof(BaseTheme.TotalUniqueClonesColor));
			TotalUniqueClonesSeries.SetBinding(ChartSeries.ItemsSourceProperty, nameof(TrendsViewModel.DailyClonesList));
			TotalUniqueClonesSeries.SetBinding(ChartSeries.IsVisibleProperty, nameof(TrendsViewModel.IsUniqueClonesSeriesVisible));

			Series =
			[
				TotalViewsSeries,
				TotalUniqueViewsSeries,

				TotalClonesSeries,
				TotalUniqueClonesSeries
			];

			this.SetBinding(IsVisibleProperty, nameof(TrendsViewModel.IsViewsClonesChartVisible));
		}

		public AreaSeries TotalViewsSeries { get; }
		public AreaSeries TotalUniqueViewsSeries { get; }
		public AreaSeries TotalClonesSeries { get; }
		public AreaSeries TotalUniqueClonesSeries { get; }

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

				this.Bind(MinimumProperty, nameof(TrendsViewModel.MinViewsClonesDate))
					.Bind(MaximumProperty, nameof(TrendsViewModel.MaxViewsClonesDate));
			}
		}

		sealed class ViewsClonesSecondaryAxis : NumericalAxis
		{
			public ViewsClonesSecondaryAxis()
			{
				ShowMajorGridLines = false;
				AxisLineStyle = new AxisLineStyle();
				MajorTickStyle = new ChartAxisTickStyle().DynamicResource(ChartAxisTickStyle.StrokeProperty, nameof(BaseTheme.ChartAxisLineColor));
				
				LabelStyle = new ChartAxisLabelStyle
				{
					FontSize = 12,
					FontFamily = FontFamilyConstants.RobotoRegular,
				}.DynamicResource(ChartLabelStyle.TextColorProperty, nameof(BaseTheme.ChartAxisTextColor));
				
				this.Bind(MinimumProperty, nameof(TrendsViewModel.DailyViewsClonesMinValue))
					.Bind(MaximumProperty, nameof(TrendsViewModel.DailyViewsClonesMaxValue))
					.Bind(IntervalProperty, nameof(TrendsViewModel.ViewsClonesChartYAxisInterval));
			}
		}

		sealed class AxisLineStyle : ChartLineStyle
		{
			public AxisLineStyle()
			{
				StrokeWidth = 1.51;
				this.DynamicResource(ChartLineStyle.StrokeProperty, nameof(BaseTheme.ChartAxisLineColor));
			}
		}
	}
}