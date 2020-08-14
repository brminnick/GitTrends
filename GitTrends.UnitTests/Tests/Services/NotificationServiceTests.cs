using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using GitTrends.Shared;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shiny;
using Shiny.Notifications;

namespace GitTrends.UnitTests
{
    class NotificationServiceTests : BaseTest
    {
        [TestCase(0)]
        [TestCase(int.MinValue)]
        public void HandleNotificationTest_InvalidBadgeCount(int badgeCount)
        {
            //Arrange
            var title = NotificationConstants.TrendingRepositoriesNotificationTitle;
            var message = NotificationService.CreateSingleRepositoryNotificationMessage(GitTrendsRepoName, GitTrendsRepoOwner);

            var notificationService = ServiceCollection.ServiceProvider.GetRequiredService<NotificationService>();

            //Act //Assert
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await notificationService.HandleNotification(title, message, badgeCount).ConfigureAwait(false));
        }


        [Test]
        public async Task HandleNotificationTest_ValidSingleNotification()
        {
            //Arrange
            const int badgeCount = 1;
            var title = NotificationConstants.TrendingRepositoriesNotificationTitle;
            var message = NotificationService.CreateSingleRepositoryNotificationMessage(GitTrendsRepoName, GitTrendsRepoOwner);

            var notificationService = ServiceCollection.ServiceProvider.GetRequiredService<NotificationService>();

            //Act
            await notificationService.HandleNotification(title, message, badgeCount).ConfigureAwait(false);

            //Assert

        }

        [Test]
        public async Task HandleNotificationTest_ValidMultipleNotification()
        {
            //Arrange
            const int badgeCount = 2;

            SortingOption sortingOption;
            var title = NotificationConstants.TrendingRepositoriesNotificationTitle;
            var message = NotificationService.CreateMultipleRepositoryNotificationMessage(badgeCount);

            bool didSortingOptionRequestedFire = false;
            var sortingOptionRequestedTCS = new TaskCompletionSource<SortingOption>();

            var notificationService = ServiceCollection.ServiceProvider.GetRequiredService<NotificationService>();
            NotificationService.SortingOptionRequested += HandleSortingOptionRequested;

            var sortingService = ServiceCollection.ServiceProvider.GetRequiredService<MobileSortingService>();
            sortingService.IsReversed = true;

            //Act
            await notificationService.HandleNotification(title, message, badgeCount).ConfigureAwait(false);
            sortingOption = await sortingOptionRequestedTCS.Task.ConfigureAwait(false);

            //Assert
            Assert.IsTrue(didSortingOptionRequestedFire);
            Assert.AreEqual(SortingOption.Views, sortingOption);

            void HandleSortingOptionRequested(object? sender, SortingOption e)
            {
                NotificationService.SortingOptionRequested -= HandleSortingOptionRequested;

                didSortingOptionRequestedFire = true;
                sortingOptionRequestedTCS.SetResult(e);
            }
        }

        [Test]
        public async Task TrySendTrendingNotificationTest()
        {
            //Arrange
            int pendingNotificationsCount_Initial, pendingNotificationsCount_BeforeRegistration, pendingNotificationsCount_AfterEmptyRepositoryList, pendingNotificationsCount_AfterTrendingRepositoryList;

            IReadOnlyList<Repository> emptyTrendingRepositoryList = new List<Repository>();
            IReadOnlyList<Repository> trendingRepositoryList = new List<Repository> { CreateRepository() };

            var notificationService = ServiceCollection.ServiceProvider.GetRequiredService<NotificationService>();
            var notificationManager = ServiceCollection.ServiceProvider.GetRequiredService<INotificationManager>();

            //Act
            pendingNotificationsCount_Initial = await getPendingNotificationCount(notificationManager).ConfigureAwait(false);

            await notificationService.TrySendTrendingNotificaiton(trendingRepositoryList).ConfigureAwait(false);
            pendingNotificationsCount_BeforeRegistration = await getPendingNotificationCount(notificationManager).ConfigureAwait(false);

            await notificationService.Register(false).ConfigureAwait(false);

            await notificationService.TrySendTrendingNotificaiton(emptyTrendingRepositoryList).ConfigureAwait(false);
            pendingNotificationsCount_AfterEmptyRepositoryList = await getPendingNotificationCount(notificationManager).ConfigureAwait(false);

            await notificationService.TrySendTrendingNotificaiton(trendingRepositoryList).ConfigureAwait(false);
            pendingNotificationsCount_AfterTrendingRepositoryList = await getPendingNotificationCount(notificationManager).ConfigureAwait(false);

#if !DEBUG
            await notificationService.TrySendTrendingNotificaiton(trendingRepositoryList).ConfigureAwait(false);
            var pendingNotificationsCount_AfterDuplicateTrendingRepositoryList = await getPendingNotificationCount(notificationManager).ConfigureAwait(false);
            Assert.AreEqual(trendingRepositoryList.Count, pendingNotificationsCount_AfterDuplicateTrendingRepositoryList);
#endif

            //Assert
            Assert.AreEqual(0, pendingNotificationsCount_Initial);
            Assert.AreEqual(0, pendingNotificationsCount_BeforeRegistration);
            Assert.AreEqual(0, pendingNotificationsCount_AfterEmptyRepositoryList);
            Assert.AreEqual(trendingRepositoryList.Count, pendingNotificationsCount_AfterTrendingRepositoryList);

            static async Task<int> getPendingNotificationCount(INotificationManager notificationManager)
            {
                var pendingNotifications = await notificationManager.GetPending().ConfigureAwait(false);
                return pendingNotifications.Count();
            }
        }

