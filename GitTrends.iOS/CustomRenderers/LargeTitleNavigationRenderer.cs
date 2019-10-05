using GitTrends.iOS.CustomRenderers;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(NavigationPage), typeof(LargeTitleNavigationRenderer))]
namespace GitTrends.iOS.CustomRenderers
{
    public class LargeTitleNavigationRenderer : NavigationRenderer
    {
        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            var navigationPage = (NavigationPage)Element;
            var navigationPageBackgroundColor = navigationPage.BarBackgroundColor;

            NavigationBar.StandardAppearance.BackgroundColor = navigationPageBackgroundColor == Color.Default
                ? UINavigationBar.Appearance.BarTintColor
                : navigationPageBackgroundColor.ToUIColor();

            NavigationBar.StandardAppearance.TitleTextAttributes = NavigationBar.TitleTextAttributes;
            NavigationBar.StandardAppearance.LargeTitleTextAttributes = NavigationBar.LargeTitleTextAttributes;

            NavigationBar.ScrollEdgeAppearance = NavigationBar.StandardAppearance;
        }
    }
}
