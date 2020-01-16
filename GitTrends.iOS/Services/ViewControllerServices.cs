using System;
using System.Linq;
using System.Threading.Tasks;
using SafariServices;
using UIKit;
using Xamarin.Forms;

namespace GitTrends.iOS
{
    public static class ViewControllerServices
    {
        public static UIViewController GetVisibleViewController() => Xamarin.Essentials.Platform.GetCurrentUIViewController();

        public static async ValueTask<UIViewController> GetVisibleViewControllerAsync()
        {
            if (Xamarin.Essentials.MainThread.IsMainThread)
                return GetVisibleViewController();

            return await Device.InvokeOnMainThreadAsync(GetVisibleViewController).ConfigureAwait(false);
        }

        public static async Task CloseSFSafariViewController()
        {
            while (await GetVisibleViewControllerAsync().ConfigureAwait(false) is SFSafariViewController sfSafariViewController)
            {
                if (Xamarin.Essentials.MainThread.IsMainThread)
                    await closeSFSafariViewController(sfSafariViewController).ConfigureAwait(false);
                else
                    await Device.InvokeOnMainThreadAsync(() => closeSFSafariViewController(sfSafariViewController)).ConfigureAwait(false);
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

