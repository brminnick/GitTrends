using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using Autofac;
using BackgroundTasks;
using Foundation;
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
            Xamarin.Forms.FormsMaterial.Init();

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
                await gitHubAuthenticationService.AuthorizeSession(callbackUri, CancellationToken.None).ConfigureAwait(false);
            }

            static void onException(Exception e)
            {
                using var containerScope = ContainerService.Container.BeginLifetimeScope();
                containerScope.Resolve<AnalyticsService>().Report(e);
            }
        }

        public override async void ReceivedLocalNotification(UIApplication application, UILocalNotification notification) =>
            await HandleLocalNotification(notification).ConfigureAwait(false);

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
