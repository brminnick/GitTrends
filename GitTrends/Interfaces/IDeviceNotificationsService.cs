namespace GitTrends;

public interface IDeviceNotificationsService
{
	void Initialize();
	Task<bool?> AreNotificationEnabled(CancellationToken token);
}