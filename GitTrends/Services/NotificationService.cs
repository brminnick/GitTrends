using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
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
        const string _trendingRepositoriesNotificationTitle = "Your Repos Are Trending";
        const string _getNotificationHubInformationKey = "GetNotificationHubInformation";

        readonly WeakEventManager<(bool isSuccessful, string errorMessage)> _registerForNotificationCompletedEventHandler = new WeakEventManager<(bool isSuccessful, string errorMessage)>();
        readonly WeakEventManager<NotificationHubInformation> _initializationCompletedEventManager = new WeakEventManager<NotificationHubInformation>();
        readonly WeakEventManager<SortingOption> _sortingOptionRequestedEventManager = new WeakEventManager<SortingOption>();

        readonly IPreferences _preferences;
        readonly ISecureStorage _secureStorage;
        readonly SortingService _sortingService;
        readonly IAnalyticsService _analyticsService;
        readonly DeepLinkingService _deepLinkingService;
        readonly INotificationManager _notificationManager;
        readonly AzureFunctionsApiService _azureFunctionsApiService;

        TaskCompletionSource<AccessState>? _settingsResultCompletionSource;

        public NotificationService(IAnalyticsService analyticsService,
                                    DeepLinkingService deepLinkingService,
                                    SortingService sortingService,
                                    AzureFunctionsApiService azureFunctionsApiService,
                                    IPreferences preferences,
                                    ISecureStorage secureStorage,
                                    INotificationManager notificationManager)
        {
            _preferences = preferences;
            _secureStorage = secureStorage;
            _sortingService = sortingService;
            _analyticsService = analyticsService;
            _deepLinkingService = deepLinkingService;
            _azureFunctionsApiService = azureFunctionsApiService;
            _notificationManager = notificationManager;

            if (Application.Current is App app)
                app.Resumed += HandleAppResumed;
        }

        public event EventHandler<NotificationHubInformation> InitializationCompleted
        {
            add => _initializationCompletedEventManager.AddEventHandler(value);
            remove => _initializationCompletedEventManager.RemoveEventHandler(value);
        }

        public event EventHandler<SortingOption> SortingOptionRequested
        {
            add => _sortingOptionRequestedEventManager.AddEventHandler(value);
            remove => _sortingOptionRequestedEventManager.RemoveEventHandler(value);
        }

        public event EventHandler<(bool isSuccessful, string errorMessage)> RegisterForNotificationsCompleted
        {
            add => _registerForNotificationCompletedEventHandler.AddEventHandler(value);
            remove => _registerForNotificationCompletedEventHandler.RemoveEventHandler(value);
        }

        public bool ShouldSendNotifications
        {
            get => _preferences.Get(nameof(ShouldSendNotifications), false);
            private set => _preferences.Set(nameof(ShouldSendNotifications), value);
        }

        bool HaveNotificationsBeenRequested
        {
            get => _preferences.Get(nameof(HaveNotificationsBeenRequested), false);
            set => _preferences.Set(nameof(HaveNotificationsBeenRequested), value);
        }

        public async Task<bool> AreNotificationsEnabled()
        {
            bool? areNotificationsEnabled = await DependencyService.Get<INotificationService>().AreNotificationEnabled().ConfigureAwait(false);
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

        public void UnRegister() => ShouldSendNotifications = false;

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
                        errorMessage = "Notifications Disabled";
                        break;

                    case AccessState.NotSetup:
                        finalNotificationRequestResult = await _notificationManager.RequestAccess().ConfigureAwait(false);
                        break;

                    case AccessState.NotSupported:
                        errorMessage = "Notifications Are Not Supported";
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
                    await DependencyService.Get<INotificationService>().SetiOSBadgeCount(count).ConfigureAwait(false);
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

        public async Task HandleReceivedLocalNotification(string title, string message, int badgeCount)
        {
            if (badgeCount is 1)
            {
                var repositoryName = ParseRepositoryName(message);
                var ownerName = ParseOwnerName(message);

                var shouldNavigateToChart = await _deepLinkingService.DisplayAlert($"{repositoryName} is Trending", "Let's check out its chart", "Let's Go", "Not Right Now").ConfigureAwait(false);

                _analyticsService.Track("Single Trending Repository Prompt Displayed", nameof(shouldNavigateToChart), shouldNavigateToChart.ToString());

                if (shouldNavigateToChart)
                {
                    //Create repository with only Name & Owner, because those are the only metrics that TrendsPage needs to fetch the chart data
                    var repository = new Repository(repositoryName, string.Empty, 0,
                                                    new RepositoryOwner(ownerName, string.Empty),
                                                    null, string.Empty, new StarGazers(0), false);

                    await _deepLinkingService.NavigateToTrendsPage(repository).ConfigureAwait(false);
                }
            }
            else
            {
                bool? shouldSortByTrending = null;

                if (!_sortingService.IsReversed)
                {
                    await _deepLinkingService.DisplayAlert(message, $"Tap the repositories tagged \"Trending\" to see more details!", "Thanks").ConfigureAwait(false);
                }
                else
                {
                    shouldSortByTrending = await _deepLinkingService.DisplayAlert(message, "Reverse The Sorting Order To Discover Which Ones", "Reverse Sorting", "Not Now").ConfigureAwait(false);
                }

                if (shouldSortByTrending is true)
                    OnSortingOptionRequestion(_sortingService.CurrentOption);

                _analyticsService.Track("Multiple Trending Repository Prompt Displayed", nameof(shouldSortByTrending), shouldSortByTrending?.ToString() ?? "null");
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
                    Title = _trendingRepositoriesNotificationTitle,
                    Message = CreateSingleRepositoryNotificationMessage(trendingRepository.Name, trendingRepository.OwnerLogin),
                    ScheduleDate = notificationDateTime,
                    BadgeCount = 1
                };

                setMostRecentNotificationDate(trendingRepository);

                await _notificationManager.Send(notification).ConfigureAwait(false);

                _analyticsService.Track("Single Trending Repository Notification Sent");
            }
            else if (trendingRepositories.Count > 1)
            {
                foreach (var repository in trendingRepositories)
                {
                    setMostRecentNotificationDate(repository);
                }

                var notification = new Notification
                {
                    //iOS crashes when ID is not set
                    Id = Device.RuntimePlatform is Device.iOS ? 1 : 0,
                    Title = _trendingRepositoriesNotificationTitle,
                    Message = $"You Have {trendingRepositories.Count} Trending Repositories",
                    ScheduleDate = notificationDateTime,
                    BadgeCount = trendingRepositories.Count
                };

                await _notificationManager.Send(notification).ConfigureAwait(false);

                _analyticsService.Track("Multiple Trending Repositories Notification Sent", "Count", trendingRepositories.Count.ToString());
            }

            void setMostRecentNotificationDate(Repository repository) => _preferences.Set(repository.Name, DateTime.UtcNow);
        }

        string CreateSingleRepositoryNotificationMessage(in string repositoryName, in string repositoryOwner) => $"{repositoryName} by {repositoryOwner} is Trending";
        string ParseRepositoryName(in string? singleRepositoryNotificationMessage) => singleRepositoryNotificationMessage?.Substring(0, singleRepositoryNotificationMessage.IndexOf(" by ")) ?? string.Empty;
        string ParseOwnerName(in string? singleRepositoryNotificationMessage)
        {
            if (string.IsNullOrWhiteSpace(singleRepositoryNotificationMessage))
                return string.Empty;

            var ownerNameIndex = singleRepositoryNotificationMessage.IndexOf(" by ") + " by ".Length;
            var ownerNameLength = singleRepositoryNotificationMessage.IndexOf(" is Trending", ownerNameIndex) - ownerNameIndex;
            return singleRepositoryNotificationMessage.Substring(ownerNameIndex, ownerNameLength);
        }

        async void HandleAppResumed(object sender, EventArgs e)
        {
            if (_settingsResultCompletionSource != null)
            {
                var finalResult = await _notificationManager.RequestAccess().ConfigureAwait(false);
                _settingsResultCompletionSource.SetResult(finalResult);
            }
        }

        void OnInitializationCompleted(NotificationHubInformation notificationHubInformation) => _initializationCompletedEventManager.HandleEvent(this, notificationHubInformation, nameof(InitializationCompleted));

        void OnSortingOptionRequestion(SortingOption sortingOption) => _sortingOptionRequestedEventManager.HandleEvent(this, sortingOption, nameof(SortingOptionRequested));

        void OnRegisterForNotificationsCompleted(bool isSuccessful, string errorMessage) =>
            _registerForNotificationCompletedEventHandler.HandleEvent(this, (isSuccessful, errorMessage), nameof(RegisterForNotificationsCompleted));
    }
}
