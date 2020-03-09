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
        readonly AnalyticsService _analyticsService;

        public App()
        {
            Device.SetFlags(new[] { "Markup_Experimental" });

            FFImageLoading.ImageService.Instance.Initialize(new FFImageLoading.Config.Configuration
            {
                HttpHeadersTimeout = 60
            });

            using var scope = ContainerService.Container.BeginLifetimeScope();
            _analyticsService = scope.Resolve<AnalyticsService>();

            MainPage = scope.Resolve<SplashScreenPage>();

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

            _analyticsService.Track("App Started");

            SetTheme();
        }

        protected override void OnResume()
        {
            base.OnResume();

            _analyticsService.Track("App Resumed");

            SetTheme();
        }

        protected override void OnSleep()
        {
            base.OnSleep();

            _analyticsService.Track("App Backgrounded");
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
                        Property = Xamarin.Forms.DebugRainbows.DebugRainbow.ShowColorsProperty,
                        Value = shouldUseDebugRainbows
                    }
                }
            });
        }

        void OnThemeChanged(Theme newTheme) => _themeChangedEventManager.HandleEvent(this, newTheme, nameof(ThemeChanged));
    }
}
