using System.Threading.Tasks;

namespace GitTrends.UnitTests;

public class MockDeviceNotificationsService : IDeviceNotificationsService
{
	bool _areNotificationsEnabled;

	public Task<bool?> AreNotificationEnabled() => Task.FromResult((bool?)_areNotificationsEnabled);

	public void Initialize() => _areNotificationsEnabled = true;

	public void Reset() => _areNotificationsEnabled = false;
}