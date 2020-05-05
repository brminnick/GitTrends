using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Autofac;
using Autofac.Core;
using Firebase.Messaging;
using Microsoft.Azure.NotificationHubs;

namespace GitTrends.Droid
{
    [Service]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    public class FirebaseService : FirebaseMessagingService
    {
        public override async void OnNewToken(string token)
        {
            var hubClient = NotificationHubClient.CreateClientFromConnectionString(NotificationHubConstants.Name, NotificationHubConstants.ListenConnectionString);
            await hubClient.CreateFcmNativeRegistrationAsync(token).ConfigureAwait(false);
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
