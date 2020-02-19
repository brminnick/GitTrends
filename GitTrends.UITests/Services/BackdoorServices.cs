using System;
using Xamarin.UITest;
using Xamarin.UITest.Android;
using Xamarin.UITest.iOS;

namespace GitTrends.UITests
{
    static class BackdoorServices
    {
        internal static void SetGitHubUser(IApp app, string accessToken)
        {
            switch (app)
            {
                case iOSApp iosApp:
                    iosApp.Invoke("setGitHubUser:", accessToken);
                    break;
                case AndroidApp androidApp:
                    androidApp.Invoke(nameof(SetGitHubUser), accessToken);
                    break;
                default:
                    throw new NotSupportedException("Platform Not Supported");
            }
        }
    }
}
