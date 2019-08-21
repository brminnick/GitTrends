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

                return rootController.PresentedViewController switch
                {
                    UINavigationController navigationController => navigationController.TopViewController,

                    UITabBarController tabBarController => tabBarController.SelectedViewController,

                    null => rootController,

                    _ => rootController.PresentedViewController,
                };
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
#pragma warning restore CS8600
                }).ConfigureAwait(false);
            }
        }
    }
}

