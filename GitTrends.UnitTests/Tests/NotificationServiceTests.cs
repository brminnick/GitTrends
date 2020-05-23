using System;
using System.Threading;
using System.Threading.Tasks;
using GitTrends.Shared;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace GitTrends.UnitTests
{
    class NotificationServiceTests : BaseTest
    {
        //            public event EventHandler<(bool isSuccessful, string errorMessage)> RegisterForNotificationsCompleted

        //            public bool ShouldSendNotifications

        //            public async Task<bool> AreNotificationsEnabled()

        //            public async Task Initialize(CancellationToken cancellationToken)
        //            public event EventHandler<NotificationHubInformation> InitializationCompleted

        //            public async Task<NotificationHubInformation> GetNotificationHubInformation()

        //            public void UnRegister() 

        //            public async Task<AccessState> Register(bool shouldShowSettingsUI)

        //            public async ValueTask SetAppBadgeCount(int count)

        //            public async ValueTask TrySendTrendingNotificaiton(IReadOnlyList<Repository> trendingRepositories, DateTimeOffset? notificationDateTime = null)

        //            public async Task HandleReceivedLocalNotification(string title, string message, int badgeCount)
        //            public event EventHandler<SortingOption> SortingOptionHandleReceivedLocalNotificationRequested

        [Test]
        public async Task InitializeTest()
        {
            //Arrange
            NotificationHubInformation notificationHubInformation_BeforeInitialization, notificationHubInformation_AfterInitialization;

            bool didInitializationCompletedFire = false;
            var initializationCompletedTCS = new TaskCompletionSource<NotificationHubInformation>();

            var notificationService = ContainerService.Container.GetService<NotificationService>();
            notificationService.InitializationCompleted += HandleInitializationCompleted;

            //Act
            notificationHubInformation_BeforeInitialization = await notificationService.GetNotificationHubInformation().ConfigureAwait(false);

            await notificationService.Initialize(CancellationToken.None).ConfigureAwait(false);

            notificationHubInformation_AfterInitialization = await notificationService.GetNotificationHubInformation().ConfigureAwait(false);
            await initializationCompletedTCS.Task.ConfigureAwait(false);

            //Assert
            Assert.IsTrue(didInitializationCompletedFire);

            Assert.AreEqual(NotificationHubInformation.Empty.ConnectionString, notificationHubInformation_BeforeInitialization.ConnectionString);
            Assert.AreEqual(NotificationHubInformation.Empty.ConnectionString_Debug, notificationHubInformation_BeforeInitialization.ConnectionString_Debug);
            Assert.AreEqual(NotificationHubInformation.Empty.Name, notificationHubInformation_BeforeInitialization.Name);
            Assert.AreEqual(NotificationHubInformation.Empty.Name_Debug, notificationHubInformation_BeforeInitialization.Name_Debug);

            Assert.IsFalse(string.IsNullOrWhiteSpace(notificationHubInformation_AfterInitialization.Name));
            Assert.IsFalse(string.IsNullOrWhiteSpace(notificationHubInformation_AfterInitialization.Name_Debug));
            Assert.IsFalse(string.IsNullOrWhiteSpace(notificationHubInformation_AfterInitialization.ConnectionString));
            Assert.IsFalse(string.IsNullOrWhiteSpace(notificationHubInformation_AfterInitialization.ConnectionString_Debug));

            void HandleInitializationCompleted(object? sender, NotificationHubInformation e)
            {
                notificationService.InitializationCompleted -= HandleInitializationCompleted;
                didInitializationCompletedFire = true;
                initializationCompletedTCS.SetResult(e);
            }
        }
    }
}
