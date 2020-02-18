using System;
using System.Linq;
using GitTrends.Mobile.Shared;
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
            _collectionView = GenerateQuery(ReferringSitesPageAutomationIds.CollectionView);
            _refreshView = GenerateQuery(ReferringSitesPageAutomationIds.RefreshView);
            _closeButton = GenerateQuery(ReferringSitesPageAutomationIds.CloseButton);
            _activityIndicator = GenerateQuery(ReferringSitesPageAutomationIds.ActivityIndicator);
        }

        public bool IsActivityIndicatorRunning => App.Query(_activityIndicator).Any();

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

        public void WaitForActivityIndicator() => App.WaitForElement(_activityIndicator);
        public void WaitForNoActivityIndicator() => App.WaitForNoElement(_activityIndicator);
    }
}