using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using AsyncAwaitBestPractices;
using Autofac;
using Newtonsoft.Json;
using Shiny;

namespace GitTrends.Droid
{
    [Activity(Label = "GitTrends", Icon = "@mipmap/icon", RoundIcon = "@mipmap/icon_round", Theme = "@style/LaunchTheme", LaunchMode = LaunchMode.SingleTop, MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
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

            Xamarin.Forms.Forms.Init(this, savedInstanceState);

            var app = new App();

            TryHandleOpenedFromUri(app, Intent.Data);
            TryHandleOpenedFromNotification(Intent);

            LoadApplication(app);
        }

        protected override async void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);

            if (intent?.Data is Android.Net.Uri callbackUri)
            {
                await NavigateToSettingsPage().ConfigureAwait(false);
                await AuthorizeGitHubSession(callbackUri).ConfigureAwait(false);
            }

            TryHandleOpenedFromNotification(intent);
        }

        static async ValueTask NavigateToSettingsPage()
        {
            var navigationPage = (Xamarin.Forms.NavigationPage)Xamarin.Forms.Application.Current.MainPage;

            if (navigationPage.CurrentPage.GetType() != typeof(SettingsPage))
            {
                using var containerScope = ContainerService.Container.BeginLifetimeScope();
                var settingsPage = containerScope.Resolve<SettingsPage>();

                await Xamarin.Essentials.MainThread.InvokeOnMainThreadAsync(() => navigateToSettingsPage(navigationPage, settingsPage)).ConfigureAwait(false);
            }

            static async Task navigateToSettingsPage(Xamarin.Forms.NavigationPage mainNavigationPage, SettingsPage settingsPage)
            {
                await mainNavigationPage.PopToRootAsync();
                await mainNavigationPage.PushAsync(settingsPage);
            }
        }

        static async Task AuthorizeGitHubSession(Android.Net.Uri callbackUri)
        {
            using var containerScope = ContainerService.Container.BeginLifetimeScope();

            try
            {
                var gitHubAuthenticationService = containerScope.Resolve<GitHubAuthenticationService>();
                await gitHubAuthenticationService.AuthorizeSession(new Uri(callbackUri.ToString())).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                containerScope.Resolve<AnalyticsService>().Report(ex);
            }
        }

        async void TryHandleOpenedFromNotification(Intent? intent)
        {
            if (intent?.GetStringExtra("ShinyNotification") is string notificationString)
            {
                var notification = JsonConvert.DeserializeObject<Shiny.Notifications.Notification>(notificationString);

                using var scope = ContainerService.Container.BeginLifetimeScope();
                var analyticsService = scope.Resolve<AnalyticsService>();

                var notificationService = scope.Resolve<NotificationService>();

                await notificationService.HandleReceivedLocalNotification(notification.Title ?? string.Empty,
                                                                            notification.Message ?? string.Empty,
                                                                            notification.BadgeCount ?? 0).ConfigureAwait(false);
            }
        }

        void TryHandleOpenedFromUri(App app, Android.Net.Uri? callbackUri)
        {
            if (callbackUri != null)
            {
                //Wait for Application.MainPage to load before handling the callbackUri
                app.PageAppearing += HandlePageAppearing;

                async void HandlePageAppearing(object sender, Xamarin.Forms.Page page)
                {
                    if (page is SettingsPage)
                    {
                        app.PageAppearing -= HandlePageAppearing;
                        await AuthorizeGitHubSession(callbackUri).ConfigureAwait(false);
                    }
                    else if (page is RepositoryPage)
                    {
                        await NavigateToSettingsPage().ConfigureAwait(false);
                    }
                }
            }
        }
    }
}