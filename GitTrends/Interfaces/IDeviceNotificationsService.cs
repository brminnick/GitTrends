using System.Threading.Tasks;

namespace GitTrends;

public interface IDeviceNotificationsService
{
	void Initialize();
	Task<bool?> AreNotificationEnabled();
}