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
            _trendsChart = GenerateQuery(TrendsPageAutomationIds.TrendsChart);
            _trendsChartLegend = GenerateQuery(TrendsPageAutomationIds.TrendsChartLegend);
            _trendsChartPrimaryAxis = GenerateQuery(TrendsPageAutomationIds.TrendsChartPrimaryAxis);
            _trendsChartSecondaryAxis = GenerateQuery(TrendsPageAutomationIds.TrendsChartSecondaryAxis);

            _totalViewsSeries = GenerateQuery(TrendsPageAutomationIds.TotalViewsSeries);
            _totalUniqueViewsSeries = GenerateQuery(TrendsPageAutomationIds.TotalUniqueViewsSeries);
            _totalClonesSeries = GenerateQuery(TrendsPageAutomationIds.TotalClonesSeries);
            _totalUniqueClonesSeries = GenerateQuery(TrendsPageAutomationIds.TotalUniqueClonesSeries);

            _activityIndicator = GenerateQuery(TrendsPageAutomationIds.ActivityIndicator);
        }

        public override void WaitForPageToLoad(TimeSpan? timespan)
        {
            App.WaitForElement(_activityIndicator);
            App.WaitForNoElement(_activityIndicator);
            App.WaitForElement(_trendsChart);
        }
    }
}