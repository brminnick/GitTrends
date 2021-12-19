using System.Linq;
using GitTrends.iOS;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(CarouselPage), typeof(CarouselPageCustomRenderer))]
namespace GitTrends.iOS
{
    public class CarouselPageCustomRenderer : CarouselPageRenderer
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            if (View?.Subviews.OfType<UIScrollView>().SingleOrDefault() is UIScrollView scrollView)
                scrollView.ContentInsetAdjustmentBehavior = UIScrollViewContentInsetAdjustmentBehavior.Never;
        }
    }
}