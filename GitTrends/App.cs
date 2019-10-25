using System;
using System.Diagnostics;
using AsyncAwaitBestPractices;
using Autofac;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace GitTrends
{
    public class App : Xamarin.Forms.Application
    {
        readonly WeakEventManager<Theme> _themeChangedEventManager = new WeakEventManager<Theme>();
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

        public event EventHandler<Theme> ThemeChanged
        {
            add => _themeChangedEventManager.AddEventHandler(value);
            remove => _themeChangedEventManager.RemoveEventHandler(value);
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
            var operatingSystemTheme = DependencyService.Get<IEnvironment>().GetOperatingSystemTheme();

            BaseTheme preferedTheme = operatingSystemTheme switch
            {
                Theme.Light => new LightTheme(),
                Theme.Dark => new DarkTheme(),
                _ => throw new NotSupportedException()
            };

            if (Resources.GetType() != preferedTheme.GetType())
            {
                Resources = preferedTheme;

                EnableDebugRainbows(false);

                OnThemeChanged(operatingSystemTheme);
            }
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

        void OnThemeChanged(Theme newTheme) => _themeChangedEventManager.HandleEvent(this, newTheme, nameof(ThemeChanged));
    }
}
