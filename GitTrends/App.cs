using System;
using System.Diagnostics;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using Autofac;
using Shiny;
using Shiny.Notifications;
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

            ClearBageNotifications().SafeFireAndForget(ex => _analyticsService.Report(ex));

#if !DEBUG
            RegisterBackgroundFetch().SafeFireAndForget(ex => _analyticsService.Report(ex));
#endif
        }

        protected override void OnResume()
        {
            base.OnResume();

            _analyticsService.Track("App Resumed");

            SetTheme();

            ClearBageNotifications().SafeFireAndForget(ex => _analyticsService.Report(ex));
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

        async Task RegisterBackgroundFetch()
        {
            using var scope = ContainerService.Container.BeginLifetimeScope();
            var backgroundFetchService = scope.Resolve<BackgroundFetchService>();

            await backgroundFetchService.Register().ConfigureAwait(false);
        }

        async Task ClearBageNotifications()
        {
            var notificationService = ShinyHost.Resolve<INotificationManager>();
            var accessState = await notificationService.RequestAccess();

            //INotificationManager.Badge Crashes on iOS
            if (accessState is AccessState.Available && Device.RuntimePlatform is Device.iOS)
                await DependencyService.Get<IEnvironment>().SetiOSBadgeCount(0).ConfigureAwait(false);
            else if (accessState is AccessState.Available)
                notificationService.Badge = 0;
        }

        void OnThemeChanged(Theme newTheme) => _themeChangedEventManager.HandleEvent(this, newTheme, nameof(ThemeChanged));
    }
}
