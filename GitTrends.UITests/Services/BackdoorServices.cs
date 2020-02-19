using System;
using GitTrends.Mobile.Shared;
using Xamarin.UITest;
using Xamarin.UITest.Android;
using Xamarin.UITest.iOS;

namespace GitTrends.UITests
{
    static class BackdoorServices
    {
        public static void SetGitHubUser(IApp app, string accessToken)=>
            InvokeBackdoorMethod(app, BackdoorMethodConstants.SetGitHubUser, accessToken);

        public static void TriggerRepositoryPullToRefresh(IApp app) =>
            InvokeBackdoorMethod(app, BackdoorMethodConstants.TriggerRepositoriesPullToRefresh);

        static void InvokeBackdoorMethod(IApp app, string methodName, string? parameter = null)
        {
            switch (app)
            {
                case iOSApp iosApp:
                    iosApp.Invoke(methodName + ":", parameter);
                    break;
                case AndroidApp androidApp:
                    androidApp.Invoke(methodName, parameter);
                    break;
                default:
                    throw new NotSupportedException("Platform Not Supported");
            }
        }
    }
}
