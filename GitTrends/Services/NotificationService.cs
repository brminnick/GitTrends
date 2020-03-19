using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using GitTrends.Shared;
using Shiny;
using Shiny.Notifications;
using Xamarin.Essentials;

namespace GitTrends
{
    public class NotificationService
    {
        const string _trendingRepositoriesNotificationTitle = "Your Repositories are Trending";

        readonly AnalyticsService _analyticsService;

        public NotificationService(AnalyticsService analyticsService) => _analyticsService = analyticsService;

        public ValueTask TrySendTrendingNotificaiton(List<Repository> trendingRepositories, DateTimeOffset? notificationDateTime = null)
        {
#if !AppStore
            return SendTrendingNotification(trendingRepositories, notificationDateTime);
#else
            var repositoriesToNotify = trendingRepositories.Where(shouldSendNotification).ToList();
            return SendTrendingNotification(repositoriesToNotify, notificationDateTime);

            static bool shouldSendNotification(Repository trendingRepository)
            {
                var nextNotificationDate = getMostRecentNotificationDate(trendingRepository).AddDays(14);
                return DateTime.Compare(nextNotificationDate, DateTime.UtcNow) > 1;

                static DateTime getMostRecentNotificationDate(Repository repository) => Preferences.Get(repository.Name, default(DateTime));
            }
#endif
        }

        public async Task HandleReceivedLocalNotification(Notification notification)
        {
            var baseNavigationPage = await GetBaseNavigationPage();

            if (isSingleTrendingRepositoryMessage(notification))
            {
                var repositoryName = ParseRepositoryName(notification.Message);
                var ownerName = ParseOwnerName(notification.Message);

                var shouldNavigateToTrendsPage = await MainThread.InvokeOnMainThreadAsync(() =>
                     baseNavigationPage.DisplayAlert($"{repositoryName} is Trending", "Let's check out its chart", "Let's Go", "Not Right Now"));

                _analyticsService.Track("Single Trending Repository Prompt Displayed", "Did Accept Response", shouldNavigateToTrendsPage.ToString());

                if (shouldNavigateToTrendsPage)
                {
                    //Create repository with only Name & Owner, because those are the only metrics that TrendsPage needs to fetch the chart data
                    var repository = new Repository(repositoryName, string.Empty, 0,
                                                    new RepositoryOwner(ownerName, string.Empty),
                                                    null, string.Empty, new StarGazers(0), false);

                    using var scope = ContainerService.Container.BeginLifetimeScope();
                    var trendsPage = scope.Resolve<TrendsPage>(new TypedParameter(typeof(Repository), repository));

                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        await baseNavigationPage.PopToRootAsync();
                        await baseNavigationPage.Navigation.PushAsync(trendsPage);
                    });
                }
            }
            else
            {
                _analyticsService.Track("Multiple Trending Repositories Prompt Displayed");

                var messageBuilder = new StringBuilder();
                messageBuilder.AppendLine(notification.Message);
                messageBuilder.AppendLine();
                messageBuilder.Append("Sort by Trending to see them all");

                await MainThread.InvokeOnMainThreadAsync(() => baseNavigationPage.DisplayAlert(notification.Title, messageBuilder.ToString(), "Thanks!"));
            }

            static bool isSingleTrendingRepositoryMessage(in Notification notification) => notification.BadgeCount is 1;
        }

        async ValueTask SendTrendingNotification(List<Repository> trendingRepositories, DateTimeOffset? notificationDateTime)
        {
            if (trendingRepositories.Count is 1)
            {
                var trendingRepository = trendingRepositories.First();

                var notificationService = ShinyHost.Resolve<INotificationManager>();

                var notification = new Notification
                {
                    Title = _trendingRepositoriesNotificationTitle,
                    Message = CreateSingleRepositoryNotificationMessage(trendingRepository.Name, trendingRepository.OwnerLogin),
                    ScheduleDate = notificationDateTime,
                    BadgeCount = 1
                };

                setMostRecentNotificationDate(trendingRepository);


                notificationService.Badge++;
                await notificationService.Send(notification).ConfigureAwait(false);
            }
            else if (trendingRepositories.Count > 1)
            {
                foreach (var repository in trendingRepositories)
                {
                    setMostRecentNotificationDate(repository);
                }

                var notification = new Notification
                {
                    Title = _trendingRepositoriesNotificationTitle,
                    Message = CreateMultipleRepositoryNotifiationMessage(trendingRepositories.Count),
                    ScheduleDate = notificationDateTime,
                    BadgeCount = trendingRepositories.Count
                };

                var notificationService = ShinyHost.Resolve<INotificationManager>();
                await notificationService.Send(notification).ConfigureAwait(false);
            }

            static void setMostRecentNotificationDate(Repository repository) => Preferences.Set(repository.Name, DateTime.UtcNow);
        }

        string CreateMultipleRepositoryNotifiationMessage(int trendingRepoCount) => $"You have {trendingRepoCount} repos trending!";

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

        async ValueTask<BaseNavigationPage> GetBaseNavigationPage()
        {
            if (Xamarin.Forms.Application.Current.MainPage is BaseNavigationPage baseNavigationPage)
                return baseNavigationPage;

            var tcs = new TaskCompletionSource<BaseNavigationPage>();

            Xamarin.Forms.Application.Current.PageAppearing += HandlePageAppearing;

            return await tcs.Task.ConfigureAwait(false);

            void HandlePageAppearing(object sender, Xamarin.Forms.Page page)
            {
                if (page is BaseNavigationPage baseNavigationPage)
                {
                    Xamarin.Forms.Application.Current.PageAppearing -= HandlePageAppearing;
                    tcs.SetResult(baseNavigationPage);
                }
                else if (page.Parent is BaseNavigationPage baseNavigation)
                {
                    Xamarin.Forms.Application.Current.PageAppearing -= HandlePageAppearing;
                    tcs.SetResult(baseNavigation);
                }
            }
        }
    }
}
