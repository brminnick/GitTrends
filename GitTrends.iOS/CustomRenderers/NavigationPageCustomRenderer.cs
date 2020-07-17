using Autofac;
using CoreGraphics;
using GitTrends.iOS;
using GitTrends.Mobile.Common;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(NavigationPage), typeof(NavigationPageCustomRenderer))]
namespace GitTrends.iOS
{
    public class NavigationPageCustomRenderer : NavigationRenderer
    {
        readonly ThemeService _themeService;

        public NavigationPageCustomRenderer()
        {
            ThemeService.PreferenceChanged += HandlePreferenceChanged;

            _themeService = ContainerService.Container.Resolve<ThemeService>();
        }

        protected override void OnElementChanged(VisualElementChangedEventArgs e)
        {
            base.OnElementChanged(e);

            AddShadow(_themeService.Preference);
        }

        void HandlePreferenceChanged(object sender, PreferredTheme e) => AddShadow(e);

        void AddShadow(PreferredTheme preferredTheme)
        {
            if (NavigationBar is null)
                return;

            if (isLightTheme(preferredTheme))
            {
                NavigationBar.Layer.ShadowColor = UIColor.Gray.CGColor;
                NavigationBar.Layer.ShadowOffset = new CGSize();
                NavigationBar.Layer.ShadowOpacity = 1;
            }
            else if (isDarkTheme(preferredTheme))
            {
                NavigationBar.Layer.ShadowColor = UIColor.White.CGColor;
                NavigationBar.Layer.ShadowOffset = new CGSize(0, -3);
                NavigationBar.Layer.ShadowOpacity = 0;
            }

            static bool isLightTheme(PreferredTheme theme) => theme is PreferredTheme.Light || theme is PreferredTheme.Default && Xamarin.Forms.Application.Current?.RequestedTheme is OSAppTheme.Light;
            static bool isDarkTheme(PreferredTheme theme) => theme is PreferredTheme.Dark || theme is PreferredTheme.Default && Xamarin.Forms.Application.Current?.RequestedTheme is OSAppTheme.Dark;
        }
    }
}
