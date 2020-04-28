using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GitTrends.Mobile.Shared;
using Xamarin.UITest;
using Xamarin.UITest.Android;

namespace GitTrends.UITests
{
    abstract class BaseCollectionPage<TCollection> : BasePage
    {
        protected BaseCollectionPage(IApp app, string pageTitle = "") : base(app, pageTitle)
        {

        }

        public List<TCollection> VisibleCollection => App.InvokeBackdoorMethod<List<TCollection>>(BackdoorMethodConstants.GetVisibleCollection);

        bool IsRefreshViewRefreshIndicatorDisplayed => App switch
        {
            AndroidApp androidApp => (bool)androidApp.Query(x => x.Class("RefreshViewRenderer").Invoke("isRefreshing")).First(),
            IApp iOSApp => iOSApp.Query(x => x.Class("UIRefreshControl")).Any(),
            _ => throw new NotSupportedException("Xamarin.UITest only supports Android and iOS"),
        };

        public override async Task WaitForPageToLoad(TimeSpan? timeout = null)
        {
            await base.WaitForPageToLoad(timeout).ConfigureAwait(false);

            await WaitForPullToRefreshIndicator(timeout).ConfigureAwait(false);
            await WaitForNoPullToRefreshIndicator(timeout).ConfigureAwait(false);

            TryDismissErrorPopup();
        }

        public void TriggerPullToRefresh() => App.InvokeBackdoorMethod(BackdoorMethodConstants.TriggerPullToRefresh);

        public void TryDismissErrorPopup()
        {
            try
            {
                var dismissText = WaitForErrorPopup(TimeSpan.FromSeconds(1));
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
                App.WaitForElement(expectedErrorEventArgs.ErrorTitle, timeout: timeout);

                App.Screenshot("Error Popup Appeared");

                return expectedErrorEventArgs.DismissText;
            }
            catch
            {
                var expectedLoginExpiredEventArgs = new LoginExpiredPullToRefreshEventArgs();
                App.WaitForElement(expectedLoginExpiredEventArgs.ErrorTitle, timeout: timeout);

                App.Screenshot("Login Expired Popup Appeared");

                return expectedLoginExpiredEventArgs.DismissText;
            }
        }

        public async Task WaitForPullToRefreshIndicator(TimeSpan? timeSpan = null)
        {
            timeSpan ??= TimeSpan.FromSeconds(25);

            int counter = 0;
            while (!IsRefreshViewRefreshIndicatorDisplayed)
            {
                await Task.Delay(1000).ConfigureAwait(false);

                if (counter++ >= timeSpan.Value.Seconds)
                    throw new Exception($"Loading the list took longer than {timeSpan.Value.Seconds} seconds");
            }
        }

        public async Task WaitForNoPullToRefreshIndicator(TimeSpan? timeSpan = null)
        {
            timeSpan ??= TimeSpan.FromSeconds(25);

            int counter = 0;
            while (IsRefreshViewRefreshIndicatorDisplayed)
            {
                await Task.Delay(1000).ConfigureAwait(false);

                if (counter++ >= timeSpan.Value.Seconds)
                    throw new Exception($"Loading the list took longer than {timeSpan.Value.Seconds} seconds");
            }
        }
    }
}
