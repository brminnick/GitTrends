using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shiny;
using Shiny.Notifications;

namespace GitTrends.UnitTests
{
    public class MockNotificationManager : INotificationManager
    {
        readonly Dictionary<int, Notification> _pendingNotificationsDitcionary = new Dictionary<int, Notification>();
        readonly IDeviceNotificationsService _deviceNotificationsService;

        int _badge;

        public MockNotificationManager(IDeviceNotificationsService deviceNotificationsService) =>
            _deviceNotificationsService = deviceNotificationsService;

        public int Badge
        {
            get => _badge;
            set
            {
                _badge = value;
                _deviceNotificationsService.SetiOSBadgeCount(value);
            }
        }

        public Task Cancel(int id)
        {
            _pendingNotificationsDitcionary.Remove(id);
            return Task.CompletedTask;
        }

        public Task Clear()
        {
            _pendingNotificationsDitcionary.Clear();
            return Task.CompletedTask;
        }

        public Task<IEnumerable<Notification>> GetPending() => Task.FromResult(_pendingNotificationsDitcionary.Values.AsEnumerable());

        public void RegisterCategory(NotificationCategory category)
        {

        }

        public Task<AccessState> RequestAccess()
        {
            _deviceNotificationsService.Initialize();
            return Task.FromResult(AccessState.Available);
        }

        public Task Send(Notification notification)
        {
            _pendingNotificationsDitcionary.Add(notification.Id, notification);
            return Task.CompletedTask;
        }
    }
}
