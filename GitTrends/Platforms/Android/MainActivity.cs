using System.Text.Json;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using GitTrends.Common;

namespace GitTrends;

[Activity(Label = "GitTrends", Exported = true, Icon = "@mipmap/icon", RoundIcon = "@mipmap/icon_round", Theme = "@style/LaunchTheme", LaunchMode = LaunchMode.SingleTop, MainLauncher = true, ScreenOrientation = ScreenOrientation.Portrait)]
[IntentFilter([Intent.ActionView], Categories = [Intent.CategoryDefault, Intent.CategoryBrowsable], DataSchemes = ["gittrends"])]
[IntentFilter([Shiny.ShinyNotificationIntents.NotificationClickAction], Categories = ["android.intent.category.DEFAULT"])]
public class MainActivity : MauiAppCompatActivity
{
	protected override void OnCreate(Bundle? savedInstanceState)
	{
		base.SetTheme(Resource.Style.MainTheme);
		base.OnCreate(savedInstanceState);

		TryHandleOpenedFromUri(Intent?.Data);
		TryHandleOpenedFromNotification(Intent);
	}

	protected override async void OnNewIntent(Intent? intent)
	{
		base.OnNewIntent(intent);

		if (intent?.Data is Android.Net.Uri callbackUri)
		{
			await AuthorizeGitHubSession(callbackUri).ConfigureAwait(false);
		}

		TryHandleOpenedFromNotification(intent);
	}

	static async Task AuthorizeGitHubSession(Android.Net.Uri callbackUri)
	{
		try
		{
			var gitHubAuthenticationService = IPlatformApplication.Current?.Services.GetRequiredService<GitHubAuthenticationService>() ?? throw new InvalidOperationException("Platform Application Cannot be Null");

			if (callbackUri.ToString() is string callBackUriString)
				await gitHubAuthenticationService.AuthorizeSession(new Uri(callBackUriString), CancellationToken.None).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			IPlatformApplication.Current?.Services.GetRequiredService<IAnalyticsService>().Report(ex);
		}
	}

	static async void TryHandleOpenedFromNotification(Intent? intent)
	{
		try
		{
			if (intent?.GetStringExtra("ShinyNotification") is string notificationString)
			{
				var notification = JsonSerializer.Deserialize<Shiny.Notifications.Notification>(notificationString);
				var notificationService = IPlatformApplication.Current?.Services.GetRequiredService<NotificationService>();

				if (notificationService is not null
					&& notification is
					{
						Title: string notificationTitle,
						Message: string notificationMessage,
						BadgeCount: int badgeCount and > 0
					})
				{
					await notificationService.HandleNotification(notificationTitle, notificationMessage, badgeCount, CancellationToken.None).ConfigureAwait(false);
				}
			}
		}
		catch (ObjectDisposedException)
		{

		}
	}

	static async void TryHandleOpenedFromUri(Android.Net.Uri? callbackUri)
	{
		if (callbackUri is not null)
		{
			await AuthorizeGitHubSession(callbackUri).ConfigureAwait(false);
		}
	}
}