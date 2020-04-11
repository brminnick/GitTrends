using System;
using System.Linq;
using System.Threading.Tasks;
using GitTrends.Mobile.Shared;
using Newtonsoft.Json;
using Xamarin.UITest;
using Xamarin.UITest.Android;
using Query = System.Func<Xamarin.UITest.Queries.AppQuery, Xamarin.UITest.Queries.AppQuery>;

namespace GitTrends.UITests
{
    class TrendsPage : BasePage
    {
        readonly Query _trendsChart, _trendsChartLegend, _trendsChartPrimaryAxis, _trendsChartSecondaryAxis,
            _activityIndicator, _androidContextMenuOverflowButton, _referringSiteButton, _viewsLegendIcon,
            _uniqueViewsLegendIcon, _clonesLegendIcon, _uniqueClonesLegendIcon;

        public TrendsPage(IApp app) : base(app)
        {
            _trendsChart = GenerateMarkedQuery(TrendsPageAutomationIds.TrendsChart);
            _trendsChartLegend = GenerateMarkedQuery(TrendsPageAutomationIds.TrendsChartLegend);
            _trendsChartPrimaryAxis = GenerateMarkedQuery(TrendsPageAutomationIds.TrendsChartPrimaryAxis);
            _trendsChartSecondaryAxis = GenerateMarkedQuery(TrendsPageAutomationIds.TrendsChartSecondaryAxis);

            _viewsLegendIcon = GenerateMarkedQuery(TrendsChartConstants.TotalViewsTitle);
            _uniqueViewsLegendIcon = GenerateMarkedQuery(TrendsChartConstants.UniqueViewsTitle);
            _clonesLegendIcon = GenerateMarkedQuery(TrendsChartConstants.TotalClonesTitle);
            _uniqueClonesLegendIcon = GenerateMarkedQuery(TrendsChartConstants.UniqueClonesTitle);

            _activityIndicator = GenerateMarkedQuery(TrendsPageAutomationIds.ActivityIndicator);

            _androidContextMenuOverflowButton = x => x.Class("androidx.appcompat.widget.ActionMenuPresenter$OverflowMenuButton");
            _referringSiteButton = GenerateMarkedQuery(TrendsPageAutomationIds.ReferringSitesButton);
        }

        public override Task WaitForPageToLoad(TimeSpan? timespan = null)
        {
            try
            {
                App.WaitForElement(_activityIndicator, timeout: TimeSpan.FromSeconds(2));
            }
            catch
            {

            }

            App.WaitForNoElement(_activityIndicator, timeout: timespan);
            App.WaitForElement(_trendsChart, timeout: timespan);

            return Task.CompletedTask;
        }

        public bool IsSeriesVisible(string seriesName)
        {
            var serializedIsSeriesVisible = App.InvokeBackdoorMethod(BackdoorMethodConstants.IsTrendsSeriesVisible, seriesName).ToString();
            return JsonConvert.DeserializeObject<bool>(serializedIsSeriesVisible);
        }

        public void TapViewsLegendIcon()
        {
            App.Tap(_viewsLegendIcon);
            App.Screenshot("Views Legend Icon Tapped");
        }

        public void TapUniqueViewsLegendIcon()
        {
            App.Tap(_uniqueViewsLegendIcon);
            App.Screenshot("Unique Views Legend Icon Tapped");
        }

        public void TapClonesLegendIcon()
        {
            App.Tap(_clonesLegendIcon);
            App.Screenshot("Clones Legend Icon Tapped");
        }

        public void TapUniqueClonesLegendIcon()
        {
            App.Tap(_uniqueClonesLegendIcon);
            App.Screenshot("Unique Clones Legend Icon Tapped");
        }

        public void TapReferringSitesButton()
        {
            App.Tap(_referringSiteButton);
            App.Screenshot("Referring Sites Button Tapped");
        }
    }
}