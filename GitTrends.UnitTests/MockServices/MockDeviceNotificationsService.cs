using System.Threading.Tasks;

namespace GitTrends.UnitTests
{
    public class MockDeviceNotificationsService : IDeviceNotificationsService
    {
        bool _areNotificationsEnabled;
        int _badgeCount;

        public Task<bool?> AreNotificationEnabled() => Task.FromResult((bool?)_areNotificationsEnabled);

        public void Initialize() => _areNotificationsEnabled = true;

        public Task SetiOSBadgeCount(int count)
        {
            _badgeCount = count;
            return Task.CompletedTask;
        }

        public void Reset() => _areNotificationsEnabled = false;
    }
}
