using CommunityToolkit.Maui.ApplicationModel;
using GitTrends.Common;
using GitTrends.Mobile.Common;
using Shiny.Notifications;

namespace GitTrends.UnitTests;

public class ExtendedNotificationService(
	IBadge badge,
	IDeviceInfo deviceInfo,
	IPreferences preferences,
	IAnalyticsService analyticsService,
	MobileSortingService sortingService,
	DeepLinkingService deepLinkingService,
	INotificationManager notificationManager,
	INotificationPermissionStatus notificationPermissionStatus)
	: NotificationService(badge, deviceInfo, preferences, analyticsService, sortingService, deepLinkingService, notificationManager, notificationPermissionStatus)
{
	public override void UnRegister()
	{
		base.UnRegister();
		HaveNotificationsBeenRequested = false;
	}
}