using System.Threading.Tasks;
using GitTrends.iOS;
using UIKit;
using UserNotifications;
using Xamarin.Essentials;
using Xamarin.Forms;

[assembly: Dependency(typeof(DeviceNotificationsService_iOS))]
namespace GitTrends.iOS
{
    public class DeviceNotificationsService_iOS : IDeviceNotificationsService
    {
        public void Initialize() => NotificationService.RegisterForNotificationsCompleted += HandleRegisterForNotificationsCompleted;

        public Task<bool?> AreNotificationEnabled() => MainThread.InvokeOnMainThreadAsync(() =>
        {
            var tcs = new TaskCompletionSource<bool?>();

            UNUserNotificationCenter.Current.GetNotificationSettings(notificationSettings =>
            {
                var hasAlertNeverBeenRequested = notificationSettings.AlertSetting is UNNotificationSetting.NotSupported;
                var hasBadgeNeverBeenRequested = notificationSettings.BadgeSetting is UNNotificationSetting.NotSupported;
                var hasSoundsNeverBeenRequested = notificationSettings.SoundSetting is UNNotificationSetting.NotSupported;

                var isAlertEnabled = notificationSettings.AlertSetting is UNNotificationSetting.Enabled;
                var isBadgeEnabled = notificationSettings.BadgeSetting is UNNotificationSetting.Enabled;
                var isSoundsEnabled = notificationSettings.SoundSetting is UNNotificationSetting.Enabled;

                if (isAlertEnabled || isBadgeEnabled || isSoundsEnabled)
                    tcs.SetResult(true);
                else if (hasAlertNeverBeenRequested && hasBadgeNeverBeenRequested && hasSoundsNeverBeenRequested)
                    tcs.SetResult(null);
                else
                    tcs.SetResult(false);
            });

            return tcs.Task;
        });

        public Task SetiOSBadgeCount(int count) => MainThread.InvokeOnMainThreadAsync(() => UIApplication.SharedApplication.ApplicationIconBadgeNumber = count);

        async void HandleRegisterForNotificationsCompleted(object sender, (bool isSuccessful, string errorMessage) e)
        {
            if (e.isSuccessful)
                await MainThread.InvokeOnMainThreadAsync(UIApplication.SharedApplication.RegisterForRemoteNotifications).ConfigureAwait(false);
        }
    }
}
