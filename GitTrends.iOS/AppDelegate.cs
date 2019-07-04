using System;
using System.Diagnostics;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using Foundation;
using GitTrends.Mobile.Shared;
using UIKit;

namespace GitTrends.iOS
{
    [Register(nameof(AppDelegate))]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        UIVisualEffectView _blurWindow;

        public override bool FinishedLaunching(UIApplication uiApplication, NSDictionary launchOptions)
        {
            global::Xamarin.Forms.Forms.Init();
            FFImageLoading.Forms.Platform.CachedImageRenderer.Init();
            FFImageLoading.Forms.Platform.CachedImageRenderer.InitImageSourceHandler();

            SyncFusionService.Initialize().SafeFireAndForget(onException: ex => Debug.WriteLine(ex));

            LoadApplication(new App());

            PrintFontNamesToConsole();

            return base.FinishedLaunching(uiApplication, launchOptions);
        }

        public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
        {
            var callbackUri = new Uri(url.AbsoluteString);

            HandleCallbackUri().SafeFireAndForget();

            return true;

            async Task HandleCallbackUri()
            {
                await HelperMethods.CloseSFSafariViewController().ConfigureAwait(false);
                await GitHubAuthenticationService.AuthorizeSession(callbackUri).ConfigureAwait(false);
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
