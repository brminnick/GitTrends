using GitTrends.iOS;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(IndicatorView), typeof(IndicatorViewCustomRenderer))]
namespace GitTrends.iOS
{
    public class IndicatorViewCustomRenderer : IndicatorViewRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<IndicatorView> e)
        {
            base.OnElementChanged(e);

#warning Workaround for https://github.com/brminnick/GitTrends/issues/154
            if (UIDevice.CurrentDevice.CheckSystemVersion(14, 0) && Control is UIPageControl pageControl)
            {
                pageControl.AllowsContinuousInteraction = false;
                pageControl.BackgroundStyle = UIPageControlBackgroundStyle.Minimal;
            }
        }
    }
}
