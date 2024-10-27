using AsyncAwaitBestPractices;
using CommunityToolkit.Maui.ApplicationModel;
using GitTrends.Common;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using Shiny;
using Shiny.Notifications;
#if IOS || MACCATALYST
using UserNotifications;
#endif

namespace GitTrends;

public class NotificationService
{
	static readonly AsyncAwaitBestPractices.WeakEventManager _initializationCompletedEventManager = new();
	static readonly WeakEventManager<SortingOption> _sortingOptionRequestedEventManager = new();
	static readonly WeakEventManager<(bool isSuccessful, string errorMessage)> _registerForNotificationCompletedEventHandler = new();

	readonly IBadge _badge;
	readonly IDeviceInfo _deviceInfo;
	readonly IPreferences _preferences;
	readonly MobileSortingService _sortingService;
	readonly IAnalyticsService _analyticsService;
	readonly DeepLinkingService _deepLinkingService;
	readonly INotificationManager _notificationManager;
	readonly INotificationPermissionStatus _notificationPermissionService;

	TaskCompletionSource<AccessState>? _settingsResultCompletionSource;

	public NotificationService(
		IBadge badge,
		IDeviceInfo deviceInfo,
		IPreferences preferences,
		IAnalyticsService analyticsService,
		MobileSortingService sortingService,
		DeepLinkingService deepLinkingService,
		INotificationManager notificationManager,
		INotificationPermissionStatus notificationPermissionService)
	{
		_badge = badge;
		_deviceInfo = deviceInfo;
		_preferences = preferences;
		_sortingService = sortingService;
		_analyticsService = analyticsService;
		_deepLinkingService = deepLinkingService;
		_notificationManager = notificationManager;
		_notificationPermissionService = notificationPermissionService;

		App.Resumed += HandleAppResumed;

#if IOS || MACCATALYST
		UNUserNotificationCenter.Current.Delegate = new NotificationReceiver();
#endif
	}

	public static event EventHandler InitializationCompleted
	{
		add => _initializationCompletedEventManager.AddEventHandler(value);
		remove => _initializationCompletedEventManager.RemoveEventHandler(value);
	}

	public static event EventHandler<SortingOption> SortingOptionRequested
	{
		add => _sortingOptionRequestedEventManager.AddEventHandler(value);
		remove => _sortingOptionRequestedEventManager.RemoveEventHandler(value);
	}

	public static event EventHandler<(bool IsSuccessful, string ErrorMessage)> RegisterForNotificationsCompleted
	{
		add => _registerForNotificationCompletedEventHandler.AddEventHandler(value);
		remove => _registerForNotificationCompletedEventHandler.RemoveEventHandler(value);
	}

	public bool ShouldSendNotifications
	{
		get => _preferences.Get(nameof(ShouldSendNotifications), false);
		private set => _preferences.Set(nameof(ShouldSendNotifications), value);
	}

	protected bool HaveNotificationsBeenRequested
	{
		get => _preferences.Get(nameof(HaveNotificationsBeenRequested), false);
		set => _preferences.Set(nameof(HaveNotificationsBeenRequested), value);
	}

	string MostRecentTrendingRepositoryOwner
	{
		get => _preferences.Get(nameof(MostRecentTrendingRepositoryOwner), string.Empty);
		set => _preferences.Set(nameof(MostRecentTrendingRepositoryOwner), value);
	}

	string MostRecentTrendingRepositoryName
	{
		get => _preferences.Get(nameof(MostRecentTrendingRepositoryName), string.Empty);
		set => _preferences.Set(nameof(MostRecentTrendingRepositoryName), value);
	}

	public static string CreateSingleRepositoryNotificationMessage(in string repositoryName, in string repositoryOwner) => string.Format(NotificationConstants.SingleRepositoryNotificationMessage, repositoryName, repositoryOwner);
	public static string CreateMultipleRepositoryNotificationMessage(in int count) => string.Format(NotificationConstants.MultipleRepositoryNotificationMessage, count);

	public Task<bool> AreNotificationsEnabled(CancellationToken token) => _notificationPermissionService.AreNotificationsEnabled(token);

	public void Initialize()
	{
		var channels = _notificationManager.GetChannels();
		if (!channels.Any()
			&& (_deviceInfo.Platform != DevicePlatform.Android || _deviceInfo.Version.Major >= 8)) // Channels only supported in Android v8.0+
		{
			_notificationManager.AddChannel(new Channel
			{
				Identifier = nameof(GitTrends),
				Description = "GitTrends Notifications",
				Importance = ChannelImportance.High,
			});
		}

		OnInitializationCompleted();
	}

