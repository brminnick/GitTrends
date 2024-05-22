using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.NotificationHubs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GitTrends.Functions;

public class SendSilentPushNotification
{
	const string _runTwicePerDay = "0 0 */12 * * *";

	static readonly string _notificationHubFullConnectionString_Debug = Environment.GetEnvironmentVariable("NotificationHubFullConnectionString_Debug") ?? string.Empty;
	static readonly string _notificationHubFullConnectionString = Environment.GetEnvironmentVariable("NotificationHubFullConnectionString") ?? string.Empty;

	static readonly Lazy<NotificationHubClient> _clientHolder = new(NotificationHubClient.CreateClientFromConnectionString(_notificationHubFullConnectionString, GetNotificationHubInformation.NotificationHubName));
	static readonly Lazy<NotificationHubClient> _debugClientHolder = new(NotificationHubClient.CreateClientFromConnectionString(_notificationHubFullConnectionString_Debug, GetNotificationHubInformation.NotificationHubName_Debug));

	static NotificationHubClient Client => _clientHolder.Value;
	static NotificationHubClient DebugClient => _debugClientHolder.Value;

	[Function(nameof(SendSilentPushNotification))]
	public static Task Run([TimerTrigger(_runTwicePerDay)] TimerInfo myTimer, FunctionContext functionContext)
	{
		var logger = functionContext.GetLogger<SendSilentPushNotification>();

		return Task.WhenAll(TrySendAppleSilentNotification(Client, logger), TrySendFcmSilentNotification(Client, logger));
	}

	[Function(nameof(SendSilentPushNotification) + "Debug")]
	public static Task RunDebug([TimerTrigger(_runTwicePerDay)] TimerInfo myTimer, FunctionContext functionContext)
	{
		var logger = functionContext.GetLogger<SendSilentPushNotification>();

		logger.LogInformation(_notificationHubFullConnectionString);
		logger.LogInformation(_notificationHubFullConnectionString_Debug);
		logger.LogInformation(GetNotificationHubInformation.NotificationHubName);
		logger.LogInformation(GetNotificationHubInformation.NotificationHubName_Debug);

		return Task.WhenAll(TrySendAppleSilentNotification(DebugClient, logger), TrySendFcmSilentNotification(DebugClient, logger));
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
		public ApplePushNotificationBody Content { get; } = new();
	}

	class ApplePushNotificationBody
	{
		[JsonProperty("content-available")]
		public int ContentAvailable { get; } = 1;
	}

	class FcmPushNotification
	{
		[JsonProperty("message")]
		public FcmPushNotificationBody Message { get; } = new();
	}

	class FcmPushNotificationBody
	{
		[JsonProperty("data")]
		public IReadOnlyDictionary<string, string> Data { get; } = new Dictionary<string, string>();
	}
}