using CommunityToolkit.Maui.Markup;
using GitTrends.Mobile.Common;
using Syncfusion.Maui.Charts;

namespace GitTrends;

abstract class BaseTrendsChart : SfCartesianChart
{
	readonly ChartZoomPanBehavior _chartZoomPanBehavior = new();

	protected BaseTrendsChart(in string automationId, in DateTimeAxis primaryAxis, in NumericalAxis secondaryAxis, in TrendsViewModel trendsViewModel)
	{
		ThemeService.PreferenceChanged += HandleThemePreferenceChanged;

		BindingContext = trendsViewModel;

		AutomationId = automationId;

		Margin = new Thickness(0, 4, 0, 4);

		BackgroundColor = Colors.Transparent;

		PrimaryAxis = primaryAxis;
		SecondaryAxis = secondaryAxis;

		XAxes.Add(PrimaryAxis);
		YAxes.Add(SecondaryAxis);

		ZoomPanBehavior = _chartZoomPanBehavior;
		TrackballBehavior = new ChartTrackballBehavior();

		SetPaletteBrushColors();
	}

	protected DateTimeAxis PrimaryAxis { get; }
	protected NumericalAxis SecondaryAxis { get; }

	protected abstract void SetPaletteBrushColors();

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
			LegendIcon = ChartLegendIconType.SeriesType;

			this.DynamicResource(StrokeProperty, colorResource);
		}
	}

	void HandleThemePreferenceChanged(object? sender, PreferredTheme e) => SetPaletteBrushColors();
}