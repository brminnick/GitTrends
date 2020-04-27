using System;
using System.Threading;
using Autofac;
using BackgroundTasks;
using Foundation;
using GitTrends.iOS;
using Xamarin.Forms;

[assembly: Dependency(typeof(BackgroundFetchService_iOS))]
namespace GitTrends.iOS
{
    public class BackgroundFetchService_iOS : IBackgroundFetchService
    {
        public void Register()
        {
            var refreshSuccessNotificationName = new NSString(BackgroundFetchService.NotifyTrendingRepositoriesIdentifier);
            var isSuccessful = BGTaskScheduler.Shared.Register(BackgroundFetchService.NotifyTrendingRepositoriesIdentifier, null, RunBackgroundTask);

            if (!isSuccessful)
            {
                using var scope = ContainerService.Container.BeginLifetimeScope();
                scope.Resolve<AnalyticsService>().Track($"{nameof(Register)} Failed");
            }
        }

        async void RunBackgroundTask(BGTask task)
        {
            var backgroudTaskCancellationTokenSource = new CancellationTokenSource();
            task.ExpirationHandler = backgroudTaskCancellationTokenSource.Cancel;

            using var scope = ContainerService.Container.BeginLifetimeScope();
            var isSuccessful = await scope.Resolve<BackgroundFetchService>().NotifyTrendingRepositories(backgroudTaskCancellationTokenSource.Token);

            task.SetTaskCompleted(isSuccessful);
            SechduleAppRefresh();
        }

        void SechduleAppRefresh()
        {
            var request = new BGProcessingTaskRequest(BackgroundFetchService.NotifyTrendingRepositoriesIdentifier)
            {
                RequiresNetworkConnectivity = true,
                RequiresExternalPower = true
            };

            var isSuccessful = BGTaskScheduler.Shared.Submit(request, out var error);

            if (!isSuccessful)
            {
                using var scope = ContainerService.Container.BeginLifetimeScope();
                scope.Resolve<AnalyticsService>().Report(new ArgumentException(error.LocalizedDescription));
            }
        }
    }
}
