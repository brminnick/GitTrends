using System.Threading.Tasks;

namespace GitTrends
{
    public interface IDeviceNotificationsService
    {
        void Initialize();
        Task SetiOSBadgeCount(int count);
        Task<bool?> AreNotificationEnabled();
    }
}
