using CommunityToolkit.Maui.Markup;
using Microsoft.Maui.Controls.Shapes;

namespace GitTrends;

public class CircleBorder : Border
{
	public CircleBorder()
	{
		HeightRequest = WidthRequest = Math.Min(HeightRequest, WidthRequest);

		StrokeShape = new RoundRectangle()
			.Bind(RoundRectangle.CornerRadiusProperty, 
				getter: circleBorder => circleBorder.Width, 
				convert: ConvertWidthToCornerRadius, 
				source: this);

		static double ConvertWidthToCornerRadius(double width) => width is -1 ? -1 : width / 2;
	}
}