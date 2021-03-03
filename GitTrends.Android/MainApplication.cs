using System;
using System.Net;
using Android.App;
using Android.Runtime;
using Shiny;
using Shiny.Notifications;
using Xamarin.Android.Net;

namespace GitTrends.Droid
{
#if AppStore
    [Application(Debuggable = false)]
#else
    [Application(Debuggable = true)]
#endif
    public partial class MainApplication : Application
    {
        public MainApplication(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer)
        {

        }

        public override void OnCreate()
        {
            base.OnCreate();

            AndroidOptions.DefaultSmallIconResourceName = nameof(Resource.Drawable.icon);
            AndroidOptions.DefaultColorResourceName = nameof(Resource.Color.colorPrimary);
            AndroidOptions.DefaultChannel = nameof(GitTrends);
            AndroidOptions.DefaultChannelDescription = "GitTrends Notifications";
            AndroidOptions.DefaultLaunchActivityFlags = AndroidActivityFlags.FromBackground;
            AndroidOptions.DefaultNotificationImportance = AndroidNotificationImportance.High;
            AndroidShinyHost.Init(this, platformBuild: services => services.UseNotifications());

            Xamarin.Essentials.Platform.Init(this);

            FFImageLoading.Forms.Platform.CachedImageRenderer.Init(true);
            FFImageLoading.Forms.Platform.CachedImageRenderer.InitImageViewHandler();
            var ignore = typeof(FFImageLoading.Svg.Forms.SvgCachedImage);

            FFImageLoading.ImageService.Instance.Initialize(new FFImageLoading.Config.Configuration
            {
                HttpHeadersTimeout = 60,
                HttpClient = new System.Net.Http.HttpClient(new AndroidClientHandler
                {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                })
            });
        }
    }
}
