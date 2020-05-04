using System;
using System.Threading;
using Android.Content;
using AndroidX.Work;
using Autofac;
using GitTrends.Droid;
using Xamarin.Forms;

[assembly: Dependency(typeof(BackgroundFetchService_Android))]
namespace GitTrends.Droid
{
    public class BackgroundFetchService_Android : IBackgroundFetchService
    {
        public void Register()
        {
            var notifyTrendingRepositoriesWorkRequest = PeriodicWorkRequest.Builder.From<NotifyTrendingRepositoriesWorker>(TimeSpan.FromHours(12)).Build();
            var cleanUpDatabaseWorkRequest = PeriodicWorkRequest.Builder.From<CleanUpDatabaseWorker>(TimeSpan.FromDays(15)).Build();

            var workManager = WorkManager.GetInstance(Xamarin.Essentials.Platform.AppContext);
            workManager.Enqueue(notifyTrendingRepositoriesWorkRequest);
            workManager.Enqueue(cleanUpDatabaseWorkRequest);
        }

        public void Scehdule()
        {
           
        }
    }

    class NotifyTrendingRepositoriesWorker : Worker
    {
        public NotifyTrendingRepositoriesWorker(Context context, WorkerParameters workerParameters) : base(context, workerParameters)
        {

        }

        public override Result DoWork()
        {
            using var scope = ContainerService.Container.BeginLifetimeScope();
            var isSuccessful = scope.Resolve<BackgroundFetchService>().NotifyTrendingRepositories(CancellationToken.None).GetAwaiter().GetResult();

            return isSuccessful ? Result.InvokeSuccess() : Result.InvokeFailure();
        }
    }

    class CleanUpDatabaseWorker : Worker
    {
        public CleanUpDatabaseWorker(Context context, WorkerParameters workerParameters) : base(context, workerParameters)
        {

        }

        public override Result DoWork()
        {
            using var scope = ContainerService.Container.BeginLifetimeScope();
            try
            {
                scope.Resolve<BackgroundFetchService>().CleanUpDatabase().GetAwaiter().GetResult();
                return Result.InvokeSuccess();
            }
            catch(Exception e)
            {
                scope.Resolve<AnalyticsService>().Report(e);
                return Result.InvokeFailure();
            }
        }
    }
}
