using System;
using System.Linq;
using GitTrends.Mobile.Shared;
using Xamarin.UITest;
using Query = System.Func<Xamarin.UITest.Queries.AppQuery, Xamarin.UITest.Queries.AppQuery>;

namespace GitTrends.UITests
{
    class SplashScreenPage : BasePage
    {
        readonly Query _gitTrendsImage, _statusLabel;

        public SplashScreenPage(IApp app) : base(app)
        {
            _gitTrendsImage = GenerateQuery(SplashScreenPageAutomationIds.GitTrendsImage);
            _statusLabel = GenerateQuery(SplashScreenPageAutomationIds.StatusLabel);
        }

        public string StatusLabelText => App.Query(_statusLabel).First().Text;

        public override void WaitForPageToLoad(TimeSpan? timespan = null) => App.WaitForElement(_gitTrendsImage);
    }
}
