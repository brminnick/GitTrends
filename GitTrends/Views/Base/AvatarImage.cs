using Xamarin.CommunityToolkit.Markup;
using Xamarin.Forms;
using Xamarin.Forms.PancakeView;

namespace GitTrends
{
	class AvatarImage : CircleImage
	{
		public AvatarImage(in ImageSource imageSource, in double diameter) : this(diameter)
		{
			ImageSource = imageSource;
		}

		public AvatarImage(in double diameter)
		{
			this.CenterExpand();

			WidthRequest = HeightRequest = diameter;

			Border = new Border
			{
				Thickness = 1,
				Color = Color.Black
			};
		}
	}
}