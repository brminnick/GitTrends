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

        public App(IAnalyticsService analyticsService,
                    INotificationService notificationService,
                    ThemeService themeService,
                    SplashScreenPage splashScreenPage)
        {
            Device.SetFlags(new[] { "Markup_Experimental", "IndicatorView_Experimental", "AppTheme_Experimental" });

            InitializeEssentialServices(themeService, notificationService);

            _analyticsService = analyticsService;

            MainPage = splashScreenPage;

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

        async void InitializeEssentialServices(ThemeService themeService, INotificationService notificationService)
        {
            await themeService.Initialize().ConfigureAwait(false);
            notificationService.Initialize();
        }

        void OnResumed() => _resumedEventManager.HandleEvent(this, EventArgs.Empty, nameof(Resumed));
    }
}
