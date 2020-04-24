using System;
using System.Linq;
using System.Threading.Tasks;
using GitTrends.Mobile.Shared;
using Newtonsoft.Json;
using Xamarin.UITest;
using Xamarin.UITest.Queries;
using Query = System.Func<Xamarin.UITest.Queries.AppQuery, Xamarin.UITest.Queries.AppQuery>;

namespace GitTrends.UITests
{
    class TrendsPage : BasePage
    {
        readonly Query _trendsChart, _trendsChartLegend, _trendsChartPrimaryAxis, _trendsChartSecondaryAxis,
            _activityIndicator, _androidContextMenuOverflowButton, _referringSiteButton, _viewsCard,
            _uniqueViewsCard, _clonesCard, _uniqueClonesCard, _viewsStatisticsLabel, _uniqueViewsStatisticsLabel,
            _clonesStatisticsLabel, _uniqueClonesStatisticsLabel, _emptyDataView;

        public TrendsPage(IApp app) : base(app)
        {
            _trendsChart = GenerateMarkedQuery(TrendsPageAutomationIds.TrendsChart);
            _trendsChartLegend = GenerateMarkedQuery(TrendsPageAutomationIds.TrendsChartLegend);
            _trendsChartPrimaryAxis = GenerateMarkedQuery(TrendsPageAutomationIds.TrendsChartPrimaryAxis);
            _trendsChartSecondaryAxis = GenerateMarkedQuery(TrendsPageAutomationIds.TrendsChartSecondaryAxis);

            _viewsCard = GenerateMarkedQuery(TrendsPageAutomationIds.ViewsCard);
            _uniqueViewsCard = GenerateMarkedQuery(TrendsPageAutomationIds.UniqueViewsCard);
            _clonesCard = GenerateMarkedQuery(TrendsPageAutomationIds.ClonesCard);
            _uniqueClonesCard = GenerateMarkedQuery(TrendsPageAutomationIds.UniqueClonesCard);

            _activityIndicator = GenerateMarkedQuery(TrendsPageAutomationIds.ActivityIndicator);

            _androidContextMenuOverflowButton = x => x.Class("androidx.appcompat.widget.ActionMenuPresenter$OverflowMenuButton");
            _referringSiteButton = GenerateMarkedQuery(TrendsPageAutomationIds.ReferringSitesButton);

            _viewsStatisticsLabel = GenerateMarkedQuery(TrendsPageAutomationIds.ViewsStatisticsLabel);
            _uniqueViewsStatisticsLabel = GenerateMarkedQuery(TrendsPageAutomationIds.UniqueViewsStatisticsLabel);
            _clonesStatisticsLabel = GenerateMarkedQuery(TrendsPageAutomationIds.ClonesStatisticsLabel);
            _uniqueClonesStatisticsLabel = GenerateMarkedQuery(TrendsPageAutomationIds.UniqueClonesStatisticsLabel);

            _emptyDataView = GenerateMarkedQuery(TrendsPageAutomationIds.EmptyDataView);
        }

        public string ViewsStatisticsLabelText => GetText(_viewsStatisticsLabel);
        public string UniqueViewsStatisticsLabelText => GetText(_uniqueViewsStatisticsLabel);
        public string ClonesStatisticsLabelText => GetText(_clonesStatisticsLabel);
        public string UniqueClonesStatisticsLabelText => GetText(_uniqueClonesStatisticsLabel);

        public bool IsEmptyDataViewVisible => App.Query(_emptyDataView).Any();

        AppResult ViewsLegendIcon => App.Query(GenerateMarkedQuery(TrendsChartConstants.TotalViewsTitle)).Last();
        AppResult UniqueViewsLegendIcon => App.Query(GenerateMarkedQuery(TrendsChartConstants.UniqueViewsTitle)).Last();
        AppResult ClonesLegendIcon => App.Query(GenerateMarkedQuery(TrendsChartConstants.TotalClonesTitle)).Last();
        AppResult UniqueClonesLegendIcon => App.Query(GenerateMarkedQuery(TrendsChartConstants.UniqueClonesTitle)).Last();

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

        public void WaitForEmptyDataView()
        {
            App.WaitForElement(_emptyDataView);
            App.Screenshot("Empty Data View Appeared");
        }

        public bool IsSeriesVisible(string seriesName)
        {
            var serializedIsSeriesVisible = App.InvokeBackdoorMethod(BackdoorMethodConstants.IsTrendsSeriesVisible, seriesName).ToString();
            return JsonConvert.DeserializeObject<bool>(serializedIsSeriesVisible);
        }

        public void TapBackButton()
        {
            App.Back();
            App.Screenshot("Back Button Tapped");
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
            var rect = ViewsLegendIcon.Rect;

            App.TapCoordinates(rect.CenterX, rect.CenterY);
            App.Screenshot("Views Legend Icon Tapped");
        }

        public void TapUniqueViewsLegendIcon()
        {
            var rect = UniqueViewsLegendIcon.Rect;

            App.TapCoordinates(rect.CenterX, rect.CenterY);
            App.Screenshot("Unique Views Legend Icon Tapped");
        }

        public void TapClonesLegendIcon()
        {
            var rect = ClonesLegendIcon.Rect;

            App.TapCoordinates(rect.CenterX, rect.CenterY);
            App.Screenshot("Clones Legend Icon Tapped");
        }

        public void TapUniqueClonesLegendIcon()
        {
            var rect = UniqueClonesLegendIcon.Rect;

            App.TapCoordinates(rect.CenterX, rect.CenterY);
            App.Screenshot("Unique Clones Legend Icon Tapped");
        }

        public void TapReferringSitesButton()
        {
            App.Tap(_referringSiteButton);
            App.Screenshot("Referring Sites Button Tapped");
        }
    }
}