using GitTrends.Mobile.Common;
using GitTrends.Shared;
using Shiny.Notifications;
using Xamarin.Essentials.Interfaces;

namespace GitTrends.UnitTests
{
    public class ExtendedNotificationService : NotificationService
    {
        public ExtendedNotificationService(IPreferences preferences,
                                            ISecureStorage secureStorage,
                                            IAnalyticsService analyticsService,
                                            MobileSortingService sortingService,
                                            DeepLinkingService deepLinkingService,
                                            INotificationManager notificationManager,
                                            AzureFunctionsApiService azureFunctionsApiService,
                                            IDeviceNotificationsService deviceNotificationsService) :
                                        base(preferences, secureStorage, analyticsService, sortingService, deepLinkingService, notificationManager, deviceNotificationsService, azureFunctionsApiService)
        {

        }

        public override void UnRegister()
        {
            base.UnRegister();
            HaveNotificationsBeenRequested = false;
        }
    }
}
