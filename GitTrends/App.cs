using System;
using System.Diagnostics;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using Autofac;
using Shiny;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace GitTrends
{
    public class App : Xamarin.Forms.Application
    {
        readonly WeakEventManager _resumedEventManager = new WeakEventManager();
        readonly AnalyticsService _analyticsService;

        public App()
        {
            Device.SetFlags(new[] { "Markup_Experimental", "IndicatorView_Experimental", "AppTheme_Experimental" });

            using var scope = ContainerService.Container.BeginLifetimeScope();
            _analyticsService = scope.Resolve<AnalyticsService>();

            MainPage = scope.Resolve<SplashScreenPage>();

            On<iOS>().SetHandleControlUpdatesOnMainThread(true);

            SetAppTheme();
            Current.RequestedThemeChanged += HandleRequestedThemeChanged;
        }

        public event EventHandler Resumed
        {
            add => _resumedEventManager.AddEventHandler(value);
            remove => _resumedEventManager.RemoveEventHandler(value);
        }

        protected override void OnStart()
        {
            base.OnStart();

            _analyticsService.Track("App Started");

            ClearBageNotifications().SafeFireAndForget(ex => _analyticsService.Report(ex));
        }

        protected override void OnResume()
        {
            base.OnResume();

            OnResumed();

            _analyticsService.Track("App Resumed");

            ClearBageNotifications().SafeFireAndForget(ex => _analyticsService.Report(ex));
        }

        protected override void OnSleep()
        {
            base.OnSleep();

            SetAppTheme();

            _analyticsService.Track("App Backgrounded");
        }

        void HandleRequestedThemeChanged(object sender, AppThemeChangedEventArgs e) => SetAppTheme();

        void SetAppTheme()
        {
            var operatingSystemTheme = Current.RequestedTheme;

            BaseTheme preferedTheme = operatingSystemTheme switch
            {
                AppTheme.Dark => new DarkTheme(),
                _ => new LightTheme(),
            };

            if (Resources.GetType() != preferedTheme.GetType())
            {
                Resources = preferedTheme;

                EnableDebugRainbows(false);
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
                    },
                    new Setter
                    {
                        Property = Xamarin.Forms.DebugRainbows.DebugRainbow.ShowGridProperty,
                        Value = shouldUseDebugRainbows
                    }
                }
            });
        }

        ValueTask ClearBageNotifications()
        {
            using var scope = ContainerService.Container.BeginLifetimeScope();
            var notificationService = scope.Resolve<NotificationService>();

            return notificationService.SetAppBadgeCount(0);
        }

        void OnResumed() => _resumedEventManager.HandleEvent(this, EventArgs.Empty, nameof(Resumed));
    }
}
