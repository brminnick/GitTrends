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
        UIVisualEffectView? _blurWindow;

        public override bool FinishedLaunching(UIApplication uiApplication, NSDictionary launchOptions)
        {
            global::Xamarin.Forms.Forms.SetFlags("CollectionView_Experimental");
            global::Xamarin.Forms.Forms.Init();
            Syncfusion.SfChart.XForms.iOS.Renderers.SfChartRenderer.Init();
            Syncfusion.XForms.iOS.Buttons.SfSegmentedControlRenderer.Init();

            FFImageLoading.Forms.Platform.CachedImageRenderer.Init();
            FFImageLoading.Forms.Platform.CachedImageRenderer.InitImageSourceHandler();
            var ignore = typeof(FFImageLoading.Svg.Forms.SvgCachedImage);

            LoadApplication(new App());

            using (var scope = ContainerService.Container.BeginLifetimeScope())
            {
                var syncFusionService = scope.Resolve<SyncFusionService>();
                syncFusionService.Initialize().SafeFireAndForget(onException: ex => Debug.WriteLine(ex));
            }

            PrintFontNamesToConsole();

            return base.FinishedLaunching(uiApplication, launchOptions);
        }

        public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
        {
            var callbackUri = new Uri(url.AbsoluteString);

            HandleCallbackUri().SafeFireAndForget(onException: x => Debug.WriteLine(x));

            return true;

            async Task HandleCallbackUri()
            {
                await ViewControllerServices.CloseSFSafariViewController().ConfigureAwait(false);

                using (var scope = ContainerService.Container.BeginLifetimeScope())
                {
                    var gitHubAuthenticationService = scope.Resolve<GitHubAuthenticationService>();
                    await gitHubAuthenticationService.AuthorizeSession(callbackUri).ConfigureAwait(false);
                }
            }
        }

        public override void OnActivated(UIApplication uiApplication)
        {
            base.OnActivated(uiApplication);

            removeBlurOverlay();

            void removeBlurOverlay()
            {
                _blurWindow?.RemoveFromSuperview();
                _blurWindow?.Dispose();
                _blurWindow = null;
            }
        }

        public override void OnResignActivation(UIApplication uiApplication)
        {
            base.OnResignActivation(uiApplication);

            addBlurOverlay();

            void addBlurOverlay()
            {
                using (var blurEffect = UIBlurEffect.FromStyle(UIBlurEffectStyle.Light))
                {
                    _blurWindow = new UIVisualEffectView(blurEffect)
                    {
                        Frame = UIApplication.SharedApplication.KeyWindow.RootViewController.View.Bounds
                    };
                }

                UIApplication.SharedApplication.KeyWindow.RootViewController.View.AddSubview(_blurWindow);
            }
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
