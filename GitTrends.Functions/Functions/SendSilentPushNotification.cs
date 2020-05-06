using System;
using System.Threading.Tasks;
using Microsoft.Azure.NotificationHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GitTrends.Functions
{
    public class SendSilentPushNotification
    {
#if DEBUG
        readonly static Lazy<NotificationHubClient> _clientHolder = new Lazy<NotificationHubClient>(NotificationHubClient.CreateClientFromConnectionString(GetNotificationHubInformation.NotificationHubConnectionString_Debug, GetNotificationHubInformation.NotificationHubName_Debug));
#else
        readonly static Lazy<NotificationHubClient> _clientHolder = new Lazy<NotificationHubClient>(NotificationHubClient.CreateClientFromConnectionString(GetNotificationHubInformation.NotificationHubConnectionString, GetNotificationHubInformation.NotificationHubName));
#endif

        static NotificationHubClient Client => _clientHolder.Value;

        [FunctionName(nameof(SendSilentPushNotification))]
        public static Task Run([TimerTrigger("0 0 0/12 * * *")] TimerInfo myTimer, ILogger log) => Task.WhenAll(TrySendAppleSilentNotification(log), TrySendFcmSilentNotification(log));

        static async Task TrySendAppleSilentNotification(ILogger log)
        {
            try
            {
                log.LogInformation("Sending Silent Apple Push Notifications");

                var jsonPayload = JsonConvert.SerializeObject(new ApplePushNotification());
                var appleNotificationResult = await Client.SendAppleNativeNotificationAsync(jsonPayload).ConfigureAwait(false);

                log.LogInformation("Apple Notifications Sent");
            }
            catch (Exception e)
            {
                log.LogInformation($"Apple Notification Failed\n{e}");
            }
        }

        static async Task TrySendFcmSilentNotification(ILogger log)
        {
            try
            {
                log.LogInformation("Sending Silent FCM Push Notifications");

                var jsonPayload = JsonConvert.SerializeObject(new FcmPushNotification());
                var fcmNotificationResult = await Client.SendFcmNativeNotificationAsync(jsonPayload).ConfigureAwait(false);

                log.LogInformation("FCM Notifications Sent");
            }
            catch (Exception e)
            {
                log.LogInformation($"FCM Notification Failed\n{e}");
            }
        }

        class ApplePushNotification
        {
            [JsonProperty("aps")]
            public ApplePushNotificationBody Content { get; } = new ApplePushNotificationBody();
        }

        class ApplePushNotificationBody
        {
            [JsonProperty("content-available")]
            public int ContentAvailable { get; } = 1;
        }

        class FcmPushNotification
        {
            [JsonProperty("priority")]
            public string Priority { get; } = "high";

            [JsonProperty("content_available")]
            public bool ContentAvailable { get; } = true;
        }
    }
}
