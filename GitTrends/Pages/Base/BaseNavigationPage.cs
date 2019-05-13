using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace GitTrends
{
    public class BaseNavigationPage : Xamarin.Forms.NavigationPage
    {
        public BaseNavigationPage(Xamarin.Forms.Page root) : base(root)
        {
            BarBackgroundColor = ColorConstants.MediumBlue;
            BarTextColor = Color.White;

            On<iOS>().PrefersLargeTitles();
        }
    }
}
