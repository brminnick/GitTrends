using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using Autofac;
using GitTrends.Shared;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace GitTrends
{
    public class App : Xamarin.Forms.Application
    {
        readonly static WeakEventManager _resumedEventManager = new();

        readonly IAnalyticsService _analyticsService;
        readonly NotificationService _notificationService;
        readonly AppInitializationService _appInitializationService;

        public App(LanguageService languageService,
                    SplashScreenPage splashScreenPage,
                    IAnalyticsService analyticsService,
                    NotificationService notificationService,
                    AppInitializationService appInitializationService)
        {
            _analyticsService = analyticsService;
            _notificationService = notificationService;
            _appInitializationService = appInitializationService;

            analyticsService.Track("App Initialized", new Dictionary<string, string>
            {
                { nameof(LanguageService.PreferredLanguage), languageService.PreferredLanguage ?? "default" },
                { nameof(CultureInfo.CurrentUICulture), CultureInfo.CurrentUICulture.TwoLetterISOLanguageName }
            });

            MainPage = splashScreenPage;

            On<iOS>().SetHandleControlUpdatesOnMainThread(true);
        }

        public static event EventHandler Resumed
        {
            add => _resumedEventManager.AddEventHandler(value);
            remove => _resumedEventManager.RemoveEventHandler(value);
        }

        protected override async void OnStart()
        {
            base.OnStart();

            _analyticsService.Track("App Started");

            await ClearBageNotifications();

            var appInitializationCancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            await _appInitializationService.InitializeApp(appInitializationCancellationTokenSource.Token);
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

        ValueTask ClearBageNotifications() => _notificationService.SetAppBadgeCount(0);

        void OnResumed() => _resumedEventManager.RaiseEvent(this, EventArgs.Empty, nameof(Resumed));
    }
}
