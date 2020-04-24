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
            _storeRatingRequestTitleLabel, _storeRatingRequestNoButton, _storeRatingRequestYesButton,
            _emptyDataView;

        public ReferringSitesPage(IApp app) : base(app, PageTitles.ReferringSitesPage)
        {
            _collectionView = GenerateMarkedQuery(ReferringSitesPageAutomationIds.CollectionView);
            _refreshView = GenerateMarkedQuery(ReferringSitesPageAutomationIds.RefreshView);
            _closeButton = GenerateMarkedQuery(ReferringSitesPageAutomationIds.CloseButton);
            _activityIndicator = GenerateMarkedQuery(ReferringSitesPageAutomationIds.ActivityIndicator);
            _storeRatingRequestTitleLabel = GenerateMarkedQuery(ReferringSitesPageAutomationIds.StoreRatingRequestTitleLabel);
            _storeRatingRequestNoButton = GenerateMarkedQuery(ReferringSitesPageAutomationIds.StoreRatingRequestNoButton);
            _storeRatingRequestYesButton = GenerateMarkedQuery(ReferringSitesPageAutomationIds.StoreRatingRequestYesButton);
            _emptyDataView = GenerateMarkedQuery(ReferringSitesPageAutomationIds.EmptyDataView);
        }

        public bool IsEmptyDataViewVisible => App.Query(_emptyDataView).Any();

        public bool IsActivityIndicatorRunning => App.Query(_activityIndicator).Any();

        public string ExpectedAppStoreRequestTitle => App.InvokeBackdoorMethod<string>(BackdoorMethodConstants.GetReviewRequestAppStoreTitle);

        public string StoreRatingRequestTitleLabelText => GetText(_storeRatingRequestTitleLabel);

        public string StoreRatingRequestNoButtonText => GetText(_storeRatingRequestNoButton);

        public string StoreRatingRequestYesButtonText => GetText(_storeRatingRequestYesButton);

        public void WaitForEmptyDataView()
        {
            App.WaitForElement(_emptyDataView);
            App.Screenshot("Empty Data View Appeared");
        }

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

        public void WaitForReviewRequest()
        {
            App.WaitForElement(_storeRatingRequestTitleLabel);
            App.Screenshot("Review Request Appeared");
        }

        public void WaitForNoReviewRequest()
        {
            App.WaitForNoElement(_storeRatingRequestTitleLabel);
            App.Screenshot("Review Request Disappeared");
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