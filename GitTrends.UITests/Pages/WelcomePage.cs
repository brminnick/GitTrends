using System;
using System.Threading.Tasks;
using GitTrends.Mobile.Common;
using Xamarin.UITest;
using Query = System.Func<Xamarin.UITest.Queries.AppQuery, Xamarin.UITest.Queries.AppQuery>;

namespace GitTrends.UITests
{
    class WelcomePage : BasePage
    {
        readonly Query _connectToGitHubButton, _tryDemoButton, _activityIndicator;

        public WelcomePage(IApp app) : base(app)
        {
            _connectToGitHubButton = GenerateMarkedQuery(WelcomePageAutomationIds.ConnectToGitHubButton);
            _tryDemoButton = GenerateMarkedQuery(WelcomePageAutomationIds.DemoModeButton);
            _activityIndicator = GenerateMarkedQuery(WelcomePageAutomationIds.IsAuthenticatingActivityIndicator);
        }

        public override Task WaitForPageToLoad(TimeSpan? timeout = null)
        {
            App.WaitForElement(_tryDemoButton, timeout: timeout);
            App.WaitForNoElement(_activityIndicator, timeout: timeout);
            return Task.CompletedTask;
        }

        public void WaitForActivityIndicator()
        {
            App.WaitForElement(_activityIndicator);
            App.Screenshot("Activity Indicator Appeared");
        }

        public void WaitForNoActivityIndicator()
        {
            App.WaitForNoElement(_activityIndicator);
            App.Screenshot("Activity Indicator Disappeared");
        }

        public void TapConnectToGitHubButton()
        {
            App.Tap(_connectToGitHubButton);
            App.Screenshot("Connect to GitHub Button Tapped");
        }

        public void TapTryDemoButton()
        {
            App.Tap(_tryDemoButton);
            App.Screenshot("Try Demo Button Tapped");
        }
    }
}
