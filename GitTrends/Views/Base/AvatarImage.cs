using CommunityToolkit.Maui.Markup;
using Microsoft.Maui.Controls.Shapes;

namespace GitTrends;

class AvatarImage : CircleImage
{
	public AvatarImage(in ImageSource imageSource, in double diameter) : this(diameter)
	{
		ImageSource = imageSource;
	}

	public AvatarImage(in double diameter)
	{
		this.Center();

		WidthRequest = HeightRequest = diameter;
		BorderColor = Colors.Black;

		StrokeThickness = 1;
	}
}