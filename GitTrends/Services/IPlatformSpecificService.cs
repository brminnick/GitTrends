using System;
using System.Threading.Tasks;

namespace GitTrends
{
    public interface IPlatformSpecificService
    {
        Task SetiOSBadgeCount(int count);
        void EnqueueAndroidWorkRequest(TimeSpan repeatInterval);
    }
}
