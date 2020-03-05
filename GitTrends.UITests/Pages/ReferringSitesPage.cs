using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GitTrends.Mobile.Shared;
using GitTrends.Shared;
using Newtonsoft.Json;
using Xamarin.UITest;
using Xamarin.UITest.Android;
using Xamarin.UITest.iOS;
using Query = System.Func<Xamarin.UITest.Queries.AppQuery, Xamarin.UITest.Queries.AppQuery>;

namespace GitTrends.UITests
{
    class ReferringSitesPage : BasePage
    {
        readonly Query _collectionView, _refreshView, _closeButton, _activityIndicator;

        public ReferringSitesPage(IApp app) : base(app, PageTitles.ReferringSitesPage)
        {
            _collectionView = GenerateMarkedQuery(ReferringSitesPageAutomationIds.CollectionView);
            _refreshView = GenerateMarkedQuery(ReferringSitesPageAutomationIds.RefreshView);
            _closeButton = GenerateMarkedQuery(ReferringSitesPageAutomationIds.CloseButton);
            _activityIndicator = GenerateMarkedQuery(ReferringSitesPageAutomationIds.ActivityIndicator);
        }

        public bool IsActivityIndicatorRunning => App.Query(_activityIndicator).Any();

        public override async Task WaitForPageToLoad(TimeSpan? timeout = null)
        {
            await base.WaitForPageToLoad(timeout).ConfigureAwait(false);

            WaitForNoActivityIndicator();

            await WaitForNoPullToRefresh().ConfigureAwait(false);
        }

        public void TriggerPullToRefresh() => App.InvokeBackdoorMethod(BackdoorMethodConstants.TriggerPullToRefresh);

        public async Task WaitForNoPullToRefresh(int timeoutInSeconds = 25)
        {
            int counter = 0;
            while (IsRefreshViewRefreshIndicatorDisplayed && counter < timeoutInSeconds)
            {
                await Task.Delay(1000).ConfigureAwait(false);
                counter++;

                if (counter >= timeoutInSeconds)
                    throw new Exception($"Loading the list took longer than {timeoutInSeconds}");
            }
        }

        public void ClosePage()
        {
            switch (App)
            {
                case iOSApp iOSApp:
                    iOSApp.Tap(_closeButton);
                    break;
                case AndroidApp androidApp:
                    androidApp.Back();
                    break;
                default:
                    throw new NotSupportedException();
            }
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

        public List<ReferringSiteModel> GetVisibleReferringSitesList()
        {
            var serializedRepositoryList = App.InvokeBackdoorMethod(BackdoorMethodConstants.GetVisibleCollection).ToString();
            return JsonConvert.DeserializeObject<List<ReferringSiteModel>>(serializedRepositoryList);
        }

        public void TriggerrPullToRefresh() =>
            App.InvokeBackdoorMethod(BackdoorMethodConstants.TriggerPullToRefresh);
    }
}