using System;
using System.Diagnostics;
using AsyncAwaitBestPractices;
using Foundation;
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
            LoadApplication(new App());

            PrintFontNamesToConsole();

            return base.FinishedLaunching(uiApplication, launchOptions);
        }

        public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
        {
            var callbackUri = new Uri(url.AbsoluteString);

            HelperMethods.CloseSFSafariViewController().SafeFireAndForget(false);
            GitHubAuthenticationService.AuthorizeSession(callbackUri).SafeFireAndForget(false);

            return true;
        }

        public override void OnActivated(UIApplication uiApplication)
        {
            base.OnActivated(uiApplication);

            RemoveBlurOverlay();
        }

        public override void OnResignActivation(UIApplication uiApplication)
        {
            base.OnResignActivation(uiApplication);

            AddBlurOverlay();
        }

        void AddBlurOverlay()
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

        void RemoveBlurOverlay()
        {
            _blurWindow?.RemoveFromSuperview();
            _blurWindow?.Dispose();
            _blurWindow = null;
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
