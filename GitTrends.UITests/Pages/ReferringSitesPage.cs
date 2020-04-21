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
        readonly Query _collectionView, _refreshView, _closeButton, _activityIndicator,
            _storeRatingRequestTitleLabel, _storeRatingRequestNoButton, _storeRatingRequestYesButton;

        public ReferringSitesPage(IApp app) : base(app, PageTitles.ReferringSitesPage)
        {
            _collectionView = GenerateMarkedQuery(ReferringSitesPageAutomationIds.CollectionView);
            _refreshView = GenerateMarkedQuery(ReferringSitesPageAutomationIds.RefreshView);
            _closeButton = GenerateMarkedQuery(ReferringSitesPageAutomationIds.CloseButton);
            _activityIndicator = GenerateMarkedQuery(ReferringSitesPageAutomationIds.ActivityIndicator);
            _storeRatingRequestTitleLabel = GenerateMarkedQuery(ReferringSitesPageAutomationIds.StoreRatingRequestTitleLabel);
            _storeRatingRequestNoButton = GenerateMarkedQuery(ReferringSitesPageAutomationIds.StoreRatingRequestNoButton);
            _storeRatingRequestYesButton = GenerateMarkedQuery(ReferringSitesPageAutomationIds.StoreRatingRequestYesButton);
        }

        public bool IsActivityIndicatorRunning => App.Query(_activityIndicator).Any();
        public string StoreRatingRequestTitleLabelText => GetText(_storeRatingRequestTitleLabel);

        public override async Task WaitForPageToLoad(TimeSpan? timeout = null)
        {
            await base.WaitForPageToLoad(timeout).ConfigureAwait(false);

            WaitForNoActivityIndicator();

            await WaitForNoPullToRefreshIndicator().ConfigureAwait(false);
        }

        public void TriggerReviewRequest()
        {
            App.InvokeBackdoorMethod(BackdoorMethodConstants.TriggerReviewRequest);
            App.Screenshot("Triggered Review Request");
        }

        public void TapStoreRatingRequestYesButton()
        {
            App.Tap(_storeRatingRequestYesButton);
            App.Screenshot("Yes Button Tapped");
        }

        public void TapStoreRatingRequestNoButton()
        {
            App.Tap(_storeRatingRequestNoButton);
            App.Screenshot("No Button Tapped");
        }

        public void DismissNoReferringSitesDialog()
        {
            App.Tap(ReferringSitesConstants.NoReferringSitesOK);
            App.Screenshot("No Referring Sites Dialog Dismissed");
        }

        public void WaitForTheNoReferringSitesDialog()
        {
            App.WaitForElement(ReferringSitesConstants.NoReferringSitesTitle);
            App.Screenshot("No Referring Sites Dialog Appeared");
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