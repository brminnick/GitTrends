using GitTrends.Mobile.Common;
using GitTrends.Shared;
using Shiny.Notifications;

namespace GitTrends.UnitTests;

public class ExtendedNotificationService(
	IDeviceInfo deviceInfo,
	IPreferences preferences,
	ISecureStorage secureStorage,
	IAnalyticsService analyticsService,
	MobileSortingService sortingService,
	DeepLinkingService deepLinkingService,
	INotificationManager notificationManager,
	AzureFunctionsApiService azureFunctionsApiService,
	IDeviceNotificationsService deviceNotificationsService)
	: NotificationService(deviceInfo, preferences, secureStorage, analyticsService, sortingService, deepLinkingService, notificationManager, deviceNotificationsService, azureFunctionsApiService)
{
	public override void UnRegister()
	{
		base.UnRegister();
		HaveNotificationsBeenRequested = false;
	}
}