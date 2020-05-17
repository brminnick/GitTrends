using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.NotificationHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GitTrends.Functions
{
    public class SendSilentPushNotification
    {
        const string _runEveryHourCron = "0 0 * * * *";

        readonly static string _notificationHubFullConnectionString_Debug = Environment.GetEnvironmentVariable("NotificationHubFullConnectionString_Debug") ?? string.Empty;
        readonly static string _notificationHubFullConnectionString = Environment.GetEnvironmentVariable("NotificationHubFullConnectionString") ?? string.Empty;

        readonly static Lazy<NotificationHubClient> _clientHolder = new Lazy<NotificationHubClient>(NotificationHubClient.CreateClientFromConnectionString(_notificationHubFullConnectionString, GetNotificationHubInformation.NotificationHubName));
        readonly static Lazy<NotificationHubClient> _debugClientHolder = new Lazy<NotificationHubClient>(NotificationHubClient.CreateClientFromConnectionString(_notificationHubFullConnectionString_Debug, GetNotificationHubInformation.NotificationHubName_Debug));

        static NotificationHubClient Client => _clientHolder.Value;
        static NotificationHubClient DebugClient => _debugClientHolder.Value;

        [FunctionName(nameof(SendSilentPushNotification))]
        public static Task Run([TimerTrigger(_runEveryHourCron)] TimerInfo myTimer, ILogger log) => Task.WhenAll(TrySendAppleSilentNotification(Client, log), TrySendFcmSilentNotification(Client, log));

        [FunctionName(nameof(SendSilentPushNotification) + "Debug")]
        public static async Task RunDebug([TimerTrigger(_runEveryHourCron, RunOnStartup = true)] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation(_notificationHubFullConnectionString);
            log.LogInformation(_notificationHubFullConnectionString_Debug);
            log.LogInformation(GetNotificationHubInformation.NotificationHubName);
            log.LogInformation(GetNotificationHubInformation.NotificationHubName_Debug);

            await Task.WhenAll(TrySendAppleSilentNotification(DebugClient, log), TrySendFcmSilentNotification(DebugClient, log)).ConfigureAwait(false);
        }

        static async Task TrySendAppleSilentNotification(NotificationHubClient client, ILogger log)
        {
            try
            {
                log.LogInformation("Sending Silent Apple Push Notifications");

                var jsonPayload = JsonConvert.SerializeObject(new ApplePushNotification());
                var notification = new AppleNotification(jsonPayload, new Dictionary<string, string>
                {
                    { "apns-push-type", "background" },
                    { "apns-priority", "5" }
                });

                var appleNotificationResult = await client.SendNotificationAsync(notification).ConfigureAwait(false);

                log.LogInformation("Apple Notifications Sent");
            }
            catch (Exception e)
            {
                log.LogInformation($"Apple Notification Failed\n{e}");
            }
        }

        static async Task TrySendFcmSilentNotification(NotificationHubClient client, ILogger log)
        {
            try
            {
                log.LogInformation("Sending Silent FCM Push Notifications");

                var jsonPayload = JsonConvert.SerializeObject(new FcmPushNotification());
                var fcmNotificationResult = await client.SendFcmNativeNotificationAsync(jsonPayload).ConfigureAwait(false);

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
            [JsonProperty("message")]
            public FcmPushNotificationBody Message { get; } = new FcmPushNotificationBody();
        }

        class FcmPushNotificationBody
        {
            [JsonProperty("data")]
            public Dictionary<string, string> Data { get; } = new Dictionary<string, string>();
        }
    }
}
