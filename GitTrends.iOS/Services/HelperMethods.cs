using System.Threading.Tasks;
using SafariServices;
using UIKit;

namespace GitTrends.iOS
{
    public static class HelperMethods
    {
        public static Task<UIViewController> GetVisibleViewController()
        {
            return XamarinFormsService.BeginInvokeOnMainThreadAsync(() =>
            {
                var rootController = UIApplication.SharedApplication.KeyWindow.RootViewController;

                switch (rootController.PresentedViewController)
                {
                    case UINavigationController navigationController:
                        return navigationController.TopViewController;

                    case UITabBarController tabBarController:
                        return tabBarController.SelectedViewController;

                    case null:
                        return rootController;

                    default:
                        return rootController.PresentedViewController;
                }
            });
        }

        public static async Task CloseSFSafariViewController()
        {
            while (await GetVisibleViewController().ConfigureAwait(false) is SFSafariViewController sfSafariViewController)
            {
                await XamarinFormsService.BeginInvokeOnMainThreadAsync(() => sfSafariViewController.DismissViewControllerAsync(true)).ConfigureAwait(false);
            }
        }
    }
}

