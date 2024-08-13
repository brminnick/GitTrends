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
			TotalViewsSeries = new TrendsAreaSeries(TrendsChartTitleConstants.TotalViewsTitle, nameof(DailyViewsModel.LocalDay), nameof(DailyViewsModel.TotalViews), nameof(BaseTheme.TotalViewsColor))
				.Bind(ChartSeries.ItemsSourceProperty,
					nameof(TrendsViewModel.DailyViewsList),
					source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext, typeof(TrendsViewModel)))
				.Bind(ChartSeries.IsVisibleProperty,
					nameof(TrendsViewModel.IsViewsSeriesVisible),
					source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext, typeof(TrendsViewModel)));

			TotalUniqueViewsSeries = new TrendsAreaSeries(TrendsChartTitleConstants.UniqueViewsTitle, nameof(DailyViewsModel.LocalDay), nameof(DailyViewsModel.TotalUniqueViews), nameof(BaseTheme.TotalUniqueViewsColor))
				.Bind(ChartSeries.ItemsSourceProperty,
					nameof(TrendsViewModel.DailyViewsList),
					source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext, typeof(TrendsViewModel)))
				.Bind(ChartSeries.IsVisibleProperty,
					nameof(TrendsViewModel.IsUniqueViewsSeriesVisible),
					source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext, typeof(TrendsViewModel)));

			TotalClonesSeries = new TrendsAreaSeries(TrendsChartTitleConstants.TotalClonesTitle, nameof(DailyClonesModel.LocalDay), nameof(DailyClonesModel.TotalClones), nameof(BaseTheme.TotalClonesColor))
				.Bind(ChartSeries.ItemsSourceProperty,
					nameof(TrendsViewModel.DailyClonesList),
					source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext, typeof(TrendsViewModel)))
				.Bind(ChartSeries.IsVisibleProperty,
					nameof(TrendsViewModel.IsClonesSeriesVisible),
					source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext, typeof(TrendsViewModel)));

			TotalUniqueClonesSeries = new TrendsAreaSeries(TrendsChartTitleConstants.UniqueClonesTitle, nameof(DailyClonesModel.LocalDay), nameof(DailyClonesModel.TotalUniqueClones), nameof(BaseTheme.TotalUniqueClonesColor))
				.Bind(ChartSeries.ItemsSourceProperty,
					nameof(TrendsViewModel.DailyClonesList),
					source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext, typeof(TrendsViewModel)))
				.Bind(ChartSeries.IsVisibleProperty,
					nameof(TrendsViewModel.IsUniqueClonesSeriesVisible),
					source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext, typeof(TrendsViewModel)));

			Series =
			[
				TotalViewsSeries,
				TotalUniqueViewsSeries,

				TotalClonesSeries,
				TotalUniqueClonesSeries
			];

			this.Bind(IsVisibleProperty, 
				nameof(TrendsViewModel.IsViewsClonesChartVisible),
				source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext, typeof(TrendsViewModel)));
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

				this.Bind(MinimumProperty, 
						nameof(TrendsViewModel.MinViewsClonesDate),
						source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext, typeof(TrendsViewModel)))
					.Bind(MaximumProperty, 
						nameof(TrendsViewModel.MaxViewsClonesDate),
						source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext, typeof(TrendsViewModel)));
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

				this.Bind(MinimumProperty, 
						nameof(TrendsViewModel.DailyViewsClonesMinValue),
						source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext, typeof(TrendsViewModel)))
					.Bind(MaximumProperty, 
						nameof(TrendsViewModel.DailyViewsClonesMaxValue),
						source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext, typeof(TrendsViewModel)))
					.Bind(IntervalProperty, 
						nameof(TrendsViewModel.ViewsClonesChartYAxisInterval),
						source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext, typeof(TrendsViewModel)));
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