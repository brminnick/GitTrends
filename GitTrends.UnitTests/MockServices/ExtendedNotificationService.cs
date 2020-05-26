using System;
using GitTrends.Shared;
using Shiny.Notifications;
using Xamarin.Essentials.Interfaces;

namespace GitTrends.UnitTests
{
    public class ExtendedNotificationService : NotificationService
    {
        public ExtendedNotificationService(IAnalyticsService analyticsService,
                                            DeepLinkingService deepLinkingService,
                                            SortingService sortingService,
                                            AzureFunctionsApiService azureFunctionsApiService,
                                            IPreferences preferences,
                                            ISecureStorage secureStorage,
                                            INotificationManager notificationManager,
                                            INotificationService notificationService) :
                                        base(analyticsService,
                                            deepLinkingService,
                                            sortingService,
                                            azureFunctionsApiService,
                                            preferences,
                                            secureStorage,
                                            notificationManager,
                                            notificationService)
        {

        }

        public override void UnRegister()
        {
            base.UnRegister();
            HaveNotificationsBeenRequested = false;
        }
    }
}
