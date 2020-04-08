using System;
using System.Net;
using Android.App;
using Android.Runtime;
using Autofac;
using Shiny;
using Shiny.Notifications;
using Xamarin.Android.Net;

namespace GitTrends.Droid
{
    [Application]
    public class MainApplication : Application
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

            Plugin.CurrentActivity.CrossCurrentActivity.Current.Init(this);

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

#if !AppStore
        #region UI Test Back Door Methods
        [Preserve, Java.Interop.Export(Mobile.Shared.BackdoorMethodConstants.SetGitHubUser)]
        public async void SetGitHubUser(string accessToken)
        {
            using var scope = ContainerService.Container.BeginLifetimeScope();
            var backdoorService = scope.Resolve<UITestBackdoorService>();

            await backdoorService.SetGitHubUser(accessToken.ToString()).ConfigureAwait(false);
        }

        [Preserve, Java.Interop.Export(Mobile.Shared.BackdoorMethodConstants.TriggerPullToRefresh)]
        public async void TriggerRepositoriesPullToRefresh()
        {
            using var scope = ContainerService.Container.BeginLifetimeScope();
            var backdoorService = scope.Resolve<UITestBackdoorService>();

            await backdoorService.TriggerPullToRefresh().ConfigureAwait(false);
        }

        [Preserve, Java.Interop.Export(Mobile.Shared.BackdoorMethodConstants.GetVisibleCollection)]
        public string GetVisibleCollection()
        {
            using var scope = ContainerService.Container.BeginLifetimeScope();
            var backdoorService = scope.Resolve<UITestBackdoorService>();

            return Newtonsoft.Json.JsonConvert.SerializeObject(backdoorService.GetVisibleCollection());
        }

        [Preserve, Java.Interop.Export(Mobile.Shared.BackdoorMethodConstants.GetCurrentTrendsChartOption)]
        public string GetCurrentTrendsChartOption()
        {
            using var scope = ContainerService.Container.BeginLifetimeScope();
            var backdoorService = scope.Resolve<UITestBackdoorService>();

            return Newtonsoft.Json.JsonConvert.SerializeObject(backdoorService.GetCurrentTrendsChartOption());
        }

        [Preserve, Java.Interop.Export(Mobile.Shared.BackdoorMethodConstants.IsTrendsSeriesVisible)]
        public bool IsTrendsSeriesVisible(string seriesLabel)
        {
            using var scope = ContainerService.Container.BeginLifetimeScope();
            var backdoorService = scope.Resolve<UITestBackdoorService>();

            return backdoorService.IsTrendsSeriesVisible(seriesLabel);
        }

        [Preserve, Java.Interop.Export(Mobile.Shared.BackdoorMethodConstants.GetCurrentOnboardingPageNumber)]
        public string GetCurrentOnboardingPageNumber()
        {
            using var scope = ContainerService.Container.BeginLifetimeScope();
            var backdoorService = scope.Resolve<UITestBackdoorService>();

            return Newtonsoft.Json.JsonConvert.SerializeObject(backdoorService.GetCurrentOnboardingPageNumber());
        }

        [Preserve, Java.Interop.Export(Mobile.Shared.BackdoorMethodConstants.PopPage)]
        public async void PopPage()
        {
            using var scope = ContainerService.Container.BeginLifetimeScope();
            var backdoorService = scope.Resolve<UITestBackdoorService>();

            await backdoorService.PopPage().ConfigureAwait(false);
        }
        #endregion
#endif
    }
}
