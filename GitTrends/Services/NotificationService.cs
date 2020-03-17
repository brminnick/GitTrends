using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GitTrends.Shared;
using Shiny;
using Shiny.Notifications;
using Xamarin.Essentials;

namespace GitTrends
{
    class NotificationService
    {
        public Task TrySendTrendingNotificaiton(in List<Repository> trendingRepositories)
        {
            var repositoriesToNotify = trendingRepositories.Select(ShouldSendNotification);
            return SendTrendingNotification(trendingRepositories);
        }

        bool ShouldSendNotification(Repository trendingRepository)
        {
            var nextNotificationDate = getMostRecentNotificationDate(trendingRepository).AddDays(14);
            return DateTime.Compare(nextNotificationDate, DateTime.UtcNow) > 1;

            static DateTime getMostRecentNotificationDate(Repository repository) => Preferences.Get(repository.Name, default(DateTime));
        }

        Task SendTrendingNotification(in List<Repository> trendingRepositories)
        {
            if (trendingRepositories.Count is 1)
            {
                var trendingRepository = trendingRepositories.First();

                var notification = new Notification
                {
                    Title = $"{trendingRepository.Name} is Trending",
                    Message = "This repository is getting more traffic than usual! Tap here to see its chart.",
                };

                setMostRecentNotificationDate(trendingRepository);

                ShinyHost.Resolve<INotificationManager>().Badge++;

                return ShinyHost.Resolve<INotificationManager>().Send(notification);
            }
            else if (trendingRepositories.Count > 1)
            {
                var notificationMesageBuilder = new StringBuilder();
                notificationMesageBuilder.AppendLine("The folloing repositories are getting more traffic thank usual!");

                foreach (var repoitory in trendingRepositories)
                {
                    notificationMesageBuilder.AppendLine($"- {repoitory.Name}");
                    setMostRecentNotificationDate(repoitory);
                }

                var notification = new Notification
                {
                    Title = $"Your Repositories are Trending",
                    Message = notificationMesageBuilder.ToString(),
                };

                ShinyHost.Resolve<INotificationManager>().Badge = trendingRepositories.Count;

                return ShinyHost.Resolve<INotificationManager>().Send(notification);
            }

            return Task.CompletedTask;

            static void setMostRecentNotificationDate(Repository repository) => Preferences.Set(repository.Name, DateTime.UtcNow);
        }
    }
}
