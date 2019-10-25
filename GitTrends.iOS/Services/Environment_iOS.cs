using System;
using GitTrends.iOS;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(Environment_iOS))]
namespace GitTrends.iOS
{
    public class Environment_iOS : IEnvironment
    {
        public Theme GetOperatingSystemTheme()
        {
            var currentUIViewController = GetVisibleViewController();

            var userInterfaceStyle = currentUIViewController.TraitCollection.UserInterfaceStyle;

            return userInterfaceStyle switch
            {
                UIUserInterfaceStyle.Light => Theme.Light,
                UIUserInterfaceStyle.Dark => Theme.Dark,
                _ => throw new NotSupportedException($"UIUserInterfaceStyle {userInterfaceStyle} not supported"),
            };
        }

        static UIViewController GetVisibleViewController()
        {
            var rootController = UIApplication.SharedApplication.KeyWindow.RootViewController;

            return rootController.PresentedViewController switch
            {
                UINavigationController navigationController => navigationController.TopViewController,
                UITabBarController tabBarController => tabBarController.SelectedViewController,
                null => rootController,
                _ => rootController.PresentedViewController,
            };
        }
    }
}
