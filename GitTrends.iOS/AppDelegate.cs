using System;
using System.Diagnostics;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using Autofac;
using Foundation;
using UIKit;

namespace GitTrends.iOS
{
    [Register(nameof(AppDelegate))]
    public class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        public override bool FinishedLaunching(UIApplication uiApplication, NSDictionary launchOptions)
        {
            global::Xamarin.Forms.Forms.Init();
            Syncfusion.SfChart.XForms.iOS.Renderers.SfChartRenderer.Init();
            Syncfusion.XForms.iOS.Buttons.SfSegmentedControlRenderer.Init();

            FFImageLoading.Forms.Platform.CachedImageRenderer.Init();
            FFImageLoading.Forms.Platform.CachedImageRenderer.InitImageSourceHandler();
            var ignore = typeof(FFImageLoading.Svg.Forms.SvgCachedImage);

#if !AppStore
            Xamarin.Calabash.Start();
#endif

            PrintFontNamesToConsole();

            LoadApplication(new App());

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
                await gitHubAuthenticationService.AuthorizeSession(callbackUri).ConfigureAwait(false);
            }

            static void onException(Exception e)
            {
                using var containerScope = ContainerService.Container.BeginLifetimeScope();
                containerScope.Resolve<AnalyticsService>().Report(e);
            }
        }

#if !AppStore
        #region UI Test Back Door Methods
        [Preserve, Export(Mobile.Shared.BackdoorMethodConstants.SetGitHubUser + ":")]
        public async void SetGitHubUser(NSString accessToken)
        {
            using var scope = ContainerService.Container.BeginLifetimeScope();
            var backdoorService = scope.Resolve<UITestBackdoorService>();

            await backdoorService.SetGitHubUser(accessToken.ToString()).ConfigureAwait(false);
        }

        [Preserve, Export(Mobile.Shared.BackdoorMethodConstants.TriggerPullToRefresh + ":")]
        public async void TriggerRepositoriesPullToRefresh(NSString noValue)
        {
            using var scope = ContainerService.Container.BeginLifetimeScope();
            var backdoorService = scope.Resolve<UITestBackdoorService>();

            await backdoorService.TriggerPullToRefresh().ConfigureAwait(false);
        }

        [Preserve, Export(Mobile.Shared.BackdoorMethodConstants.GetVisibleRepositoryList + ":")]
        public NSString GetVisibleRepositoryList(NSString noValue)
        {
            using var scope = ContainerService.Container.BeginLifetimeScope();
            var backdoorService = scope.Resolve<UITestBackdoorService>();

            var serializedRepositoryList = Newtonsoft.Json.JsonConvert.SerializeObject(backdoorService.GetVisibleRepositoryList());
            return new NSString(serializedRepositoryList);
        }

        [Preserve, Export(Mobile.Shared.BackdoorMethodConstants.GetVisibleReferringSitesList + ":")]
        public NSString GetVisibleReferringSitesList(NSString noValue)
        {
            using var scope = ContainerService.Container.BeginLifetimeScope();
            var backdoorService = scope.Resolve<UITestBackdoorService>();

            var serializedReferringSitesList = Newtonsoft.Json.JsonConvert.SerializeObject(backdoorService.GetVisibleReferringSitesList());
            return new NSString(serializedReferringSitesList);
        }
        #endregion
#endif

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