	public virtual void UnRegister() => ShouldSendNotifications = false;

	public async Task<AccessState> Register(bool shouldShowSettingsUI, CancellationToken token)
	{
		AccessState? finalNotificationRequestResult = null;
		string errorMessage = string.Empty;

		HaveNotificationsBeenRequested = ShouldSendNotifications = true;

		var initialNotificationRequestResult = await _notificationManager.RequestAccess().ConfigureAwait(false);

		try
		{
			switch (initialNotificationRequestResult)
			{
				case AccessState.Denied when shouldShowSettingsUI:
				case AccessState.Disabled when shouldShowSettingsUI:
					_settingsResultCompletionSource = new TaskCompletionSource<AccessState>();
					await _deepLinkingService.ShowSettingsUI(token).ConfigureAwait(false);
					finalNotificationRequestResult = await _settingsResultCompletionSource.Task.ConfigureAwait(false);
					break;

				case AccessState.Denied:
				case AccessState.Disabled:
					errorMessage = NotificationConstants.NotificationsDisabled;
					break;

				case AccessState.NotSetup:
					finalNotificationRequestResult = await _notificationManager.RequestAccess().ConfigureAwait(false);
					break;

				case AccessState.NotSupported:
					errorMessage = NotificationConstants.NotificationsNotSupported;
					break;
			}

			return finalNotificationRequestResult ??= initialNotificationRequestResult;
		}
		catch (Exception e)
		{
			_analyticsService.Report(e);
			errorMessage = e.Message;

			return initialNotificationRequestResult;
		}
		finally
		{
			if (finalNotificationRequestResult is AccessState.Available or AccessState.Restricted)
				OnRegisterForNotificationsCompleted(true, string.Empty);
			else
				OnRegisterForNotificationsCompleted(false, errorMessage);

			_settingsResultCompletionSource = null;

			_analyticsService.Track("Register For Notifications", new Dictionary<string, string>
			{
				{
					nameof(initialNotificationRequestResult), initialNotificationRequestResult.ToString()
				},
				{
					nameof(finalNotificationRequestResult), finalNotificationRequestResult?.ToString() ?? "null"
				},
			});
		}
	}

	public async ValueTask SetAppBadgeCount(int count, CancellationToken token)
	{
		if (HaveNotificationsBeenRequested || count is 0)
		{
			var isSuccessful = await _notificationManager.TrySetBadge(count).WaitAsync(token).ConfigureAwait(false);
			if (!isSuccessful)
			{
				_badge.SetCount(Convert.ToUInt32(count));
			}
		}
	}

	public async ValueTask TrySendTrendingNotification(IReadOnlyList<Repository> trendingRepositories, DateTimeOffset? notificationDateTime = null)
	{
		if (!ShouldSendNotifications)
			return;
#if DEBUG
		await SendTrendingNotification(trendingRepositories, notificationDateTime).ConfigureAwait(false);
#else
		IReadOnlyList<Repository> repositoriesToNotify = [.. trendingRepositories.Where(shouldSendNotification)];
		await SendTrendingNotification(repositoriesToNotify, notificationDateTime).ConfigureAwait(false);

		bool shouldSendNotification(Repository trendingRepository)
		{
			var nextNotificationDate = getMostRecentNotificationDate(trendingRepository).AddDays(3);
			return DateTime.Compare(nextNotificationDate, DateTime.UtcNow) < 1;

			DateTime getMostRecentNotificationDate(Repository repository) => _preferences.Get(repository.Name, default(DateTime));
		}
#endif
	}

