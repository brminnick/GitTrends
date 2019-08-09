using Autofac;
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

            using (var scope = ContainerService.Container.BeginLifetimeScope())
                MainPage = new BaseNavigationPage(scope.Resolve<RepositoryPage>());

            On<iOS>().SetHandleControlUpdatesOnMainThread(true);
        }
    }
}
