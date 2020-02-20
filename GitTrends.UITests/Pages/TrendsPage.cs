using System;
using GitTrends.Mobile.Shared;
using Xamarin.UITest;
using Query = System.Func<Xamarin.UITest.Queries.AppQuery, Xamarin.UITest.Queries.AppQuery>;

namespace GitTrends.UITests
{
    class TrendsPage : BasePage
    {
        readonly Query _trendsChart, _trendsChartLegend, _trendsChartPrimaryAxis, _trendsChartSecondaryAxis,
            _totalViewsSeries, _totalUniqueViewsSeries, _totalClonesSeries, _totalUniqueClonesSeries,
            _activityIndicator;

        public TrendsPage(IApp app) : base(app)
        {
            _trendsChart = GenerateMarkedQuery(TrendsPageAutomationIds.TrendsChart);
            _trendsChartLegend = GenerateMarkedQuery(TrendsPageAutomationIds.TrendsChartLegend);
            _trendsChartPrimaryAxis = GenerateMarkedQuery(TrendsPageAutomationIds.TrendsChartPrimaryAxis);
            _trendsChartSecondaryAxis = GenerateMarkedQuery(TrendsPageAutomationIds.TrendsChartSecondaryAxis);

            _totalViewsSeries = GenerateMarkedQuery(TrendsPageAutomationIds.TotalViewsSeries);
            _totalUniqueViewsSeries = GenerateMarkedQuery(TrendsPageAutomationIds.TotalUniqueViewsSeries);
            _totalClonesSeries = GenerateMarkedQuery(TrendsPageAutomationIds.TotalClonesSeries);
            _totalUniqueClonesSeries = GenerateMarkedQuery(TrendsPageAutomationIds.TotalUniqueClonesSeries);

            _activityIndicator = GenerateMarkedQuery(TrendsPageAutomationIds.ActivityIndicator);
        }

        public override void WaitForPageToLoad(TimeSpan? timespan)
        {
            App.WaitForElement(_activityIndicator);
            App.WaitForNoElement(_activityIndicator);
            App.WaitForElement(_trendsChart);
        }
    }
}