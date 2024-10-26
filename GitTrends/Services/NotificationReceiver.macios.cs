using AsyncAwaitBestPractices;
using UserNotifications;

namespace GitTrends;

public class NotificationReceiver : UNUserNotificationCenterDelegate
{
	// Called if app is in the foreground.
	public override void WillPresentNotification(UNUserNotificationCenter center, UNNotification notification, Action<UNNotificationPresentationOptions> completionHandler)
	{
		ProcessNotification(notification);

		var presentationOptions = OperatingSystem.IsIOSVersionAtLeast(14)
			? UNNotificationPresentationOptions.Banner
			: UNNotificationPresentationOptions.Alert;

		completionHandler(presentationOptions);
	}

	// Called if app is in the background, or killed state.
	public override void DidReceiveNotificationResponse(UNUserNotificationCenter center, UNNotificationResponse response, Action completionHandler)
	{
		if (response.IsDefaultAction)
			ProcessNotification(response.Notification).SafeFireAndForget();

		completionHandler();
	}

	static Task ProcessNotification(UNNotification notification)
	{
		var title = notification.Request.Content.Title;
		var message = notification.Request.Content.Body;
		var badgeNumber = notification.Request.Content.Badge;

		var service = IPlatformApplication.Current?.Services.GetRequiredService<NotificationService>()
			?? throw new InvalidOperationException($"{nameof(NotificationService)} not found in {nameof(IServiceProvider)}");

		return service.HandleNotification(title, message, badgeNumber?.Int32Value ?? 1, CancellationToken.None);
	}
}