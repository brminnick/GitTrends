using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace GitTrends
{
    public class App : Xamarin.Forms.Application
    {
        public App()
        {
            FFImageLoading.ImageService.Instance.Initialize(new FFImageLoading.Config.Configuration
            {
                HttpHeadersTimeout = 60
            });

            MainPage = new BaseNavigationPage(new RepositoryPage());

            On<iOS>().SetHandleControlUpdatesOnMainThread(true);
        }
    }
}
