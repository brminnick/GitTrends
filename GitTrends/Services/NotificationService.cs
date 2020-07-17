using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using GitTrends.Shared;
using Newtonsoft.Json;
using Shiny;
using Shiny.Notifications;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;

namespace GitTrends
{
    public class NotificationService
    {
        const string _getNotificationHubInformationKey = "GetNotificationHubInformation";

        readonly static WeakEventManager<SortingOption> _sortingOptionRequestedEventManager = new WeakEventManager<SortingOption>();
        readonly static WeakEventManager<NotificationHubInformation> _initializationCompletedEventManager = new WeakEventManager<NotificationHubInformation>();
        readonly static WeakEventManager<(bool isSuccessful, string errorMessage)> _registerForNotificationCompletedEventHandler = new WeakEventManager<(bool isSuccessful, string errorMessage)>();

        readonly IPreferences _preferences;
        readonly ISecureStorage _secureStorage;
        readonly MobileSortingService _sortingService;
        readonly IAnalyticsService _analyticsService;
        readonly DeepLinkingService _deepLinkingService;
        readonly INotificationManager _notificationManager;
        readonly IDeviceNotificationsService _notificationService;
        readonly AzureFunctionsApiService _azureFunctionsApiService;

        TaskCompletionSource<AccessState>? _settingsResultCompletionSource;

        public NotificationService(IPreferences preferences,
                                    ISecureStorage secureStorage,
                                    IAnalyticsService analyticsService,
                                    MobileSortingService sortingService,
                                    DeepLinkingService deepLinkingService,
                                    INotificationManager notificationManager,
                                    IDeviceNotificationsService notificationService,
                                    AzureFunctionsApiService azureFunctionsApiService)
        {
            _preferences = preferences;
            _secureStorage = secureStorage;
            _sortingService = sortingService;
            _analyticsService = analyticsService;
            _deepLinkingService = deepLinkingService;
            _notificationManager = notificationManager;
            _notificationService = notificationService;
            _azureFunctionsApiService = azureFunctionsApiService;

            App.Resumed += HandleAppResumed;
        }

        public static event EventHandler<NotificationHubInformation> InitializationCompleted
        {
            add => _initializationCompletedEventManager.AddEventHandler(value);
            remove => _initializationCompletedEventManager.RemoveEventHandler(value);
        }

        public static event EventHandler<SortingOption> SortingOptionRequested
        {
            add => _sortingOptionRequestedEventManager.AddEventHandler(value);
            remove => _sortingOptionRequestedEventManager.RemoveEventHandler(value);
        }

        public static event EventHandler<(bool isSuccessful, string errorMessage)> RegisterForNotificationsCompleted
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

        public async Task<bool> AreNotificationsEnabled()
        {
            bool? areNotificationsEnabled = await _notificationService.AreNotificationEnabled().ConfigureAwait(false);
            return areNotificationsEnabled ?? false;
        }

        public async Task Initialize(CancellationToken cancellationToken)
        {
            var notificationHubInformation = await GetNotificationHubInformation().ConfigureAwait(false);

            if (notificationHubInformation.IsEmpty())
            {
                await initalize().ConfigureAwait(false);
            }
            else
            {
                initalize().SafeFireAndForget();
            }

            async Task initalize()
            {
                try
                {
                    notificationHubInformation = await _azureFunctionsApiService.GetNotificationHubInformation(cancellationToken).ConfigureAwait(false);
                    await _secureStorage.SetAsync(_getNotificationHubInformationKey, JsonConvert.SerializeObject(notificationHubInformation)).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    _analyticsService.Report(e);
                }
                finally
                {
                    OnInitializationCompleted(notificationHubInformation);
                }
            }
        }

        public async Task<NotificationHubInformation> GetNotificationHubInformation()
        {
            var serializedToken = await _secureStorage.GetAsync(_getNotificationHubInformationKey).ConfigureAwait(false);

            try
            {
                var token = JsonConvert.DeserializeObject<NotificationHubInformation?>(serializedToken);

                return token ?? NotificationHubInformation.Empty;
            }
            catch (ArgumentNullException)
            {
                return NotificationHubInformation.Empty;
            }
            catch (JsonReaderException)
            {
                return NotificationHubInformation.Empty;
            }
        }

        public virtual void UnRegister() => ShouldSendNotifications = false;

        public async Task<AccessState> Register(bool shouldShowSettingsUI)
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
                        await _deepLinkingService.ShowSettingsUI().ConfigureAwait(false);
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
                if (finalNotificationRequestResult is AccessState.Available || finalNotificationRequestResult is AccessState.Restricted)
                    OnRegisterForNotificationsCompleted(true, string.Empty);
                else
                    OnRegisterForNotificationsCompleted(false, errorMessage);

                _settingsResultCompletionSource = null;

