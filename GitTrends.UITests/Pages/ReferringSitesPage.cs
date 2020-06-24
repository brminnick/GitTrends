using System;
using System.Linq;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using Xamarin.UITest;
using Xamarin.UITest.Android;
using Xamarin.UITest.iOS;
using Query = System.Func<Xamarin.UITest.Queries.AppQuery, Xamarin.UITest.Queries.AppQuery>;

namespace GitTrends.UITests
{
    class ReferringSitesPage : BaseCollectionPage<ReferringSiteModel>
    {
        readonly Query _collectionView, _refreshView, _closeButton,
            _storeRatingRequestTitleLabel, _storeRatingRequestNoButton, _storeRatingRequestYesButton,
            _emptyDataView;

        public ReferringSitesPage(IApp app) : base(app, () => PageTitles.ReferringSitesPage)
        {
            _collectionView = GenerateMarkedQuery(ReferringSitesPageAutomationIds.CollectionView);
            _refreshView = GenerateMarkedQuery(ReferringSitesPageAutomationIds.RefreshView);
            _closeButton = GenerateMarkedQuery(ReferringSitesPageAutomationIds.CloseButton);
            _storeRatingRequestTitleLabel = GenerateMarkedQuery(ReferringSitesPageAutomationIds.StoreRatingRequestTitleLabel);
            _storeRatingRequestNoButton = GenerateMarkedQuery(ReferringSitesPageAutomationIds.StoreRatingRequestNoButton);
            _storeRatingRequestYesButton = GenerateMarkedQuery(ReferringSitesPageAutomationIds.StoreRatingRequestYesButton);
            _emptyDataView = GenerateMarkedQuery(ReferringSitesPageAutomationIds.EmptyDataView);
        }

        public bool IsEmptyDataViewVisible => App.Query(_emptyDataView).Any();

        public string ExpectedAppStoreRequestTitle => App.InvokeBackdoorMethod<string>(BackdoorMethodConstants.GetReviewRequestAppStoreTitle);

        public string StoreRatingRequestTitleLabelText => GetText(_storeRatingRequestTitleLabel);

        public string StoreRatingRequestNoButtonText => GetText(_storeRatingRequestNoButton);

        public string StoreRatingRequestYesButtonText => GetText(_storeRatingRequestYesButton);

        public void WaitForEmptyDataView()
        {
            App.WaitForElement(_emptyDataView);
            App.Screenshot("Empty Data View Appeared");
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
    }
}