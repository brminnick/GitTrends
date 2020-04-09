using System;
using System.Linq;
using System.Threading.Tasks;
using GitTrends.Mobile.Shared;
using Newtonsoft.Json;
using Xamarin.UITest;
using Query = System.Func<Xamarin.UITest.Queries.AppQuery, Xamarin.UITest.Queries.AppQuery>;

namespace GitTrends.UITests
{
    class TrendsPage : BasePage
    {
        readonly Query _trendsChart, _trendsChartLegend, _trendsChartPrimaryAxis, _trendsChartSecondaryAxis,
            _activityIndicator, _androidContextMenuOverflowButton, _referringSiteButton, _viewsLegendIcon,
            _uniqueViewsLegendIcon, _clonesLegendIcon, _uniqueClonesLegendIcon, _viewsCard,
            _uniqueViewsCard, _clonesCard, _uniqueClonesCard;

        public TrendsPage(IApp app) : base(app)
        {
            _trendsChart = GenerateMarkedQuery(TrendsPageAutomationIds.TrendsChart);
            _trendsChartLegend = GenerateMarkedQuery(TrendsPageAutomationIds.TrendsChartLegend);
            _trendsChartPrimaryAxis = GenerateMarkedQuery(TrendsPageAutomationIds.TrendsChartPrimaryAxis);
            _trendsChartSecondaryAxis = GenerateMarkedQuery(TrendsPageAutomationIds.TrendsChartSecondaryAxis);

            var viewsLegendIconsQuery = GenerateMarkedQuery(TrendsChartConstants.TotalViewsTitle);
            var uniqueViewsLegendIconsQuery = GenerateMarkedQuery(TrendsChartConstants.UniqueViewsTitle);
            var clonesLegendIconsQuery = GenerateMarkedQuery(TrendsChartConstants.TotalClonesTitle);
            var uniqueClonesLegendIconsQuery = GenerateMarkedQuery(TrendsChartConstants.UniqueClonesTitle);

            _viewsLegendIcon = x => x.Id(App.Query(viewsLegendIconsQuery).Last().Id);
            _uniqueViewsLegendIcon = x => x.Id(App.Query(uniqueViewsLegendIconsQuery).Last().Id);
            _clonesLegendIcon = x => x.Id(App.Query(clonesLegendIconsQuery).Last().Id);
            _uniqueClonesLegendIcon = x => x.Id(App.Query(uniqueClonesLegendIconsQuery).Last().Id);

            _viewsCard = GenerateMarkedQuery(TrendsPageAutomationIds.ViewsCard);
            _uniqueViewsCard = GenerateMarkedQuery(TrendsPageAutomationIds.UniqueViewsCard);
            _clonesCard = GenerateMarkedQuery(TrendsPageAutomationIds.ClonesCard);
            _uniqueClonesCard = GenerateMarkedQuery(TrendsPageAutomationIds.UniqueClonesCard);

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

        public void TapViewsCard()
        {
            App.Tap(_viewsCard);
            App.Screenshot("Views Card Tapped");
        }

        public void TapUniqueViewsCard()
        {
            App.Tap(_uniqueViewsCard);
            App.Screenshot("Unique Views Card Tapped");
        }

        public void TapClonesCard()
        {
            App.Tap(_clonesCard);
            App.Screenshot("Clones Card Tapped");
        }

        public void TapUniqueClonesCard()
        {
            App.Tap(_uniqueClonesCard);
            App.Screenshot("Unique Clones Card Tapped");
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