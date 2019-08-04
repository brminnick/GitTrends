using System.Threading.Tasks;
using SafariServices;
using UIKit;
using Xamarin.Forms;

namespace GitTrends.iOS
{
    public static class ViewControllerServices
    {
        public static Task<UIViewController> GetVisibleViewController()
        {
            return Device.InvokeOnMainThreadAsync(() =>
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
                await Device.InvokeOnMainThreadAsync(() =>
                {
                    sfSafariViewController.DismissViewControllerAsync(true);
                    sfSafariViewController.Dispose();

#pragma warning disable CS8600 //Converting null literal or possible null value to non-nullable type
                    sfSafariViewController = null;
#pragma warning enable CS8600
                }).ConfigureAwait(false);
            }
        }
    }
}

