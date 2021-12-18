using Sharpnado.MaterialFrame;
using Xamarin.Forms;

namespace GitTrends;

abstract class BaseChartView : MaterialFrame
{
	protected BaseChartView(in BaseTrendsChart trendsChart)
	{
		CornerRadius = 4;
		Elevation = 4;
		Content = trendsChart;

		Margin = new Thickness(16, 0);
		Padding = new Thickness(4, 8, 4, 4);

		this.DynamicResource(MaterialThemeProperty, nameof(BaseTheme.MaterialFrameTheme));
	}
}