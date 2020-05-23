using System;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using Autofac;
using GitTrends.Shared;
using Shiny;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace GitTrends
{
    public class App : Xamarin.Forms.Application
    {
        readonly WeakEventManager _resumedEventManager = new WeakEventManager();
        readonly IAnalyticsService _analyticsService;

        public App()
        {
            Device.SetFlags(new[] { "Markup_Experimental", "IndicatorView_Experimental", "AppTheme_Experimental" });

            InitializeEssentialServices();

            using var scope = ContainerService.Container.BeginLifetimeScope();
            _analyticsService = scope.Resolve<IAnalyticsService>();

            MainPage = scope.Resolve<SplashScreenPage>();

            On<iOS>().SetHandleControlUpdatesOnMainThread(true);
        }

        public event EventHandler Resumed
        {
            add => _resumedEventManager.AddEventHandler(value);
            remove => _resumedEventManager.RemoveEventHandler(value);
        }

        protected override async void OnStart()
        {
            base.OnStart();

            _analyticsService.Track("App Started");

            await ClearBageNotifications();
        }

        protected override async void OnResume()
        {
            base.OnResume();

            OnResumed();

            _analyticsService.Track("App Resumed");

            await ClearBageNotifications();
        }

        protected override void OnSleep()
        {
            base.OnSleep();

            _analyticsService.Track("App Backgrounded");
        }

        ValueTask ClearBageNotifications()
        {
            using var scope = ContainerService.Container.BeginLifetimeScope();
            var notificationService = scope.Resolve<NotificationService>();

            return notificationService.SetAppBadgeCount(0);
        }

        async void InitializeEssentialServices()
        {
            using var scope = ContainerService.Container.BeginLifetimeScope();

            var themeService = scope.Resolve<ThemeService>();
            await themeService.Initialize().ConfigureAwait(false);

            var notificationService = DependencyService.Resolve<INotificationService>();
            notificationService.Initialize();
        }

        void OnResumed() => _resumedEventManager.HandleEvent(this, EventArgs.Empty, nameof(Resumed));
    }
}
