using System.Threading.Tasks;

namespace GitTrends
{
    public interface INotificationService
    {
        Task SetiOSBadgeCount(int count);
        Task<bool?> AreNotificationEnabled();
        void Initialize();
    }
}
