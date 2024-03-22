using System;
using Xamarin.CommunityToolkit.Markup;
using Xamarin.Forms.PancakeView;

namespace GitTrends;

public class CirclePancakeView : PancakeView
{
	public CirclePancakeView()
	{
		HeightRequest = WidthRequest = Math.Min(HeightRequest, WidthRequest);

		this.Bind<PancakeView, double, double>(CornerRadiusProperty, nameof(Width), convert: convertWidthToCornerRadius, source: this);

		IsClippedToBounds = true;

		static double convertWidthToCornerRadius(double width) => width is -1 ? -1 : width / 2;
	}
}