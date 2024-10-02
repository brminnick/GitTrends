using GitTrends.Mobile.Common;
using GitTrends.Shared;
using Shiny.Notifications;

namespace GitTrends.UnitTests;

public class ExtendedNotificationService(
	IDeviceInfo deviceInfo,
	IPreferences preferences,
	IAnalyticsService analyticsService,
	MobileSortingService sortingService,
	DeepLinkingService deepLinkingService,
	INotificationManager notificationManager,
	INotificationPermissionStatus notificationPermissionStatus)
	: NotificationService(deviceInfo, preferences, analyticsService, sortingService, deepLinkingService, notificationManager, notificationPermissionStatus)
{
	public override void UnRegister()
	{
		base.UnRegister();
		HaveNotificationsBeenRequested = false;
	}
}