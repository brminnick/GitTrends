using System;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Autofac;
using GitTrends.Shared;
using Newtonsoft.Json;
using Shiny;
using Xamarin.Forms;

namespace GitTrends.Droid
{
    [Activity(Label = "GitTrends", Icon = "@mipmap/icon", RoundIcon = "@mipmap/icon_round", Theme = "@style/LaunchTheme", LaunchMode = LaunchMode.SingleTop, MainLauncher = true, ScreenOrientation = ScreenOrientation.Portrait)]
    [IntentFilter(new string[] { Intent.ActionView }, Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable }, DataSchemes = new[] { "gittrends" })]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            AndroidShinyHost.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.SetTheme(Resource.Style.MainTheme);
            base.OnCreate(savedInstanceState);

            Forms.Init(this, savedInstanceState);

            var themeService = ContainerService.Container.Resolve<ThemeService>();
            var languageService = ContainerService.Container.Resolve<LanguageService>();
            var splashScreenPage = ContainerService.Container.Resolve<SplashScreenPage>();
            var analyticsService = ContainerService.Container.Resolve<IAnalyticsService>();
            var notificationService = ContainerService.Container.Resolve<NotificationService>();
            var deviceNotificationsService = ContainerService.Container.Resolve<IDeviceNotificationsService>();

            LoadApplication(new App(themeService, languageService, analyticsService, splashScreenPage, notificationService, deviceNotificationsService));

            TryHandleOpenedFromUri(Intent?.Data);
            TryHandleOpenedFromNotification(Intent);
        }

        protected override async void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);

            if (intent?.Data is Android.Net.Uri callbackUri)
            {
                await AuthorizeGitHubSession(callbackUri).ConfigureAwait(false);
            }

            TryHandleOpenedFromNotification(intent);
        }

        static async Task AuthorizeGitHubSession(Android.Net.Uri callbackUri)
        {
            try
            {
                var gitHubAuthenticationService = ContainerService.Container.Resolve<GitHubAuthenticationService>();

                await gitHubAuthenticationService.AuthorizeSession(new Uri(callbackUri.ToString()), CancellationToken.None).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                ContainerService.Container.Resolve<IAnalyticsService>().Report(ex);
            }
        }

        async void TryHandleOpenedFromNotification(Intent? intent)
        {
            try
            {
                if (intent?.GetStringExtra("ShinyNotification") is string notificationString)
                {
                    var notification = JsonConvert.DeserializeObject<Shiny.Notifications.Notification>(notificationString);

                    var analyticsService = ContainerService.Container.Resolve<IAnalyticsService>();
                    var notificationService = ContainerService.Container.Resolve<NotificationService>();

                    if (notification.Title is string notificationTitle
                        && notification.Message is string notificationMessage
                        && notification.BadgeCount is int badgeCount
                        && badgeCount > 0)
                    {
                        await notificationService.HandleNotification(notificationTitle, notificationMessage, badgeCount).ConfigureAwait(false);
                    }
                }
            }
            catch (ObjectDisposedException)
            {

            }
        }

        async void TryHandleOpenedFromUri(Android.Net.Uri? callbackUri)
        {
            if (callbackUri != null)
            {
                await AuthorizeGitHubSession(callbackUri).ConfigureAwait(false);
            }
        }
    }
}