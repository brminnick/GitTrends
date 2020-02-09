using GitTrends.iOS;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(NavigationPage), typeof(LargeTitleNavigationRenderer))]
namespace GitTrends.iOS
{
    public class LargeTitleNavigationRenderer : NavigationRenderer
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var app = (App)Xamarin.Forms.Application.Current;
            app.ThemeChanged += HandleThemeChanged;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            UpdateNavigationBarColors();
        }

        void HandleThemeChanged(object sender, Theme e) => UpdateNavigationBarColors();

        void UpdateNavigationBarColors()
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion(13, 0)
                    && Element is NavigationPage navigationPage)
            {
                var navigationPageBackgroundColor = navigationPage.BarBackgroundColor;

                NavigationBar.StandardAppearance.BackgroundColor = navigationPageBackgroundColor == Color.Default
                                                                    ? UINavigationBar.Appearance.BarTintColor
                                                                    : navigationPageBackgroundColor.ToUIColor();

                if (NavigationBar.TitleTextAttributes != null)
                    NavigationBar.StandardAppearance.TitleTextAttributes = NavigationBar.TitleTextAttributes;

                if (NavigationBar.LargeTitleTextAttributes != null)
                    NavigationBar.StandardAppearance.LargeTitleTextAttributes = NavigationBar.LargeTitleTextAttributes;

                NavigationBar.ScrollEdgeAppearance = NavigationBar.StandardAppearance;
            }
        }
    }
}
