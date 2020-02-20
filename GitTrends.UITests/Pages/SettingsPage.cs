using System.Linq;
using GitTrends.Mobile.Shared;
using Xamarin.UITest;
using Query = System.Func<Xamarin.UITest.Queries.AppQuery, Xamarin.UITest.Queries.AppQuery>;

namespace GitTrends.UITests
{
    class SettingsPage : BasePage
    {
        readonly Query _gitHubAvatarImage, _gitHubAliasLabel, _gitHubLoginButton,
            _gitHubSettingsViewActivityIndicator, _trendsChartSettingsLabel, _trendsChartSettingsControl;

        public SettingsPage(IApp app) : base(app, PageTitles.SettingsPage)
        {
            _gitHubAvatarImage = GenerateMarkedQuery(SettingsPageAutomationIds.GitHubAvatarImage);
            _gitHubAliasLabel = GenerateMarkedQuery(SettingsPageAutomationIds.GitHubAliasLabel);
            _gitHubLoginButton = GenerateMarkedQuery(SettingsPageAutomationIds.GitHubLoginButton);
            _gitHubSettingsViewActivityIndicator = GenerateMarkedQuery(SettingsPageAutomationIds.GitHubSettingsViewActivityIndicator);

            _trendsChartSettingsLabel = GenerateMarkedQuery(SettingsPageAutomationIds.TrendsChartSettingsLabel);
            _trendsChartSettingsControl = GenerateMarkedQuery(SettingsPageAutomationIds.TrendsChartSettingsControl);
        }

        public bool IsLoggedIn => App.Query(GitHubLoginButtonConstants.Disconnect).Any();

        public bool IsActivityIndicatorRunning => App.Query(_gitHubSettingsViewActivityIndicator).Any();

        public string GitHubAliasLabelText => App.Query(_gitHubAliasLabel).First().Text;

        public string GitHubButtonText => App.Query(_gitHubLoginButton).First().Text ?? App.Query(_gitHubLoginButton).First().Label;

        public string TrendsChartLabelText => App.Query(_trendsChartSettingsLabel).First().Text;

        public void TapGitHubButton()
        {
            App.Tap(_gitHubLoginButton);
            App.Screenshot("GitHub Button Tapped");
        }

        public void WaitForActivityIndicator()
        {
            App.WaitForElement(_gitHubSettingsViewActivityIndicator);
            App.Screenshot("Activity Indicator Appeared");
        }

        public void WaitForNoActivityIndicator()
        {
            App.WaitForNoElement(_gitHubSettingsViewActivityIndicator);
            App.Screenshot("Activity Indicator Disappeared");
        }

        public void TapBackButton()
        {
            App.Back();
            App.Screenshot("Back Button Tapped");
        }

        public void WaitForGitHubLoginToComplete()
        {
            App.WaitForNoElement(_gitHubSettingsViewActivityIndicator);
            App.WaitForElement(GitHubLoginButtonConstants.Disconnect);

            App.Screenshot("GitHub Login Completed");
        }

        public void WaitForGitHubLogoutToComplete()
        {
            App.WaitForNoElement(_gitHubSettingsViewActivityIndicator);
            App.WaitForElement(GitHubLoginButtonConstants.ConnectWithGitHub);

            App.Screenshot("GitHub Logout Completed");
        }
    }
}