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

            LoadApplication(new App());

            TryHandleOpenedFromUri(Intent.Data);
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
            using var containerScope = ContainerService.Container.BeginLifetimeScope();

            try
            {
                var gitHubAuthenticationService = containerScope.Resolve<GitHubAuthenticationService>();

                await gitHubAuthenticationService.AuthorizeSession(new Uri(callbackUri.ToString()), CancellationToken.None).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                containerScope.Resolve<IAnalyticsService>().Report(ex);
            }
        }

        async void TryHandleOpenedFromNotification(Intent? intent)
        {
            try
            {
                if (intent?.GetStringExtra("ShinyNotification") is string notificationString)
                {
                    var notification = JsonConvert.DeserializeObject<Shiny.Notifications.Notification>(notificationString);

                    using var scope = ContainerService.Container.BeginLifetimeScope();
                    var analyticsService = scope.Resolve<IAnalyticsService>();

                    var notificationService = scope.Resolve<NotificationService>();

                    await notificationService.HandleReceivedLocalNotification(notification.Title ?? string.Empty,
                                                                                notification.Message ?? string.Empty,
                                                                                notification.BadgeCount ?? 0).ConfigureAwait(false);
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