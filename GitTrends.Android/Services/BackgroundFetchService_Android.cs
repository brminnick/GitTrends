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
            WorkManager.GetInstance(Xamarin.Essentials.Platform.AppContext).Enqueue(notifyTrendingRepositoriesWorkRequest);
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
}
