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

        public void TriggerPullToRefresh() => App.InvokeBackdoorMethod(BackdoorMethodConstants.TriggerPullToRefresh);

        public async Task WaitForPullToRefreshIndicator(int timeoutInSeconds = 25)
        {
            int counter = 0;
            while (!IsRefreshViewRefreshIndicatorDisplayed)
            {
                await Task.Delay(1000).ConfigureAwait(false);

                if (counter++ >= timeoutInSeconds)
                    throw new Exception($"Loading the list took longer than {timeoutInSeconds} seconds");
            }
        }

        public async Task WaitForNoPullToRefreshIndicator(int timeoutInSeconds = 25)
        {
            int counter = 0;
            while (IsRefreshViewRefreshIndicatorDisplayed)
            {
                await Task.Delay(1000).ConfigureAwait(false);

                if (counter++ >= timeoutInSeconds)
                    throw new Exception($"Loading the list took longer than {timeoutInSeconds} seconds");
            }
        }
    }
}
