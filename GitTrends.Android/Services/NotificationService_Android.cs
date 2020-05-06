using System;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using Autofac;
using Firebase.Messaging;
using GitTrends.Droid;
using Microsoft.Azure.NotificationHubs;
using Xamarin.Forms;

[assembly: Dependency(typeof(BadgeService_Android))]
namespace GitTrends.Droid
{
    public class BadgeService_Android : INotificationService
    {
        Context CurrentContext => Xamarin.Essentials.Platform.AppContext;

        public Task<bool?> AreNotificationEnabled()
        {
            bool isPushNotificationEnabled;

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O
                && CurrentContext.GetSystemService(Context.NotificationService) is NotificationManager notificationManager)
            {
                isPushNotificationEnabled = notificationManager.AreNotificationsEnabled();
            }
            else
            {
                isPushNotificationEnabled = NotificationManagerCompat.From(CurrentContext).AreNotificationsEnabled();
            }

            return Task.FromResult<bool?>(isPushNotificationEnabled);
        }

        public Task SetiOSBadgeCount(int count) => throw new NotSupportedException();
    }

    [Service, IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    public class FirebaseService : FirebaseMessagingService
    {
        public override async void OnNewToken(string token)
        {
            var hubClient = NotificationHubClient.CreateClientFromConnectionString(NotificationHubConstants.ListenConnectionString, NotificationHubConstants.Name);
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