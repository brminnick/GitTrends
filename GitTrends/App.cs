using System;
using System.Diagnostics;
using Autofac;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace GitTrends
{
    public class App : Xamarin.Forms.Application
    {
        readonly bool _isInitiatedByCallBackUri;

        public App(bool isInitiatedByCallBackUri = false)
        {
            _isInitiatedByCallBackUri = isInitiatedByCallBackUri;

            FFImageLoading.ImageService.Instance.Initialize(new FFImageLoading.Config.Configuration
            {
                HttpHeadersTimeout = 60
            });

            //Initialize a blank page in the Constructor because the Theme cannot be set on iOS until a UIViewController has been initialized
            MainPage = new Xamarin.Forms.Page();

            On<iOS>().SetHandleControlUpdatesOnMainThread(true);
        }

        protected override void OnStart()
        {
            base.OnStart();

            SetTheme();

            //Initialize the MainPage in OnStart() because the Theme cannot be set on iOS until a UIViewController has been initialized
            using (var scope = ContainerService.Container.BeginLifetimeScope())
            {
                MainPage = new BaseNavigationPage(scope.Resolve<RepositoryPage>(new TypedParameter(typeof(bool), _isInitiatedByCallBackUri)));
            }
        }

        protected override void OnResume()
        {
            base.OnResume();

            SetTheme();
        }

        void SetTheme()
        {
            Resources = DependencyService.Get<IEnvironment>().GetOperatingSystemTheme() switch
            {
                Theme.Light => new LightTheme(),
                Theme.Dark => throw new NotImplementedException(),
                _ => throw new NotSupportedException()
            };

            EnableDebugRainbows(false);
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
