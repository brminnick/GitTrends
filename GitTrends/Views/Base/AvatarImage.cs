using CommunityToolkit.Maui.Markup;
using GitTrends.Resources;

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
		GetBorderColor = () => AppResources.GetResource<Color>(nameof(BaseTheme.SeparatorColor));

		StrokeThickness = 1;
	}
}