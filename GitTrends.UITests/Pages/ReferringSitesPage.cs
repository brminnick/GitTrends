using System;
using System.Linq;
using System.Threading.Tasks;
using GitTrends.Mobile.Shared;
using GitTrends.Shared;
using Xamarin.UITest;
using Xamarin.UITest.Android;
using Xamarin.UITest.iOS;
using Query = System.Func<Xamarin.UITest.Queries.AppQuery, Xamarin.UITest.Queries.AppQuery>;

namespace GitTrends.UITests
{
    class ReferringSitesPage : BaseCollectionPage<ReferringSiteModel>
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

            await WaitForNoPullToRefreshIndicator().ConfigureAwait(false);
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
    }
}