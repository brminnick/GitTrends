using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace GitTrends
{
    public class App : Xamarin.Forms.Application
    {
        public App()
        {
            MainPage = new BaseNavigationPage(new RepositoryPage());

            On<iOS>().SetHandleControlUpdatesOnMainThread(true);
        }
    }
}
