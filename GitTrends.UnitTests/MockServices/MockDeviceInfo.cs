using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace GitTrends.UnitTests
{
    class MockDeviceInfo : DeviceInfo
    {
        public override Size PixelScreenSize => new Size(0, 0);

        public override Size ScaledScreenSize => new Size(0, 0);

        public override double ScalingFactor => 1;
    }
}
