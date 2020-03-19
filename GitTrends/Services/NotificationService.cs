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
        const string _singleTrendingRepositoryNotificationMesage = "This repository is getting more traffic than usual! Tap here to see its chart.";
        const string _multipleTrendingRepositoriesNotificationTitle = "Your Repositories are Trending";
        const string _multipleTrendingReposiotiresNotificationMessage = "The folloing repositories are getting more traffic than usual:";

        readonly AnalyticsService _analyticsService;

        public NotificationService(AnalyticsService analyticsService) => _analyticsService = analyticsService;

        public ValueTask TrySendTrendingNotificaiton(in List<Repository> trendingRepositories, DateTimeOffset? notificationDateTime = null)
        {
            var repositoriesToNotify = trendingRepositories.Where(ShouldSendNotification).ToList();
#if AppStore
            return SendTrendingNotification(repositoriesToNotify, notificationDateTime);
#else
            return SendTrendingNotification(trendingRepositories, notificationDateTime);
#endif
        }

        public async Task HandleReceivedLocalNotification(string title, string message)
        {
            var baseNavigationPage = await GetBaseNavigationPage();

            if (isSingleTrendingRepositoryMessage(message))
            {
                var repositoryName = ParseRepositoryName(title);
                var ownerName = ParseOwnerName(title);

                var shouldNavigateToTrendsPage = await MainThread.InvokeOnMainThreadAsync(() =>
                     baseNavigationPage.DisplayAlert($"{repositoryName} is Trending", "Let's check out its chart!", "Let's Go", "Not Right Now"));

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
                        await MainThread.InvokeOnMainThreadAsync(() => baseNavigationPage.Navigation.PushAsync(trendsPage));
                    });
                }
            }
            else
            {
                _analyticsService.Track("Multiple Trending Repositories Prompt Displayed");

                await MainThread.InvokeOnMainThreadAsync(() => baseNavigationPage.DisplayAlert(title, message, "Thanks!"));
            }


            static bool isSingleTrendingRepositoryMessage(in string message) => message is _singleTrendingRepositoryNotificationMesage;
        }

        bool ShouldSendNotification(Repository trendingRepository)
        {
            var nextNotificationDate = getMostRecentNotificationDate(trendingRepository).AddDays(14);
            return DateTime.Compare(nextNotificationDate, DateTime.UtcNow) > 1;

            static DateTime getMostRecentNotificationDate(Repository repository) => Preferences.Get(repository.Name, default(DateTime));
        }

        async ValueTask SendTrendingNotification(List<Repository> trendingRepositories, DateTimeOffset? notificationDateTime)
        {
            if (trendingRepositories.Count is 1)
            {
                var trendingRepository = trendingRepositories.First();

                var notification = new Notification
                {
                    Title = CreateSingleRepositoryNotificationTitle(trendingRepository.Name, trendingRepository.OwnerLogin),
                    Message = _singleTrendingRepositoryNotificationMesage,
                    ScheduleDate = notificationDateTime
                };

                setMostRecentNotificationDate(trendingRepository);

                var notificationService = ShinyHost.Resolve<INotificationManager>();
                var result = await notificationService.RequestAccessAndSend(notification).ConfigureAwait(false);

                if (result.AccessStatus is AccessState.Available)
                    notificationService.Badge++;
            }
            else if (trendingRepositories.Count > 1)
            {
                var notificationMesageBuilder = new StringBuilder();
                notificationMesageBuilder.AppendLine(_multipleTrendingReposiotiresNotificationMessage);

                foreach (var repository in trendingRepositories)
                {
                    notificationMesageBuilder.AppendLine(createMultipleRepositoryNotificationMessageLine(repository.Name));
                    setMostRecentNotificationDate(repository);
                }

                var notification = new Notification
                {
                    Title = _multipleTrendingRepositoriesNotificationTitle,
                    Message = notificationMesageBuilder.ToString(),
                    ScheduleDate = notificationDateTime
                };

                var notificationService = ShinyHost.Resolve<INotificationManager>();
                var result = await notificationService.RequestAccessAndSend(notification).ConfigureAwait(false);

                if (result.AccessStatus is AccessState.Available)
                    ShinyNotifications.Badge = trendingRepositories.Count;
            }

            static void setMostRecentNotificationDate(Repository repository) => Preferences.Set(repository.Name, DateTime.UtcNow);
            static string createMultipleRepositoryNotificationMessageLine(in string repositoryName) => $"- {repositoryName}";
        }

        string CreateSingleRepositoryNotificationTitle(in string repositoryName, in string repositoryOwner) => $"{repositoryName} by {repositoryOwner} is Trending";
        string ParseRepositoryName(in string singleRepositoryNotificationTitle) => singleRepositoryNotificationTitle.Substring(0, singleRepositoryNotificationTitle.IndexOf(" "));
        string ParseOwnerName(in string singleRepositoryNotificationTitle)
        {
            var ownerNameIndex = singleRepositoryNotificationTitle.IndexOf("by") + "by".Length + 1;
            var ownerNameLength = singleRepositoryNotificationTitle.IndexOf(" ", ownerNameIndex);
            return singleRepositoryNotificationTitle.Substring(ownerNameIndex, ownerNameLength);
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
