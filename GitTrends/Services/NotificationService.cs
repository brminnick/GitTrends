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

        readonly AnalyticsService _analyticsService;
        readonly DeepLinkingService _deepLinkingService;
        readonly SortingService _sortingService;

        public NotificationService(AnalyticsService analyticsService, DeepLinkingService deepLinkingService, SortingService sortingService) =>
            (_analyticsService, _deepLinkingService, _sortingService) = (analyticsService, deepLinkingService, sortingService);

        public event EventHandler<SortingOption> SortingOptionRequested
        {
            add => _sortingOptionRequestedEventManager.AddEventHandler(value);
            remove => _sortingOptionRequestedEventManager.RemoveEventHandler(value);
        }

        static INotificationManager NotificationManager => ShinyHost.Resolve<INotificationManager>();

        bool HaveNotificationsBeenRequested
        {
            get => Preferences.Get(nameof(HaveNotificationsBeenRequested), false);
            set => Preferences.Set(nameof(HaveNotificationsBeenRequested), value);
        }

        public Task<AccessState> Register()
        {
            HaveNotificationsBeenRequested = true;
            return NotificationManager.RequestAccess();
        }

        public async ValueTask SetAppBadgeCount(int count)
        {
            if (!HaveNotificationsBeenRequested)
                return;

            var accessState = await Register().ConfigureAwait(false);

            //INotificationManager.Badge Crashes on iOS
            if (accessState is AccessState.Available && Device.RuntimePlatform is Device.iOS)
                await DependencyService.Get<IEnvironment>().SetiOSBadgeCount(count).ConfigureAwait(false);
            else if (accessState is AccessState.Available)
                NotificationManager.Badge = count;
        }

        public async ValueTask TrySendTrendingNotificaiton(List<Repository> trendingRepositories, DateTimeOffset? notificationDateTime = null)
        {
            if (!HaveNotificationsBeenRequested)
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
                    await _deepLinkingService.DisplayAlert(message, $"Tap the repositories tagged \"Trending\" to learn more!", "Thanks").ConfigureAwait(false);
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

        async ValueTask SendTrendingNotification(List<Repository> trendingRepositories, DateTimeOffset? notificationDateTime)
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

        void OnSortingOptionRequestion(SortingOption sortingOption) => _sortingOptionRequestedEventManager.HandleEvent(this, sortingOption, nameof(SortingOptionRequested));
    }
}
