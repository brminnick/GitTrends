using CommunityToolkit.Maui.Markup;
using Syncfusion.Maui.Charts;

namespace GitTrends;

abstract class BaseTrendsChart : SfCartesianChart
{
	readonly ChartZoomPanBehavior _chartZoomPanBehavior = new();

	protected BaseTrendsChart(in string automationId)
	{
		AutomationId = automationId;

		Margin = 0;
		ChartPadding = new Thickness(0, 24, 0, 4);

		BackgroundColor = Colors.Transparent;

		ChartBehaviors = new ChartBehaviorCollection
		{
			_chartZoomPanBehavior,
			new ChartTrackballBehavior()
		};
	}

	protected Task SetZoom(double primaryAxisStart, double primaryAxisEnd, double secondaryAxisStart, double secondaryAxisEnd) => Dispatcher.DispatchAsync(() =>
	{
		_chartZoomPanBehavior.ZoomByRange(PrimaryAxis, primaryAxisStart, primaryAxisEnd);
		_chartZoomPanBehavior.ZoomByRange(SecondaryAxis, secondaryAxisStart, secondaryAxisEnd);
	});

	protected class TrendsAreaSeries : AreaSeries
	{
		public TrendsAreaSeries(in string title, in string xDataTitle, in string yDataTitle, in string colorResource)
		{
			Opacity = 0.9;
			Label = title;
			XBindingPath = xDataTitle;
			YBindingPath = yDataTitle;
			LegendIcon = ChartLegendIcon.SeriesType;

			this.DynamicResource(Color, colorResource);
		}
	}
}