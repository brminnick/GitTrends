using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Autofac;
using Autofac.Core;
using Firebase.Messaging;
using WindowsAzure.Messaging;

namespace GitTrends.Droid
{
    [Service]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    public class FirebaseService : FirebaseMessagingService
    {
        public override void OnNewToken(string token)
        {
            var hub = new NotificationHub(NotificationHubConstants.Name, NotificationHubConstants.ListenConnectionString, this);
            hub.Register(token);
        }

        public override async void OnMessageReceived(RemoteMessage message)
        {
            base.OnMessageReceived(message);

            using var scope = ContainerService.Container.BeginLifetimeScope();
            var backgroundFetchService = scope.Resolve<BackgroundFetchService>();

            await Task.WhenAll(backgroundFetchService.CleanUpDatabase(), backgroundFetchService.NotifyTrendingRepositories(CancellationToken.None));
        }
    }
}
