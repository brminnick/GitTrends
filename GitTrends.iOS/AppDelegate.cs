using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using Autofac;
using Foundation;
using GitTrends.Shared;
using Microsoft.Azure.NotificationHubs;
using Shiny;
using UIKit;

namespace GitTrends.iOS
{
    [Register(nameof(AppDelegate))]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        public override bool FinishedLaunching(UIApplication uiApplication, NSDictionary launchOptions)
        {
            iOSShinyHost.Init(platformBuild: services => services.UseNotifications());

            global::Xamarin.Forms.Forms.Init();
            Sharpnado.MaterialFrame.iOS.iOSMaterialFrameRenderer.Init();

            Syncfusion.SfChart.XForms.iOS.Renderers.SfChartRenderer.Init();
            Syncfusion.XForms.iOS.Buttons.SfSegmentedControlRenderer.Init();

            FFImageLoading.Forms.Platform.CachedImageRenderer.Init();
            FFImageLoading.Forms.Platform.CachedImageRenderer.InitImageSourceHandler();
            var ignore = typeof(FFImageLoading.Svg.Forms.SvgCachedImage);

            FFImageLoading.ImageService.Instance.Initialize(new FFImageLoading.Config.Configuration
            {
                HttpHeadersTimeout = 60,
                HttpClient = new HttpClient(new NSUrlSessionHandler())
            });

            PrintFontNamesToConsole();

            var themeService = ContainerService.Container.Resolve<ThemeService>();
            var languageService = ContainerService.Container.Resolve<LanguageService>();
            var splashScreenPage = ContainerService.Container.Resolve<SplashScreenPage>();
            var analyticsService = ContainerService.Container.Resolve<IAnalyticsService>();
            var notificationService = ContainerService.Container.Resolve<NotificationService>();
            var deviceNotificationsService = ContainerService.Container.Resolve<IDeviceNotificationsService>();

            LoadApplication(new App(themeService, languageService, analyticsService, splashScreenPage, notificationService, deviceNotificationsService));

            if (launchOptions?.ContainsKey(UIApplication.LaunchOptionsLocalNotificationKey) is true)
                HandleLocalNotification((UILocalNotification)launchOptions[UIApplication.LaunchOptionsLocalNotificationKey]).SafeFireAndForget(ex => analyticsService.Report(ex));

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

                var gitHubAuthenticationService = ContainerService.Container.Resolve<GitHubAuthenticationService>();
                await gitHubAuthenticationService.AuthorizeSession(callbackUri, CancellationToken.None).ConfigureAwait(false);
            }

            static void onException(Exception e)
            {
                ContainerService.Container.Resolve<IAnalyticsService>().Report(e);
            }
        }

        public override async void ReceivedRemoteNotification(UIApplication application, NSDictionary userInfo)
        {
            var backgroundFetchService = ContainerService.Container.Resolve<BackgroundFetchService>();

            await Task.WhenAll(backgroundFetchService.CleanUpDatabase(), backgroundFetchService.NotifyTrendingRepositories(CancellationToken.None)).ConfigureAwait(false);
        }

        public override async void ReceivedLocalNotification(UIApplication application, UILocalNotification notification) =>
            await HandleLocalNotification(notification).ConfigureAwait(false);

        public override async void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
        {
            var analyticsService = ContainerService.Container.Resolve<IAnalyticsService>();
            var notificationService = ContainerService.Container.Resolve<NotificationService>();

            var notificationHubInformation = await notificationService.GetNotificationHubInformation().ConfigureAwait(false);

            var tokenAsString = BitConverter.ToString(deviceToken.ToArray()).Replace("-", "").Replace("\"", "");

#if AppStore
            var hubClient = NotificationHubClient.CreateClientFromConnectionString(notificationHubInformation.ConnectionString, notificationHubInformation.Name);
#else
            if (notificationHubInformation.IsEmpty())
                return;

            var hubClient = NotificationHubClient.CreateClientFromConnectionString(notificationHubInformation.ConnectionString_Debug, notificationHubInformation.Name_Debug);
#endif
            try
            {
                var doesRegistrationExist = await hubClient.RegistrationExistsAsync(tokenAsString).ConfigureAwait(false);

                if (!doesRegistrationExist)
                    await hubClient.CreateAppleNativeRegistrationAsync(tokenAsString).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                analyticsService.Report(e);
            }
        }

        Task HandleLocalNotification(in UILocalNotification notification)
        {
            var notificationService = ContainerService.Container.Resolve<NotificationService>();

            if (notification is null || notification.AlertTitle is null || notification.AlertBody is null)
                return Task.CompletedTask;

            return notificationService.HandleNotification(notification.AlertTitle, notification.AlertBody, (int)notification.ApplicationIconBadgeNumber);
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
