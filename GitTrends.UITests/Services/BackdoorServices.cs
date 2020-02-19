using System;
using Xamarin.UITest;
using Xamarin.UITest.Android;
using Xamarin.UITest.iOS;

namespace GitTrends.UITests
{
    static class BackdoorServices
    {
        internal static void SetGitHubUser(IApp app, string token)
        {
            switch (app)
            {
                case iOSApp iosApp:
                    iosApp.Invoke("setGitHubUser:", token);
                    break;
                case AndroidApp androidApp:
                    androidApp.Invoke(nameof(SetGitHubUser), token);
                    break;
                default:
                    throw new NotSupportedException("Platform Not Supported");
            }
        }
    }
}
