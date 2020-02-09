using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using AsyncAwaitBestPractices;
using Autofac;
using Plugin.CurrentActivity;

namespace GitTrends.Droid
{
    [Activity(Label = "GitTrends", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    [IntentFilter(new string[] { Intent.ActionView }, Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable }, DataSchemes = new[] { "gittrends" })]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            CrossCurrentActivity.Current.Init(this, savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);

            FFImageLoading.Forms.Platform.CachedImageRenderer.Init(true);
            FFImageLoading.Forms.Platform.CachedImageRenderer.InitImageViewHandler();
            var ignore = typeof(FFImageLoading.Svg.Forms.SvgCachedImage);

            using var containerScope = ContainerService.Container.BeginLifetimeScope();

            var syncFusionService = containerScope.Resolve<SyncFusionService>();
            syncFusionService.Initialize().SafeFireAndForget(onException: ex => System.Diagnostics.Debug.WriteLine(ex));

            if (Intent?.Data is Android.Net.Uri callbackUri)
            {
                LoadApplication(new App(true));
                ExecuteCallbackUri(callbackUri);
            }
            else
            {
                LoadApplication(new App(false));
            }
        }

        void ExecuteCallbackUri(Android.Net.Uri callbackUri)
        {
            var navigationPage = (Xamarin.Forms.NavigationPage)Xamarin.Forms.Application.Current.MainPage;
            navigationPage.Pushed += HandlePushed;

            async void HandlePushed(object sender, Xamarin.Forms.NavigationEventArgs e)
            {
                if (e.Page is SettingsPage)
                {
                    navigationPage.Pushed -= HandlePushed;

                    try
                    {
                        using var containerScope = ContainerService.Container.BeginLifetimeScope();
                        var gitHubAuthenticationService = containerScope.Resolve<GitHubAuthenticationService>();

                        await gitHubAuthenticationService.AuthorizeSession(new Uri(callbackUri.ToString())).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex);
                    }
                }
            }
        }
    }
}