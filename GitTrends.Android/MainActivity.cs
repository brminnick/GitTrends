using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using AsyncAwaitBestPractices;
using GitTrends.Mobile.Shared;
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

            SyncFusionService.Initialize();

            LoadApplication(new App());

            executeCallbackUri().SafeFireAndForget(false);

            async Task executeCallbackUri()
            {
                if (Intent?.Data is Android.Net.Uri callbackUri)
                {
                    await GitHubAuthenticationService.AuthorizeSession(new System.Uri(callbackUri.ToString())).ConfigureAwait(false);

                    if (Xamarin.Forms.Application.Current.MainPage is BaseNavigationPage navigationPage
                        && navigationPage.CurrentPage is RepositoryPage)
                    {
                        await Xamarin.Forms.Device.InvokeOnMainThreadAsync(() => navigationPage.Navigation.PushAsync(new ProfilePage())).ConfigureAwait(false);
                    }
                }
            }
        }
    }
}