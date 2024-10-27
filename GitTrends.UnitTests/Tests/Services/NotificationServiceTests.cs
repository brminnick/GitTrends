using GitTrends.Common;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using Shiny;
using Shiny.Notifications;

namespace GitTrends.UnitTests;

class NotificationServiceTests : BaseTest
{
	[TestCase(0)]
	[TestCase(int.MinValue)]
	public void HandleNotificationTest_InvalidBadgeCount(int badgeCount)
	{
		//Arrange
		var title = NotificationConstants.TrendingRepositoriesNotificationTitle;
		var message = NotificationService.CreateSingleRepositoryNotificationMessage(GitHubConstants.GitTrendsRepoName, GitHubConstants.GitTrendsRepoOwner);

		var notificationService = ServiceCollection.ServiceProvider.GetRequiredService<NotificationService>();

		//Act //Assert
		Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await notificationService.HandleNotification(title, message, badgeCount, TestCancellationTokenSource.Token).ConfigureAwait(false));
	}


	[Test]
	public async Task HandleNotificationTest_ValidSingleNotification()
	{
		//Arrange
		const int badgeCount = 1;
		var title = NotificationConstants.TrendingRepositoriesNotificationTitle;
		var message = NotificationService.CreateSingleRepositoryNotificationMessage(GitHubConstants.GitTrendsRepoName, GitHubConstants.GitTrendsRepoOwner);

		var notificationService = ServiceCollection.ServiceProvider.GetRequiredService<NotificationService>();

		//Act
		await notificationService.HandleNotification(title, message, badgeCount, TestCancellationTokenSource.Token).ConfigureAwait(false);

		//Assert

	}

	[Test, CancelAfter(5000)]
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
		await notificationService.HandleNotification(title, message, badgeCount, TestCancellationTokenSource.Token).ConfigureAwait(false);
		sortingOption = await sortingOptionRequestedTCS.Task.WaitAsync(TestCancellationTokenSource.Token).ConfigureAwait(false);

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(didSortingOptionRequestedFire);
			Assert.That(sortingOption, Is.EqualTo(SortingOption.Views));
		});

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

		IReadOnlyList<Repository> emptyTrendingRepositoryList = [];
		IReadOnlyList<Repository> trendingRepositoryList = [CreateRepository()];

		var notificationService = ServiceCollection.ServiceProvider.GetRequiredService<NotificationService>();
		var notificationManager = ServiceCollection.ServiceProvider.GetRequiredService<INotificationManager>();

		//Act
		pendingNotificationsCount_Initial = await getPendingNotificationCount(notificationManager).ConfigureAwait(false);

		await notificationService.TrySendTrendingNotification(trendingRepositoryList).ConfigureAwait(false);
		pendingNotificationsCount_BeforeRegistration = await getPendingNotificationCount(notificationManager).ConfigureAwait(false);

		await notificationService.Register(false, TestCancellationTokenSource.Token).ConfigureAwait(false);

		await notificationService.TrySendTrendingNotification(emptyTrendingRepositoryList).ConfigureAwait(false);
		pendingNotificationsCount_AfterEmptyRepositoryList = await getPendingNotificationCount(notificationManager).ConfigureAwait(false);

		await notificationService.TrySendTrendingNotification(trendingRepositoryList).ConfigureAwait(false);
		pendingNotificationsCount_AfterTrendingRepositoryList = await getPendingNotificationCount(notificationManager).ConfigureAwait(false);

#if !DEBUG
            await notificationService.TrySendTrendingNotification(trendingRepositoryList).ConfigureAwait(false);
            var pendingNotificationsCount_AfterDuplicateTrendingRepositoryList = await getPendingNotificationCount(notificationManager).ConfigureAwait(false);
            Assert.That(trendingRepositoryList, Has.Count.EqualTo(pendingNotificationsCount_AfterDuplicateTrendingRepositoryList));
