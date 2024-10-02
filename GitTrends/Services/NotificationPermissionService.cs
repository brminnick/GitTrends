namespace GitTrends;

public class NotificationPermissionService : INotificationPermissionStatus
{
	public async Task<bool> AreNotificationsEnabled(CancellationToken token)
	{
		var result = await Permissions.CheckStatusAsync<Permissions.PostNotifications>().WaitAsync(token).ConfigureAwait(false);
		return result is PermissionStatus.Granted or PermissionStatus.Limited;
	}
}