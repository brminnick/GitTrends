using Xamarin.UITest;
using GitTrends.Mobile.Shared;
using Query = System.Func<Xamarin.UITest.Queries.AppQuery, Xamarin.UITest.Queries.AppQuery>;
using System.Linq;

namespace GitTrends.UITests
{
    class SettingsPage : BasePage
    {
        readonly Query _gitHubAvatarImage, _gitHubAliasLabel, _gitHubLoginButton,
            _gitHubSettingsViewActivityIndicator, _trendsChartSettingsLabel, _trendsChartSettingsControl;

        public SettingsPage(IApp app) : base(app, PageTitles.SettingsPage)
        {
            _gitHubAvatarImage = GenerateQuery(SettingsPageAutomationIds.GitHubAvatarImage);
            _gitHubAliasLabel = GenerateQuery(SettingsPageAutomationIds.GitHubAliasLabel);
            _gitHubLoginButton = GenerateQuery(SettingsPageAutomationIds.GitHubLoginButton);
            _gitHubSettingsViewActivityIndicator = GenerateQuery(SettingsPageAutomationIds.GitHubSettingsViewActivityIndicator);

            _trendsChartSettingsLabel = GenerateQuery(SettingsPageAutomationIds.TrendsChartSettingsLabel);
            _trendsChartSettingsControl = GenerateQuery(SettingsPageAutomationIds.TrendsChartSettingsControl);
        }

        public bool IsActivityIndicatorRunning => App.Query(_gitHubSettingsViewActivityIndicator).Any();
        public string GitHubAliasLabelText => App.Query(_gitHubAliasLabel).First().Text;
        public string TrendsChartLabelText => App.Query(_trendsChartSettingsLabel).First().Text;

        public void WaitForActivityIndicator() => App.WaitForElement(_gitHubSettingsViewActivityIndicator);
        public void WaitForNoActivityIndicator() => App.WaitForNoElement(_gitHubSettingsViewActivityIndicator);
    }
}