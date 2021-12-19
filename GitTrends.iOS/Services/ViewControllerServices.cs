using System.Threading.Tasks;
using SafariServices;
using UIKit;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace GitTrends.iOS
{
    public static class ViewControllerServices
    {
        public static UIViewController GetVisibleViewController() => Platform.GetCurrentUIViewController();

        public static Task<UIViewController> GetVisibleViewControllerAsync() => MainThread.InvokeOnMainThreadAsync(GetVisibleViewController);

        public static async Task CloseSFSafariViewController()
        {
            while (await GetVisibleViewControllerAsync() is SFSafariViewController sfSafariViewController)
            {
                await MainThread.InvokeOnMainThreadAsync(() => closeSFSafariViewController(sfSafariViewController));
            }

            static async Task closeSFSafariViewController(SFSafariViewController? safariViewController)
            {
                await (safariViewController?.DismissViewControllerAsync(true) ?? Task.CompletedTask).ConfigureAwait(false);

                safariViewController?.Dispose();
                safariViewController = null;
            }
        }
    }
}

