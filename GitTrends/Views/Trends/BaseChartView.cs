using CommunityToolkit.Maui.Markup;
using Sharpnado.MaterialFrame;

namespace GitTrends;

abstract class BaseChartView : MaterialFrame
{
	protected BaseChartView(in BaseTrendsChart trendsChart)
	{
		CornerRadius = 4;
		Elevation = 4;
		Content = trendsChart;

		Margin = new Thickness(16, 0);
		Padding = new Thickness(0, 8, 4, 4);

		this.DynamicResource(MaterialThemeProperty, nameof(BaseTheme.MaterialFrameTheme));
	}
}