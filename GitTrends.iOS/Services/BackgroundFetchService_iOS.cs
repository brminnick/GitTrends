using System;
using System.Threading;
using Autofac;
using BackgroundTasks;
using GitTrends.iOS;
using Xamarin.Forms;

[assembly: Dependency(typeof(BackgroundFetchService_iOS))]
namespace GitTrends.iOS
{
    public class BackgroundFetchService_iOS : IBackgroundFetchService
    {
        public void Register()
        {
            var isNotfyTrendingRepositoriesSuccessful = BGTaskScheduler.Shared.Register(BackgroundFetchService.NotifyTrendingRepositoriesIdentifier, null, NotifyTrendingRepositoriesBackgroundTask);
            var isCleanupDatabaseSuccessful = BGTaskScheduler.Shared.Register(BackgroundFetchService.CleanUpDatabaseIdentifier, null, CleanUpDatabaseBackgroundTask);

            using var scope = ContainerService.Container.BeginLifetimeScope();
            var analyticsService = scope.Resolve<AnalyticsService>();

            if (!isNotfyTrendingRepositoriesSuccessful)
                analyticsService.Track($"Register Notify Trending Repositories Failed");

            if (!isCleanupDatabaseSuccessful)
                analyticsService.Track($"Register Cleanup Database Failed");
        }

        public void Scehdule()
        {
            var notifyTrendingRepositoriesRequest = new BGProcessingTaskRequest(BackgroundFetchService.NotifyTrendingRepositoriesIdentifier)
            {
                RequiresNetworkConnectivity = true,
                RequiresExternalPower = true
            };

            var cleanupDatabaseRequest = new BGProcessingTaskRequest(BackgroundFetchService.CleanUpDatabaseIdentifier);

            var isNotifyTrendingRepositoriesSuccessful = BGTaskScheduler.Shared.Submit(notifyTrendingRepositoriesRequest, out var notifyTrendingRepositoriesError);
            var isCleanupDatabaseSuccessful = BGTaskScheduler.Shared.Submit(cleanupDatabaseRequest, out var cleanUpDataBaseError);

            using var scope = ContainerService.Container.BeginLifetimeScope();
            var anayticsService = scope.Resolve<AnalyticsService>();

            if (!isNotifyTrendingRepositoriesSuccessful)
                anayticsService.Report(new ArgumentException(notifyTrendingRepositoriesError.LocalizedDescription));

            if (!isCleanupDatabaseSuccessful)
                anayticsService.Report(new ArgumentException(cleanUpDataBaseError.LocalizedDescription));
        }

        async void NotifyTrendingRepositoriesBackgroundTask(BGTask task)
        {
            var backgroudTaskCancellationTokenSource = new CancellationTokenSource();
            task.ExpirationHandler = backgroudTaskCancellationTokenSource.Cancel;

            using var scope = ContainerService.Container.BeginLifetimeScope();
            var isSuccessful = await scope.Resolve<BackgroundFetchService>().NotifyTrendingRepositories(backgroudTaskCancellationTokenSource.Token).ConfigureAwait(false);

            task.SetTaskCompleted(isSuccessful);
        }

        async void CleanUpDatabaseBackgroundTask(BGTask task)
        {
            var backgroudTaskCancellationTokenSource = new CancellationTokenSource();
            task.ExpirationHandler = backgroudTaskCancellationTokenSource.Cancel;

            using var scope = ContainerService.Container.BeginLifetimeScope();

            try
            {
                await scope.Resolve<BackgroundFetchService>().CleanUpDatabase().ConfigureAwait(false);
                task.SetTaskCompleted(true);
            }
            catch(Exception e)
            {
                scope.Resolve<AnalyticsService>().Report(e);
                task.SetTaskCompleted(false);
            }
        }
    }
}
