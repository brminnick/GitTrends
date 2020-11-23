using Xamarin.Forms;
using Xamarin.Forms.Markup;
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
            this.Center();

            WidthRequest = diameter;

            Border = new Border
            {
                Thickness = 1,
                Color = Color.Black
            };
        }
    }
}
