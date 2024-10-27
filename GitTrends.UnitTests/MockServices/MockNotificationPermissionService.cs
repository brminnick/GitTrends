namespace GitTrends.UnitTests;

class MockNotificationPermissionService : INotificationPermissionStatus
{
	bool _areNotificationsEnabled;

	public Task<bool> AreNotificationsEnabled(CancellationToken token) => Task.FromResult(_areNotificationsEnabled);

	public void SetNotificationsEnabled(bool areNotificationsEnabled) => _areNotificationsEnabled = areNotificationsEnabled;
}