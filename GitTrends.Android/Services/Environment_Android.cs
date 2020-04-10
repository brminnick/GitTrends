using System;
using System.Threading.Tasks;
using Android.Content;
using AndroidX.Work;
using Autofac;
using GitTrends.Droid;
using Plugin.CurrentActivity;
using Xamarin.Forms;

[assembly: Dependency(typeof(PlatformSpecificService_Android))]
namespace GitTrends.Droid
{
    public class PlatformSpecificService_Android : IPlatformSpecificService
    {

        public Task SetiOSBadgeCount(int count) => throw new NotSupportedException();

        public void EnqueueAndroidWorkRequest(TimeSpan repeatInterval)
        {
            var notifyTrendingRepositoriesRequest = PeriodicWorkRequest.Builder.From<NotifyTrendingRepositoriesWorker>(repeatInterval).AddTag(nameof(NotifyTrendingRepositoriesWorker)).Build();

            var workManager = WorkManager.GetInstance(CrossCurrentActivity.Current.AppContext);
            workManager.EnqueueUniquePeriodicWork(nameof(NotifyTrendingRepositoriesWorker), ExistingPeriodicWorkPolicy.Replace, notifyTrendingRepositoriesRequest);
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

            var backgroundFetchService = scope.Resolve<BackgroundFetchService>();
            var result = backgroundFetchService.NotifyTrendingRepositories().GetAwaiter().GetResult();

            return result ? Result.InvokeSuccess() : Result.InvokeFailure();
        }
    }
}