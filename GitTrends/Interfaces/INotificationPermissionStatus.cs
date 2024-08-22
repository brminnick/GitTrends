namespace GitTrends;

public interface INotificationPermissionStatus
{
	Task<bool> AreNotificationsEnabled(CancellationToken token);
}