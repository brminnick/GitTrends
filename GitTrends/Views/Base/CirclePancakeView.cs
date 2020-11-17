using Xamarin.Forms.Markup;
using Xamarin.Forms.PancakeView;

namespace GitTrends
{
    public class CirclePancakeView : PancakeView
    {
        public CirclePancakeView()
        {
            this.Bind<PancakeView, double, double>(CornerRadiusProperty, nameof(Width), convert: convertWidthToCornerRadius, source: this);

            IsClippedToBounds = true;

            static double convertWidthToCornerRadius(double width) => width is -1 ? -1 : width / 2;
        }
    }
}