#endif

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(pendingNotificationsCount_Initial, Is.EqualTo(0));
			Assert.That(pendingNotificationsCount_BeforeRegistration, Is.EqualTo(0));
			Assert.That(pendingNotificationsCount_AfterEmptyRepositoryList, Is.EqualTo(0));
			Assert.That(pendingNotificationsCount_AfterTrendingRepositoryList, Is.EqualTo(trendingRepositoryList.Count));
		});

		static async Task<int> getPendingNotificationCount(INotificationManager notificationManager)
		{
			var pendingNotifications = await notificationManager.GetPendingNotifications().ConfigureAwait(false);
			return pendingNotifications.Count;
		}
	}

	[Test]
	public async Task SetAppBadgeCountTest()
	{
		//Arrange
		const int five = 5;
		const int zero = 0;

		int? appBadgeCount_Initial, appBadgeCount_BeforeRegistration, appBadgeCount_AfterSet, appBadgeCount_AfterClear;

		var notificationService = ServiceCollection.ServiceProvider.GetRequiredService<NotificationService>();
		var notificationManager = ServiceCollection.ServiceProvider.GetRequiredService<INotificationManager>();

		//Act
		(_, appBadgeCount_Initial) = await notificationManager.TryGetBadge();

		await notificationService.SetAppBadgeCount(five, TestCancellationTokenSource.Token).ConfigureAwait(false);

		(_, appBadgeCount_BeforeRegistration) = await notificationManager.TryGetBadge().ConfigureAwait(false);

		await notificationService.Register(false, TestCancellationTokenSource.Token).ConfigureAwait(false);
		await notificationService.SetAppBadgeCount(five, TestCancellationTokenSource.Token).ConfigureAwait(false);

		(_, appBadgeCount_AfterSet) = await notificationManager.TryGetBadge().ConfigureAwait(false);

		await notificationService.SetAppBadgeCount(zero, TestCancellationTokenSource.Token).ConfigureAwait(false);

		(_, appBadgeCount_AfterClear) = await notificationManager.TryGetBadge().ConfigureAwait(false);

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(appBadgeCount_Initial, Is.EqualTo(zero));
			Assert.That(appBadgeCount_BeforeRegistration, Is.EqualTo(zero));
			Assert.That(appBadgeCount_AfterSet, Is.EqualTo(five));
			Assert.That(appBadgeCount_AfterClear, Is.EqualTo(zero));
		});
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
		areNotificationsEnabled_Initial = await notificationService.AreNotificationsEnabled(TestCancellationTokenSource.Token).ConfigureAwait(false);
		shouldSendNotifications_Initial = notificationService.ShouldSendNotifications;

		var registrationResult = await notificationService.Register(false, TestCancellationTokenSource.Token).ConfigureAwait(false);
		var (isRegistrationSuccessful, registrationErrorMessage) = await registrationCompletedTCS.Task.WaitAsync(TestCancellationTokenSource.Token).ConfigureAwait(false);

		areNotificationsEnabled_AfterRegistration = await notificationService.AreNotificationsEnabled(TestCancellationTokenSource.Token).ConfigureAwait(false);
		shouldSendNotifications_AfterRegistration = notificationService.ShouldSendNotifications;

		notificationService.UnRegister();
		areNotificationsEnabled_AfterUnRegistration = await notificationService.AreNotificationsEnabled(TestCancellationTokenSource.Token).ConfigureAwait(false);
		shouldSendNotifications_AfterUnRegistration = notificationService.ShouldSendNotifications;

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(didRegistrationCompletedFire, Is.True);
			Assert.That(registrationResult, Is.EqualTo(AccessState.Available));

			Assert.That(shouldSendNotifications_Initial, Is.False);
			Assert.That(shouldSendNotifications_AfterUnRegistration, Is.False);

			Assert.That(shouldSendNotifications_AfterRegistration, Is.EqualTo(isRegistrationSuccessful));
			Assert.That(string.IsNullOrWhiteSpace(registrationErrorMessage), Is.EqualTo(isRegistrationSuccessful));

			Assert.That(areNotificationsEnabled_Initial, Is.False);
			Assert.That(areNotificationsEnabled_AfterRegistration);
			Assert.That(areNotificationsEnabled_AfterUnRegistration);
		});

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
		bool didInitializationCompletedFire = false;
		var initializationCompletedTCS = new TaskCompletionSource();

		var notificationService = ServiceCollection.ServiceProvider.GetRequiredService<NotificationService>();
		NotificationService.InitializationCompleted += HandleInitializationCompleted;

		//Act
		notificationService.Initialize();

		await initializationCompletedTCS.Task.WaitAsync(TestCancellationTokenSource.Token).ConfigureAwait(false);

		//Assert
		Assert.That(didInitializationCompletedFire);

		void HandleInitializationCompleted(object? sender, EventArgs e)
		{
			NotificationService.InitializationCompleted -= HandleInitializationCompleted;
			didInitializationCompletedFire = true;
			initializationCompletedTCS.SetResult();
		}
	}
}