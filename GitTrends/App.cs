using System;
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
        readonly WeakEventManager _resumedEventManager = new WeakEventManager();
        readonly AnalyticsService _analyticsService;

        public App()
        {
            Device.SetFlags(new[] { "Markup_Experimental", "IndicatorView_Experimental", "AppTheme_Experimental" });

            InitializeEssentialServices();

            using var scope = ContainerService.Container.BeginLifetimeScope();
            _analyticsService = scope.Resolve<AnalyticsService>();

            MainPage = scope.Resolve<SplashScreenPage>();

            On<iOS>().SetHandleControlUpdatesOnMainThread(true);
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

            _analyticsService.Track("App Resumed");

            ClearBageNotifications().SafeFireAndForget(ex => _analyticsService.Report(ex));
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

        void InitializeEssentialServices()
        {
            using var scope = ContainerService.Container.BeginLifetimeScope();

            var themeService = scope.Resolve<ThemeService>();
            var notificationService = DependencyService.Resolve<INotificationService>();
        }

        void OnResumed() => _resumedEventManager.HandleEvent(this, EventArgs.Empty, nameof(Resumed));
    }
}
