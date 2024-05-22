using CommunityToolkit.Maui.Markup;

namespace GitTrends;

public class CircleBorder : Border
{
	public CircleBorder()
	{
		HeightRequest = WidthRequest = Math.Min(HeightRequest, WidthRequest);

		this.Bind<Border, double, double>(CornerRadiusProperty, nameof(Width), convert: convertWidthToCornerRadius, source: this);

		static double convertWidthToCornerRadius(double width) => width is -1 ? -1 : width / 2;
	}
}