	public async Task HandleNotification(string title, string message, int badgeCount, CancellationToken token)
	{
		if (badgeCount is 1)
		{
			var alertTitle = string.Format(NotificationConstants.HandleNotification_SingleTrendingRepository_Title, MostRecentTrendingRepositoryName);
			var shouldNavigateToChart = await _deepLinkingService.DisplayAlert(alertTitle,
				NotificationConstants.HandleNotification_SingleTrendingRepository_Message,
				NotificationConstants.HandleNotification_SingleTrendingRepository_Accept,
				NotificationConstants.HandleNotification_SingleTrendingRepository_Decline,
				token).ConfigureAwait(false);

			if (Application.Current is null) // Set default value for unit tests
				shouldNavigateToChart = true;

			if (shouldNavigateToChart is null)
				throw new InvalidOperationException($"{nameof(shouldNavigateToChart)} cannot be null");

			_analyticsService.Track("Single Trending Repository Prompt Displayed", nameof(shouldNavigateToChart), shouldNavigateToChart.Value.ToString());

			if (shouldNavigateToChart.Value)
			{
				//Create repository with only Name & Owner, because those are the only metrics that TrendsPage needs to fetch the chart data
				var repository = new Repository(MostRecentTrendingRepositoryName, string.Empty, 0,
					MostRecentTrendingRepositoryOwner, string.Empty,
					0, 0, 0, string.Empty, false, default, RepositoryPermission.UNKNOWN, false);

				await _deepLinkingService.NavigateToTrendsPage(repository, token).ConfigureAwait(false);
			}
		}
		else if (badgeCount > 1)
		{
			bool? shouldSortByTrending = null;

			if (!_sortingService.IsReversed)
			{
				await _deepLinkingService.DisplayAlert(message,
					NotificationConstants.HandleNotification_MultipleTrendingRepository_Sorted_Message,
					NotificationConstants.HandleNotification_MultipleTrendingRepository_Cancel,
					token).ConfigureAwait(false);
			}
			else
			{
				shouldSortByTrending = await _deepLinkingService.DisplayAlert(message,
					NotificationConstants.HandleNotification_MultipleTrendingRepositor_Unsorted_Message,
					NotificationConstants.HandleNotification_MultipleTrendingRepositor_Unsorted_Accept,
					NotificationConstants.HandleNotification_MultipleTrendingRepositor_Unsorted_Decline,
					token).ConfigureAwait(false);
			}

			if (Application.Current is null // Set default value for unit tests
				|| shouldSortByTrending is true)
				OnSortingOptionRequested(_sortingService.CurrentOption);

			_analyticsService.Track("Multiple Trending Repository Prompt Displayed", nameof(shouldSortByTrending), shouldSortByTrending?.ToString() ?? "null");
		}
		else
		{
			throw new ArgumentOutOfRangeException(nameof(badgeCount), @$"{badgeCount} must be greater than zero");
		}
	}

	async ValueTask SendTrendingNotification(IReadOnlyList<Repository> trendingRepositories, DateTimeOffset? notificationDateTime)
	{
		if (trendingRepositories.Count is 1)
		{
			var trendingRepository = trendingRepositories[0];

			var notification = new Notification
			{
				//iOS crashes when ID is not set
				Id = _deviceInfo.Platform == DevicePlatform.iOS ? 1 : 0,
				Title = NotificationConstants.TrendingRepositoriesNotificationTitle,
				Message = CreateSingleRepositoryNotificationMessage(trendingRepository.Name, trendingRepository.OwnerLogin),
				ScheduleDate = notificationDateTime,
				BadgeCount = 1
			};

			MostRecentTrendingRepositoryName = trendingRepository.Name;
			MostRecentTrendingRepositoryOwner = trendingRepository.OwnerLogin;

			await _notificationManager.Send(notification).ConfigureAwait(false);

			_analyticsService.Track("Single Trending Repository Notification Sent");
		}
		else if (trendingRepositories.Count > 1)
		{
			var notification = new Notification
			{
				//iOS crashes when ID is not set
				Id = _deviceInfo.Platform == DevicePlatform.iOS ? 1 : 0,
				Title = NotificationConstants.TrendingRepositoriesNotificationTitle,
				Message = CreateMultipleRepositoryNotificationMessage(trendingRepositories.Count),
				ScheduleDate = notificationDateTime,
				BadgeCount = trendingRepositories.Count
			};

			await _notificationManager.Send(notification).ConfigureAwait(false);

			_analyticsService.Track("Multiple Trending Repositories Notification Sent", "Count", trendingRepositories.Count.ToString());
		}

		foreach (var repository in trendingRepositories)
		{
			setMostRecentNotificationDate(repository);
		}

		void setMostRecentNotificationDate(Repository repository) => _preferences.Set(repository.Name, DateTime.UtcNow);
	}

	async void HandleAppResumed(object? sender, EventArgs e)
	{
		if (_settingsResultCompletionSource is not null)
		{
			var finalResult = await _notificationManager.RequestAccess().ConfigureAwait(false);
			_settingsResultCompletionSource.SetResult(finalResult);
		}
	}

	void OnInitializationCompleted() => _initializationCompletedEventManager.RaiseEvent(this, EventArgs.Empty, nameof(InitializationCompleted));

	void OnSortingOptionRequested(SortingOption sortingOption) => _sortingOptionRequestedEventManager.RaiseEvent(this, sortingOption, nameof(SortingOptionRequested));

	void OnRegisterForNotificationsCompleted(bool isSuccessful, string errorMessage) =>
		_registerForNotificationCompletedEventHandler.RaiseEvent(this, (isSuccessful, errorMessage), nameof(RegisterForNotificationsCompleted));
}