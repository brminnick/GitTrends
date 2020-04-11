using System;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.UITest;
using Xamarin.UITest.Android;
using Xamarin.UITest.iOS;
using Query = System.Func<Xamarin.UITest.Queries.AppQuery, Xamarin.UITest.Queries.AppQuery>;

namespace GitTrends.UITests
{
    abstract class BasePage
    {
        const string _syncfusionLicenseWarningTitle = "Syncfusion License";

        protected BasePage(IApp app, string pageTitle = "")
        {
            App = app;
            PageTitle = pageTitle;
        }

        public string PageTitle { get; }
        protected IApp App { get; }

        bool IsRefreshViewRefreshIndicatorDisplayed => App switch
        {
            AndroidApp androidApp => (bool)androidApp.Query(x => x.Class("RefreshViewRenderer").Invoke("isRefreshing")).First(),
            iOSApp iOSApp => iOSApp.Query(x => x.Class("UIRefreshControl")).Any(),
            _ => throw new NotSupportedException("Xamarin.UITest only supports Android and iOS"),
        };

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

        public void DismissSyncfusionLicensePopup()
        {
            try
            {
                App.WaitForElement(_syncfusionLicenseWarningTitle);
                App.Tap("Ok");

                App.Screenshot("Syncfusion License Popup Dismissed");
            }
            catch
            {
            }
        }

        public virtual Task WaitForPageToLoad(TimeSpan? timeout = null)
        {
            if (!string.IsNullOrWhiteSpace(PageTitle))
                App.WaitForElement(x => x.Marked(PageTitle), timeout: timeout);
            else
                throw new InvalidOperationException($"{nameof(PageTitle)} cannot be empty");

            return Task.CompletedTask;
        }

        protected static Query GenerateMarkedQuery(string automationId) => (x => x.Marked(automationId));

        protected void EnterText(in Query textEntryQuery, in string text, in bool shouldDismissKeyboard = true)
        {
            App.ClearText(textEntryQuery);
            App.EnterText(textEntryQuery, text);

            if (shouldDismissKeyboard)
                App.DismissKeyboard();
        }
    }
}

