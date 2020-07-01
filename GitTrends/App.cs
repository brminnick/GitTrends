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
        readonly LanguageService _languageService;

        public App(IAnalyticsService analyticsService,
                    INotificationService notificationService,
                    ThemeService themeService,
                    SplashScreenPage splashScreenPage,
                    LanguageService languageService)
        {
            Device.SetFlags(new[] { "Markup_Experimental", "IndicatorView_Experimental", "AppTheme_Experimental" });

            InitializeEssentialServices(themeService, notificationService, languageService);

            _analyticsService = analyticsService;
            _languageService = languageService;

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

            _analyticsService.Track("App Started", nameof(LanguageService.PreferredLanguage), _languageService.PreferredLanguage ?? "null");

            await ClearBageNotifications();
        }

        protected override async void OnResume()
        {
            base.OnResume();

            OnResumed();

            _analyticsService.Track("App Resumed", nameof(LanguageService.PreferredLanguage), _languageService.PreferredLanguage ?? "null");

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

        async void InitializeEssentialServices(ThemeService themeService, INotificationService notificationService, LanguageService languageService)
        {
            await themeService.Initialize().ConfigureAwait(false);
            notificationService.Initialize();
            languageService.Initialize();
        }

        void OnResumed() => _resumedEventManager.HandleEvent(this, EventArgs.Empty, nameof(Resumed));
    }
}
