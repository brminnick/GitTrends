using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace GitTrends
{
    public class BaseNavigationPage : Xamarin.Forms.NavigationPage
    {
        public BaseNavigationPage(Xamarin.Forms.Page root) : base(root)
        {
            //Set Default Values for Navigation Bar Colors
            //These default values will be overridden by the Resource Dictionary when the theme changes
            BarBackgroundColor = Xamarin.Forms.Color.FromHex("1FAECE");
            BarTextColor = Xamarin.Forms.Color.White;

            SetDynamicResource(BarBackgroundColorProperty, nameof(BaseTheme.NavigationBarBackgroundColor));
            SetDynamicResource(BarTextColorProperty, nameof(BaseTheme.NavigationBarTextColor));
            SetDynamicResource(BackgroundColorProperty, nameof(BaseTheme.PageBackgroundColor));

            On<iOS>().SetPrefersLargeTitles(true);
            On<iOS>().SetModalPresentationStyle(UIModalPresentationStyle.FormSheet);
        }
    }
}
