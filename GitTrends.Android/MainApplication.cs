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
			this.ShinyOnCreate(new ShinyStartup(new DeviceNotificationsService_Android()));

			base.OnCreate();

			AndroidOptions.DefaultSmallIconResourceName = nameof(Resource.Drawable.icon);
			AndroidOptions.DefaultColorResourceName = nameof(Resource.Color.colorPrimary);
			AndroidOptions.DefaultLaunchActivityFlags = AndroidActivityFlags.FromBackground;

			Xamarin.Essentials.Platform.Init(this);

			FFImageLoading.Forms.Platform.CachedImageRenderer.Init(true);
			FFImageLoading.Forms.Platform.CachedImageRenderer.InitImageViewHandler();
			var ignore = typeof(FFImageLoading.Svg.Forms.SvgCachedImage);

			// Fix Java.Lang.IllegalStateException: Unbalanced enter/exit
			// https://github.com/luberda-molinet/FFImageLoading/issues/1492
			FFImageLoading.ImageService.Instance.Initialize(new FFImageLoading.Config.Configuration
			{
				HttpHeadersTimeout = 60,
				HttpReadTimeout = 60,
				DiskCacheDuration = TimeSpan.FromDays(14),
				TryToReadDiskCacheDurationFromHttpHeaders = false,
				ExecuteCallbacksOnUIThread = true,
				HttpClient = new System.Net.Http.HttpClient(new AndroidClientHandler
				{
					AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
				})
			});
		}
	}
}
