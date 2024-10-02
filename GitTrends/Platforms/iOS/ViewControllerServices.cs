using SafariServices;
using UIKit;

namespace GitTrends;

public static class ViewControllerServices
{
	public static UIViewController GetVisibleViewController()
	{
		if (Platform.GetCurrentUIViewController() is UIViewController currentUiViewController)
			return currentUiViewController;

		throw new InvalidOperationException("No View Controller Found");
	}

	public static Task<UIViewController> GetVisibleViewControllerAsync() => MainThread.InvokeOnMainThreadAsync(GetVisibleViewController);

	public static async Task CloseSFSafariViewController()
	{
		while (await GetVisibleViewControllerAsync() is SFSafariViewController sfSafariViewController)
		{
			if (Dispatcher.GetForCurrentThread() is { IsDispatchRequired: true } dispatcher)
				await dispatcher.DispatchAsync(() => closeSFSafariViewController(sfSafariViewController));
			else
				await closeSFSafariViewController(sfSafariViewController);
		}

		static async Task closeSFSafariViewController(SFSafariViewController safariViewController)
		{
			await safariViewController.DismissViewControllerAsync(true).ConfigureAwait(false);
			safariViewController.Dispose();
		}
	}
}