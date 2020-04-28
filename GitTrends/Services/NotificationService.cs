using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using GitTrends.Shared;
using Shiny;
using Shiny.Notifications;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace GitTrends
{
    public class NotificationService
    {
        const string _trendingRepositoriesNotificationTitle = "Your Repos Are Trending";

        readonly WeakEventManager<SortingOption> _sortingOptionRequestedEventManager = new WeakEventManager<SortingOption>();
        readonly WeakEventManager<(bool isSuccessful, string errorMessage)> _registerForNotificationCompletedEventHandler = new WeakEventManager<(bool isSuccessful, string errorMessage)>();

        readonly AnalyticsService _analyticsService;
        readonly DeepLinkingService _deepLinkingService;
        readonly SortingService _sortingService;

        TaskCompletionSource<AccessState>? _settingsResultCompletionSource;

        public NotificationService(AnalyticsService analyticsService,
                                    DeepLinkingService deepLinkingService,
                                    SortingService sortingService)
        {
            _analyticsService = analyticsService;
            _deepLinkingService = deepLinkingService;
            _sortingService = sortingService;

            var app = (App)Application.Current;
            app.Resumed += HandleAppResumed;
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
            get => Preferences.Get(nameof(ShouldSendNotifications), false);
            private set => Preferences.Set(nameof(ShouldSendNotifications), value);
        }

        static INotificationManager NotificationManager => ShinyHost.Resolve<INotificationManager>();

        bool HaveNotificationsBeenRequested
        {
            get => Preferences.Get(nameof(HaveNotificationsBeenRequested), false);
            set => Preferences.Set(nameof(HaveNotificationsBeenRequested), value);
        }

        public async Task<bool> AreNotificationsEnabled()
        {
            bool? areNotificationsEnabled = await DependencyService.Get<INotificationService>().AreNotificationEnabled().ConfigureAwait(false);
            return areNotificationsEnabled ?? false;
        }

        public void UnRegister() => ShouldSendNotifications = false;

        public async Task<AccessState> Register(bool shouldShowSettingsUI)
        {
            AccessState? finalNotificationRequestResult = null;
            HaveNotificationsBeenRequested = ShouldSendNotifications = true;

            var initialNotificationRequestResult = await NotificationManager.RequestAccess().ConfigureAwait(false);

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
                        OnRegisterForNotificationsCompleted(false, "Notifications Disabled");
                        break;

                    case AccessState.Available:
                    case AccessState.Restricted:
                        OnRegisterForNotificationsCompleted(true, string.Empty);
                        break;

                    case AccessState.NotSetup:
                        finalNotificationRequestResult = await NotificationManager.RequestAccess().ConfigureAwait(false);
                        break;

                    case AccessState.NotSupported:
                        OnRegisterForNotificationsCompleted(false, "Notifications Are Not Supported");
                        break;
                }

                return finalNotificationRequestResult ?? initialNotificationRequestResult;
            }
            catch (Exception e)
            {
                _analyticsService.Report(e);
                return initialNotificationRequestResult;
            }
            finally
            {
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
                    NotificationManager.Badge = count;
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

            static bool shouldSendNotification(Repository trendingRepository)
            {
                var nextNotificationDate = getMostRecentNotificationDate(trendingRepository).AddDays(3);
                return DateTime.Compare(nextNotificationDate, DateTime.UtcNow) < 1;

                static DateTime getMostRecentNotificationDate(Repository repository) => Preferences.Get(repository.Name, default(DateTime));
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

                await NotificationManager.Send(notification).ConfigureAwait(false);

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

                await NotificationManager.Send(notification).ConfigureAwait(false);

                _analyticsService.Track("Multiple Trending Repositories Notification Sent", "Count", trendingRepositories.Count.ToString());
            }

            static void setMostRecentNotificationDate(Repository repository) => Preferences.Set(repository.Name, DateTime.UtcNow);
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
                var finalResult = await NotificationManager.RequestAccess().ConfigureAwait(false);
                _settingsResultCompletionSource.SetResult(finalResult);
            }
        }

        void OnSortingOptionRequestion(SortingOption sortingOption) => _sortingOptionRequestedEventManager.HandleEvent(this, sortingOption, nameof(SortingOptionRequested));

        void OnRegisterForNotificationsCompleted(bool isSuccessful, string errorMessage) =>
            _registerForNotificationCompletedEventHandler.HandleEvent(this, (isSuccessful, errorMessage), nameof(RegisterForNotificationsCompleted));
    }
}
