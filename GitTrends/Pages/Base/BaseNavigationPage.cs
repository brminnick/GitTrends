using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace GitTrends
{
    public class BaseNavigationPage : Xamarin.Forms.NavigationPage
    {
        public BaseNavigationPage(Xamarin.Forms.Page root) : base(root)
        {
            SetDynamicResource(BarTextColorProperty, nameof(BaseTheme.NavigationBarTextColor));
            SetDynamicResource(BackgroundColorProperty, nameof(BaseTheme.PageBackgroundColor));
            SetDynamicResource(BarBackgroundColorProperty, nameof(BaseTheme.NavigationBarBackgroundColor));

            On<iOS>().SetPrefersLargeTitles(true);
            On<iOS>().SetModalPresentationStyle(UIModalPresentationStyle.FormSheet);
        }
    }
}
