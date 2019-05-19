using System;
using Foundation;
using UIKit;

namespace GitTrends.iOS
{
    [Register(nameof(AppDelegate))]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        public override bool FinishedLaunching(UIApplication uiApplication, NSDictionary launchOptions)
        {
            global::Xamarin.Forms.Forms.Init();
            LoadApplication(new App());

            return base.FinishedLaunching(uiApplication, launchOptions);
        }

        public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
        {
            var uri_netfx = new Uri(url.AbsoluteString);

            AuthenticationService.Authenticator.OnPageLoading(uri_netfx);

            return true;
        }
    }
}
