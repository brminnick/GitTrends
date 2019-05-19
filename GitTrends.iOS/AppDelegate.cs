using System;
using Foundation;
using UIKit;
using System.Threading.Tasks;
using Octokit;
using SafariServices;
using AsyncAwaitBestPractices;

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
            var callbackUri = new Uri(url.AbsoluteString);

            CloseSFSafariViewController().SafeFireAndForget();
            GitHubAuthenticationService.AuthorizeSession(callbackUri).SafeFireAndForget();

            return true;
        }

        async Task CloseSFSafariViewController()
        {
            var currentViewController = await HelperMethods.GetVisibleViewController().ConfigureAwait(false);

            if (currentViewController is SFSafariViewController safariViewController)
                await XamarinFormsServices.BeginInvokeOnMainThreadAsync(() => safariViewController.DismissViewControllerAsync(true));
        }
    }
}