        [Test]
        public async Task SetAppBadgeCountTest()
        {
            //Arrange
            const int five = 5;
            const int zero = 0;

            int appBadgeCount_Initial, appBadgeCount_BeforeRegistration, appBadgeCount_AfterSet, appBadgeCount_AfterClear;

            var notificationService = ServiceCollection.ServiceProvider.GetRequiredService<NotificationService>();
            var notificationManager = ServiceCollection.ServiceProvider.GetRequiredService<INotificationManager>();

            //Act
            appBadgeCount_Initial = notificationManager.Badge;

            await notificationService.SetAppBadgeCount(five).ConfigureAwait(false);

            appBadgeCount_BeforeRegistration = notificationManager.Badge;

            await notificationService.Register(false).ConfigureAwait(false);
            await notificationService.SetAppBadgeCount(five).ConfigureAwait(false);

            appBadgeCount_AfterSet = notificationManager.Badge;

            await notificationService.SetAppBadgeCount(zero).ConfigureAwait(false);

            appBadgeCount_AfterClear = notificationManager.Badge;

            //Assert
            Assert.AreEqual(zero, appBadgeCount_Initial);
            Assert.AreEqual(zero, appBadgeCount_BeforeRegistration);
            Assert.AreEqual(five, appBadgeCount_AfterSet);
            Assert.AreEqual(zero, appBadgeCount_AfterClear);
        }

        [Test]
        public async Task RegisterTest()
        {
            //Arrange
            bool areNotificationsEnabled_Initial, areNotificationsEnabled_AfterRegistration, areNotificationsEnabled_AfterUnRegistration;
            bool shouldSendNotifications_Initial, shouldSendNotifications_AfterRegistration, shouldSendNotifications_AfterUnRegistration;

            bool didRegistrationCompletedFire = false;
            var registrationCompletedTCS = new TaskCompletionSource<(bool isSuccessful, string errorMessage)>();

            var notificationService = ServiceCollection.ServiceProvider.GetRequiredService<NotificationService>();
            NotificationService.RegisterForNotificationsCompleted += HandleRegistrationCompleted;

            //Act
            areNotificationsEnabled_Initial = await notificationService.AreNotificationsEnabled().ConfigureAwait(false);
            shouldSendNotifications_Initial = notificationService.ShouldSendNotifications;

            var registrationResult = await notificationService.Register(false).ConfigureAwait(false);
            var (isRegistrationSuccessful, registrationErrorMessage) = await registrationCompletedTCS.Task.ConfigureAwait(false);

            areNotificationsEnabled_AfterRegistration = await notificationService.AreNotificationsEnabled().ConfigureAwait(false);
            shouldSendNotifications_AfterRegistration = notificationService.ShouldSendNotifications;

            notificationService.UnRegister();
            areNotificationsEnabled_AfterUnRegistration = await notificationService.AreNotificationsEnabled().ConfigureAwait(false);
            shouldSendNotifications_AfterUnRegistration = notificationService.ShouldSendNotifications;

            //Assert
            Assert.IsTrue(didRegistrationCompletedFire);
            Assert.AreEqual(AccessState.Available, registrationResult);

            Assert.IsFalse(shouldSendNotifications_Initial);
            Assert.IsFalse(shouldSendNotifications_AfterUnRegistration);

            Assert.AreEqual(isRegistrationSuccessful, shouldSendNotifications_AfterRegistration);
            Assert.AreEqual(isRegistrationSuccessful, string.IsNullOrWhiteSpace(registrationErrorMessage));

            Assert.IsFalse(areNotificationsEnabled_Initial);
            Assert.IsTrue(areNotificationsEnabled_AfterRegistration);
            Assert.IsTrue(areNotificationsEnabled_AfterUnRegistration);


            void HandleRegistrationCompleted(object? sender, (bool isSuccessful, string errorMessage) e)
            {
                NotificationService.RegisterForNotificationsCompleted -= HandleRegistrationCompleted;

                didRegistrationCompletedFire = true;
                registrationCompletedTCS.SetResult(e);
            }
        }

        [Test]
        public async Task InitializeTest()
        {
            //Arrange
            NotificationHubInformation notificationHubInformation_BeforeInitialization, notificationHubInformation_AfterInitialization;

            bool didInitializationCompletedFire = false;
            var initializationCompletedTCS = new TaskCompletionSource<NotificationHubInformation>();

            var notificationService = ServiceCollection.ServiceProvider.GetRequiredService<NotificationService>();
            NotificationService.InitializationCompleted += HandleInitializationCompleted;

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
                NotificationService.InitializationCompleted -= HandleInitializationCompleted;
                didInitializationCompletedFire = true;
                initializationCompletedTCS.SetResult(e);
            }
        }
    }
}
