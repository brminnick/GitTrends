using System;
using System.Threading.Tasks;
using GitTrends.iOS;
using UIKit;
using Xamarin.Essentials;
using Xamarin.Forms;

[assembly: Dependency(typeof(Environment_iOS))]
namespace GitTrends.iOS
{
    public class Environment_iOS : IEnvironment
    {
        public Theme GetOperatingSystemTheme()
        {
            var currentUIViewController = ViewControllerServices.GetVisibleViewController();

            var userInterfaceStyle = currentUIViewController.TraitCollection.UserInterfaceStyle;

            return userInterfaceStyle switch
            {
                UIUserInterfaceStyle.Light => Theme.Light,
                UIUserInterfaceStyle.Dark => Theme.Dark,
                _ => throw new NotSupportedException($"UIUserInterfaceStyle {userInterfaceStyle} not supported"),
            };
        }

        public Task<Theme> GetOperatingSystemThemeAsync() => MainThread.InvokeOnMainThreadAsync(GetOperatingSystemTheme);

        public Task SetiOSBadgeCount(int count) => MainThread.InvokeOnMainThreadAsync(() => UIApplication.SharedApplication.ApplicationIconBadgeNumber = count);
    }
}