                _analyticsService.Track("Register For Notifications", new Dictionary<string, string>
                {
                    { nameof(initialNotificationRequestResult), initialNotificationRequestResult.ToString() },
                    { nameof(finalNotificationRequestResult), finalNotificationRequestResult?.ToString() ?? "null" },
                });
            }
        }

        public async ValueTask SetAppBadgeCount(int count)
        {
            if (HaveNotificationsBeenRequested)
            {
                //INotificationManager.Badge Crashes on iOS
                if (Device.RuntimePlatform is Device.iOS)
                    await _notificationService.SetiOSBadgeCount(count).ConfigureAwait(false);
                else
                    _notificationManager.Badge = count;
            }
        }

        public async ValueTask TrySendTrendingNotificaiton(IReadOnlyList<Repository> trendingRepositories, DateTimeOffset? notificationDateTime = null)
        {
            if (!ShouldSendNotifications)
                return;
#if DEBUG
            await SendTrendingNotification(trendingRepositories, notificationDateTime).ConfigureAwait(false);
#else
            var repositoriesToNotify = trendingRepositories.Where(shouldSendNotification).ToList();
            await SendTrendingNotification(repositoriesToNotify, notificationDateTime).ConfigureAwait(false);

            bool shouldSendNotification(Repository trendingRepository)
            {
                var nextNotificationDate = getMostRecentNotificationDate(trendingRepository).AddDays(3);
                return DateTime.Compare(nextNotificationDate, DateTime.UtcNow) < 1;

                DateTime getMostRecentNotificationDate(Repository repository) => _preferences.Get(repository.Name, default(DateTime));
            }
#endif
        }

        public async Task HandleNotification(string title, string message, int badgeCount)
        {
            if (badgeCount is 1)
            {
                var alertTitle = string.Format(NotificationConstants.HandleNotification_SingleTrendingRepository_Title, MostRecentTrendingRepositoryName);
                var shouldNavigateToChart = await _deepLinkingService.DisplayAlert(alertTitle,
                                                                                    NotificationConstants.HandleNotification_SingleTrendingRepository_Message,
                                                                                    NotificationConstants.HandleNotification_SingleTrendingRepository_Accept,
                                                                                    NotificationConstants.HandleNotification_SingleTrendingRepository_Decline).ConfigureAwait(false);

                _analyticsService.Track("Single Trending Repository Prompt Displayed", nameof(shouldNavigateToChart), shouldNavigateToChart.ToString());

                if (shouldNavigateToChart)
                {
                    //Create repository with only Name & Owner, because those are the only metrics that TrendsPage needs to fetch the chart data
                    var repository = new Repository(MostRecentTrendingRepositoryName, string.Empty, 0,
                                                    new RepositoryOwner(MostRecentTrendingRepositoryOwner, string.Empty),
                                                    null, string.Empty, new StarGazers(0), false);

                    await _deepLinkingService.NavigateToTrendsPage(repository).ConfigureAwait(false);
                }
            }
            else if (badgeCount > 1)
            {
                bool? shouldSortByTrending = null;

                if (!_sortingService.IsReversed)
                {
                    await _deepLinkingService.DisplayAlert(message,
                                                            NotificationConstants.HandleNotification_MultipleTrendingRepository_Sorted_Message,
                                                            NotificationConstants.HandleNotification_MultipleTrendingRepository_Cancel).ConfigureAwait(false);
                }
                else
                {
                    shouldSortByTrending = await _deepLinkingService.DisplayAlert(message,
                                                                                    NotificationConstants.HandleNotification_MultipleTrendingRepositor_Unsorted_Message,
                                                                                    NotificationConstants.HandleNotification_MultipleTrendingRepositor_Unsorted_Accept,
                                                                                    NotificationConstants.HandleNotification_MultipleTrendingRepositor_Unsorted_Decline).ConfigureAwait(false);
                }

                if (shouldSortByTrending is true)
                    OnSortingOptionRequestion(_sortingService.CurrentOption);

                _analyticsService.Track("Multiple Trending Repository Prompt Displayed", nameof(shouldSortByTrending), shouldSortByTrending?.ToString() ?? "null");
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(badgeCount), $"{badgeCount} must be greater than zero");
            }
        }

        async ValueTask SendTrendingNotification(IReadOnlyList<Repository> trendingRepositories, DateTimeOffset? notificationDateTime)
        {
            if (trendingRepositories.Count is 1)
            {
                var trendingRepository = trendingRepositories.First();

                var notification = new Notification
                {
                    //iOS crashes when ID is not set
                    Id = Device.RuntimePlatform is Device.iOS ? 1 : 0,
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
                    Id = Device.RuntimePlatform is Device.iOS ? 1 : 0,
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

        async void HandleAppResumed(object sender, EventArgs e)
        {
            if (_settingsResultCompletionSource != null)
            {
                var finalResult = await _notificationManager.RequestAccess().ConfigureAwait(false);
                _settingsResultCompletionSource.SetResult(finalResult);
            }
        }

        void OnInitializationCompleted(NotificationHubInformation notificationHubInformation) => _initializationCompletedEventManager.RaiseEvent(this, notificationHubInformation, nameof(InitializationCompleted));

        void OnSortingOptionRequestion(SortingOption sortingOption) => _sortingOptionRequestedEventManager.RaiseEvent(this, sortingOption, nameof(SortingOptionRequested));

        void OnRegisterForNotificationsCompleted(bool isSuccessful, string errorMessage) =>
            _registerForNotificationCompletedEventHandler.RaiseEvent(this, (isSuccessful, errorMessage), nameof(RegisterForNotificationsCompleted));
    }
}
