using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using GitTrends.Droid;
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
}