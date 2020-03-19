using System;
using System.Diagnostics;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using Autofac;
using Foundation;
using Shiny;
using UIKit;

namespace GitTrends.iOS
{
    [Register(nameof(AppDelegate))]
    public class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        public override bool FinishedLaunching(UIApplication uiApplication, NSDictionary launchOptions)
        {
            iOSShinyHost.Init(platformBuild: services => services.UseNotifications());

            global::Xamarin.Forms.Forms.Init();
            Syncfusion.SfChart.XForms.iOS.Renderers.SfChartRenderer.Init();
            Syncfusion.XForms.iOS.Buttons.SfSegmentedControlRenderer.Init();

            FFImageLoading.Forms.Platform.CachedImageRenderer.Init();
            FFImageLoading.Forms.Platform.CachedImageRenderer.InitImageSourceHandler();
            var ignore = typeof(FFImageLoading.Svg.Forms.SvgCachedImage);

#if !AppStore
            Xamarin.Calabash.Start();
#endif

            PrintFontNamesToConsole();

            LoadApplication(new App());

            if (launchOptions?.ContainsKey(UIApplication.LaunchOptionsLocalNotificationKey) is true)
                HandleLocalNotification((UILocalNotification)launchOptions[UIApplication.LaunchOptionsLocalNotificationKey]).SafeFireAndForget(ex => ContainerService.Container.Resolve<AnalyticsService>().Report(ex));

            return base.FinishedLaunching(uiApplication, launchOptions);
        }


        public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
        {
            var callbackUri = new Uri(url.AbsoluteString);

            HandleCallbackUri(callbackUri).SafeFireAndForget(onException);

            return true;

            static async Task HandleCallbackUri(Uri callbackUri)
            {
                await ViewControllerServices.CloseSFSafariViewController().ConfigureAwait(false);

                using var scope = ContainerService.Container.BeginLifetimeScope();

                var gitHubAuthenticationService = scope.Resolve<GitHubAuthenticationService>();
                await gitHubAuthenticationService.AuthorizeSession(callbackUri).ConfigureAwait(false);
            }

            static void onException(Exception e)
            {
                using var containerScope = ContainerService.Container.BeginLifetimeScope();
                containerScope.Resolve<AnalyticsService>().Report(e);
            }
        }

        public override async void ReceivedLocalNotification(UIApplication application, UILocalNotification notification)
        {
            await HandleLocalNotification(notification).ConfigureAwait(false);
        }

        public override void PerformFetch(UIApplication application, Action<UIBackgroundFetchResult> completionHandler)
        {
            Shiny.Jobs.JobManager.OnBackgroundFetch(completionHandler);
        }

#if !AppStore
        #region UI Test Back Door Methods
        [Preserve, Export(Mobile.Shared.BackdoorMethodConstants.SetGitHubUser + ":")]
        public async void SetGitHubUser(NSString accessToken)
        {
            using var scope = ContainerService.Container.BeginLifetimeScope();
            var backdoorService = scope.Resolve<UITestBackdoorService>();

            await backdoorService.SetGitHubUser(accessToken.ToString()).ConfigureAwait(false);
        }

        [Preserve, Export(Mobile.Shared.BackdoorMethodConstants.TriggerPullToRefresh + ":")]
        public async void TriggerRepositoriesPullToRefresh(NSString noValue)
        {
            using var scope = ContainerService.Container.BeginLifetimeScope();
            var backdoorService = scope.Resolve<UITestBackdoorService>();

            await backdoorService.TriggerPullToRefresh().ConfigureAwait(false);
        }

        [Preserve, Export(Mobile.Shared.BackdoorMethodConstants.GetVisibleCollection + ":")]
        public NSString GetVisibleRepositoryList(NSString noValue)
        {
            using var scope = ContainerService.Container.BeginLifetimeScope();
            var backdoorService = scope.Resolve<UITestBackdoorService>();

            var serializedCollection = Newtonsoft.Json.JsonConvert.SerializeObject(backdoorService.GetVisibleCollection());
            return new NSString(serializedCollection);
        }

        [Preserve, Export(Mobile.Shared.BackdoorMethodConstants.GetCurrentTrendsChartOption + ":")]
        public NSString GetCurrentTrendsChartOption(NSString noValue)
        {
            using var scope = ContainerService.Container.BeginLifetimeScope();
            var backdoorService = scope.Resolve<UITestBackdoorService>();

            var serializedTrendChartOption = Newtonsoft.Json.JsonConvert.SerializeObject(backdoorService.GetCurrentTrendsChartOption());
            return new NSString(serializedTrendChartOption);
        }

        [Preserve, Export(Mobile.Shared.BackdoorMethodConstants.IsTrendsSeriesVisible + ":")]
        public NSString IsTrendsSeriesVisible(NSString seriesLabel)
        {
            using var scope = ContainerService.Container.BeginLifetimeScope();
            var backdoorService = scope.Resolve<UITestBackdoorService>();

            var isSeriesVisible = backdoorService.IsTrendsSeriesVisible(seriesLabel.ToString());
            return new NSString(Newtonsoft.Json.JsonConvert.SerializeObject(isSeriesVisible));
        }
        #endregion
#endif

        Task HandleLocalNotification(UILocalNotification notification)
        {
            using var scope = ContainerService.Container.BeginLifetimeScope();
            var notificationService = scope.Resolve<NotificationService>();

            return notificationService.HandleReceivedLocalNotification(notification.AlertTitle, notification.AlertBody, (int)notification.ApplicationIconBadgeNumber);
        }

        [Conditional("DEBUG")]
        void PrintFontNamesToConsole()
        {
            foreach (var fontFamilyName in UIFont.FamilyNames)
            {
                Console.WriteLine(fontFamilyName);

                foreach (var fontName in UIFont.FontNamesForFamilyName(fontFamilyName))
                    Console.WriteLine($"={fontName}");
            }
        }
    }
}
