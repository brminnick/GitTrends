using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GitTrends.Mobile.Common;
using Xamarin.UITest;
using Xamarin.UITest.Android;

namespace GitTrends.UITests
{
    abstract class BaseCollectionPage<TCollection> : BasePage
    {
        protected BaseCollectionPage(IApp app, Func<string>? getPageTitle) : base(app, getPageTitle)
        {

        }

        public IReadOnlyList<TCollection> VisibleCollection => App.InvokeBackdoorMethod<List<TCollection>>(BackdoorMethodConstants.GetVisibleCollection);

        bool IsRefreshViewRefreshIndicatorDisplayed => App switch
        {
            AndroidApp androidApp => (bool)androidApp.Query(x => x.Class("RefreshViewRenderer").Invoke("isRefreshing")).First(),
            IApp iOSApp => iOSApp.Query(x => x.Class("UIRefreshControl")).Any(),
            _ => throw new NotSupportedException("Xamarin.UITest only supports Android and iOS"),
        };

        public override async Task WaitForPageToLoad(TimeSpan? timeout = null)
        {
            await base.WaitForPageToLoad(timeout).ConfigureAwait(false);

            try
            {
                await WaitForPullToRefreshIndicator(timeout).ConfigureAwait(false);
            }
            catch
            {

            }

            await WaitForNoPullToRefreshIndicator(timeout).ConfigureAwait(false);

            TryDismissErrorPopup();
        }

        public void TriggerPullToRefresh() => App.InvokeBackdoorMethod(BackdoorMethodConstants.TriggerPullToRefresh);

        public void TryDismissErrorPopup()
        {
            try
            {
                var dismissText = WaitForErrorPopup(TimeSpan.FromMilliseconds(250));
                App.Tap(dismissText);

                App.Screenshot("Error Popup Dismissed");
            }
            catch
            {

            }
        }

        public string WaitForErrorPopup(TimeSpan? timeout = null)
        {
            try
            {
                var expectedErrorEventArgs = new ErrorPullToRefreshEventArgs(string.Empty);
                App.WaitForElement(expectedErrorEventArgs.Title, timeout: timeout);

                App.Screenshot("Error Popup Appeared");

                return expectedErrorEventArgs.Cancel;
            }
            catch
            {
                try
                {
                    var expectedLoginExpiredEventArgs = new LoginExpiredPullToRefreshEventArgs();
                    App.WaitForElement(expectedLoginExpiredEventArgs.Title, timeout: timeout);

                    App.Screenshot("Login Expired Popup Appeared");

                    return expectedLoginExpiredEventArgs.Cancel;
                }
                catch
                {
                    var maximimApiRequestsReachedEventArgs = new MaximimApiRequestsReachedEventArgs(0);
                    App.WaitForElement(maximimApiRequestsReachedEventArgs.Title, timeout: timeout);

                    App.Screenshot("Maximum API Requests Popup Appeared");

                    return maximimApiRequestsReachedEventArgs.Cancel;
                }
            }
        }

        public async Task WaitForPullToRefreshIndicator(TimeSpan? timeSpan = null)
        {
            timeSpan ??= TimeSpan.FromSeconds(3);

            int counter = 0;
            while (!IsRefreshViewRefreshIndicatorDisplayed)
            {
                await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);

                if (counter++ >= timeSpan.Value.TotalSeconds)
                    throw new Exception($"Waiting for the Pull To Refresh Indicator took longer than {timeSpan.Value.TotalSeconds} seconds");
            }
        }

        public async Task WaitForNoPullToRefreshIndicator(TimeSpan? timeSpan = null)
        {
            timeSpan ??= TimeSpan.FromSeconds(60);

            int counter = 0;
            while (IsRefreshViewRefreshIndicatorDisplayed)
            {
                await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);

                if (counter++ >= timeSpan.Value.TotalSeconds)
                    throw new Exception($"Loading the list took longer than {timeSpan.Value.TotalSeconds} seconds");
            }
        }
    }
}
