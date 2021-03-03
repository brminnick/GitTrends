using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace GitTrends
{
    public class BaseNavigationPage : Xamarin.Forms.NavigationPage
    {
        public BaseNavigationPage(Xamarin.Forms.Page root) : base(root)
        {
            this.DynamicResources((BarTextColorProperty, nameof(BaseTheme.NavigationBarTextColor)),
                                    (BackgroundColorProperty, nameof(BaseTheme.PageBackgroundColor)),
                                    (BarBackgroundColorProperty, nameof(BaseTheme.NavigationBarBackgroundColor)));

            On<iOS>().SetPrefersLargeTitles(true);
            On<iOS>().SetModalPresentationStyle(UIModalPresentationStyle.FormSheet);
        }
    }
}
