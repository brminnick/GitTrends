using System.Diagnostics;
using Autofac;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace GitTrends
{
    public class App : Xamarin.Forms.Application
    {
        public App()
        {
            EnableDebugRainbows(false);

            FFImageLoading.ImageService.Instance.Initialize(new FFImageLoading.Config.Configuration
            {
                HttpHeadersTimeout = 60
            });

            using (var scope = ContainerService.Container.BeginLifetimeScope())
            {
                MainPage = new BaseNavigationPage(scope.Resolve<RepositoryPage>());
            }

            On<iOS>().SetHandleControlUpdatesOnMainThread(true);
        }

        [Conditional("DEBUG")]
        void EnableDebugRainbows(bool shouldUseDebugRainbows)
        {
            Resources.Add(new Style(typeof(ContentPage))
            {
                ApplyToDerivedTypes = true,
                Setters = {
                 new Setter
                 {
                     Property = Xamarin.Forms.DebugRainbows.DebugRainbow.IsDebugProperty,
                     Value = shouldUseDebugRainbows
                 }
             }
            });
        }
    }
}
