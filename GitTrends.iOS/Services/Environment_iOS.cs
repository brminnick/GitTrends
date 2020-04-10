using System;
using System.Threading.Tasks;
using GitTrends.iOS;
using UIKit;
using Xamarin.Essentials;
using Xamarin.Forms;

[assembly: Dependency(typeof(PlatformSpecificService_iOS))]
namespace GitTrends.iOS
{
    public class PlatformSpecificService_iOS : IPlatformSpecificService
    {

        public Task SetiOSBadgeCount(int count) => MainThread.InvokeOnMainThreadAsync(() => UIApplication.SharedApplication.ApplicationIconBadgeNumber = count);

        public void EnqueueAndroidWorkRequest(TimeSpan repeatInterval) => throw new NotSupportedException();
    }
}
