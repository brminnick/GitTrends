using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using GitTrends.Shared;
using Syncfusion.SfChart.XForms;
using Xamarin.CommunityToolkit.Markup;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;

namespace GitTrends;

class ViewsClonesChart : BaseChartView
{
	public ViewsClonesChart(IMainThread mainThread) : base(new ViewsClonesTrendsChart(mainThread))
	{
	}

	class ViewsClonesTrendsChart : BaseTrendsChart
	{
		public ViewsClonesTrendsChart(IMainThread mainThread) : base(mainThread, TrendsPageAutomationIds.ViewsClonesChart)
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

			Series = new ChartSeriesCollection
				{
					TotalViewsSeries,
					TotalUniqueViewsSeries,
					TotalClonesSeries,
					TotalUniqueClonesSeries,
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
			PrimaryAxis.SetBinding(DateTimeAxis.MinimumProperty, nameof(TrendsViewModel.MinViewsClonesDate));
			PrimaryAxis.SetBinding(DateTimeAxis.MaximumProperty, nameof(TrendsViewModel.MaxViewsClonesDate));

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
				ShowMajorGridLines = false,
			}.Bind(NumericalAxis.MinimumProperty, nameof(TrendsViewModel.DailyViewsClonesMinValue))
			 .Bind(NumericalAxis.MaximumProperty, nameof(TrendsViewModel.DailyViewsClonesMaxValue))
			 .Bind(NumericalAxis.IntervalProperty, nameof(TrendsViewModel.ViewsClonesChartYAxisInterval));

			this.SetBinding(IsVisibleProperty, nameof(TrendsViewModel.IsViewsClonesChartVisible));
		}

		public AreaSeries TotalViewsSeries { get; }
		public AreaSeries TotalUniqueViewsSeries { get; }
		public AreaSeries TotalClonesSeries { get; }
		public AreaSeries TotalUniqueClonesSeries { get; }
	}
}