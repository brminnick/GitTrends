using CommunityToolkit.Maui.Markup;
using Microsoft.Maui.Controls.Shapes;

namespace GitTrends;

class SvgTextLabel : Border
{
	public SvgTextLabel(in IDeviceInfo deviceInfo, in string svgFileName, in string text, in string automationId, in int fontSize, in string fontFamily, in double logoTextSpacing)
	{
		AutomationId = automationId;

		StrokeShape = new RoundRectangle
		{
			CornerRadius = new CornerRadius(4)
		};

		Content = new StackLayout
		{
			Orientation = StackOrientation.Horizontal,
			Spacing = logoTextSpacing,
			Children =
			{
				new SvgImage(deviceInfo, svgFileName, () => Colors.White),
				new TextLabel(text, fontSize,fontFamily)
			}
		};

		this.Center().Padding(16, 10);
	}

	class TextLabel : Label
	{
		public TextLabel(in string text, in int fontSize = 18, in string fontFamily = FontFamilyConstants.RobotoRegular)
		{
			Text = text;
			TextColor = Colors.White;

			LineBreakMode = LineBreakMode.TailTruncation;

			this.Fill().TextCenter().Font(fontFamily, fontSize);
		}
	}
}