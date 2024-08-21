using System.Diagnostics;
using Android.App;
using Firebase.Messaging;
using GitTrends.Shared;
using Microsoft.Azure.NotificationHubs;

namespace GitTrends;

[Service(Exported = true), IntentFilter(["com.google.firebase.MESSAGING_EVENT"])]
public sealed class FirebaseService : FirebaseMessagingService
{
	static TaskCompletionSource<NotificationHubInformation>? _notificationHubInformationTCS;

	public override async void OnNewToken(string token)
	{
		var notificationService = IPlatformApplication.Current?.Services.GetRequiredService<NotificationService>();
		if (notificationService is null)
		{
			Trace.WriteLine($"Unable to complete {nameof(OnNewToken)} because {nameof(GitTrends.NotificationService)} is null");
			return;
		}

		_notificationHubInformationTCS = new TaskCompletionSource<NotificationHubInformation>();
		GitTrends.NotificationService.InitializationCompleted += HandleInitializationCompleted;

		try
		{
			var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
			var notificationHubInformation = await notificationService.GetNotificationHubInformation(cancellationTokenSource.Token).ConfigureAwait(false);

			if (notificationHubInformation.IsEmpty())
				notificationHubInformation = await _notificationHubInformationTCS.Task.ConfigureAwait(false);

			await RegisterWithNotificationHub(notificationHubInformation, token).ConfigureAwait(false);
		}
		catch (Exception e)
		{
			IPlatformApplication.Current?.Services.GetRequiredService<IAnalyticsService>().Report(e);
		}
		finally
		{
			GitTrends.NotificationService.InitializationCompleted -= HandleInitializationCompleted;
		}
	}

	public override void OnMessageReceived(RemoteMessage message)
	{
		base.OnMessageReceived(message);

		var backgroundFetchService = IPlatformApplication.Current?.Services.GetRequiredService<BackgroundFetchService>();
		if (backgroundFetchService is null)
		{
			Trace.WriteLine($"Unable to complete {nameof(OnMessageReceived)} because {nameof(BackgroundFetchService)} is null");
			return;
		}

		backgroundFetchService.TryScheduleCleanUpDatabase();
		backgroundFetchService.TryScheduleNotifyTrendingRepositories(CancellationToken.None);
	}

	static Task RegisterWithNotificationHub(in NotificationHubInformation notificationHubInformation, in string token)
	{
#if AppStore
		var hubClient = NotificationHubClient.CreateClientFromConnectionString(notificationHubInformation.ConnectionString, notificationHubInformation.Name);
#else
		if (notificationHubInformation.IsEmpty())
			return Task.CompletedTask;

		var hubClient = NotificationHubClient.CreateClientFromConnectionString(notificationHubInformation.ConnectionString_Debug, notificationHubInformation.Name_Debug);
#endif
		return hubClient.CreateFcmNativeRegistrationAsync(token);
	}

	static void HandleInitializationCompleted(object? sender, NotificationHubInformation e) => _notificationHubInformationTCS?.TrySetResult(e);
}