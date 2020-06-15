using System;
using System.Threading.Tasks;
using GitTrends.Mobile.Common;
using Xamarin.UITest;
using Query = System.Func<Xamarin.UITest.Queries.AppQuery, Xamarin.UITest.Queries.AppQuery>;

namespace GitTrends.UITests
{
    class SplashScreenPage : BasePage
    {
        readonly Query _gitTrendsImage, _statusLabel;

        public SplashScreenPage(IApp app) : base(app)
        {
            _gitTrendsImage = GenerateMarkedQuery(SplashScreenPageAutomationIds.GitTrendsImage);
            _statusLabel = GenerateMarkedQuery(SplashScreenPageAutomationIds.StatusLabel);
        }

        public string StatusLabelText => GetText(_statusLabel);

        public override Task WaitForPageToLoad(TimeSpan? timespan = null)
        {
            App.WaitForElement(_gitTrendsImage);
            return Task.CompletedTask;
        }
    }
}
