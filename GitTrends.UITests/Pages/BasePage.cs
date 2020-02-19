using System;
using System.Linq;
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

        protected bool IsRefreshViewRefreshIndicatorDisplayed => App switch
        {
            AndroidApp androidApp => (bool)androidApp.Query(x => x.Class("RefreshViewRenderer").Invoke("isRefreshing")).First(),
            iOSApp iOSApp => iOSApp.Query(x => x.Class("UIRefreshControl")).Any(),
            _ => throw new NotSupportedException("Xamarin.UITest only supports Android and iOS"),
        };

        protected static Query GenerateQuery(string automationId) => (x => x.Marked(automationId));

        protected void DismissSyncfusionLicensePopup()
        {
            try
            {
                App.WaitForElement(_syncfusionLicenseWarningTitle);
                App.Tap("Ok");
            }
            catch
            {
            }
        }

        public virtual void WaitForPageToLoad(TimeSpan? timeout = null)
        {
            if (!string.IsNullOrWhiteSpace(PageTitle))
                App.WaitForElement(x => x.Marked(PageTitle), timeout: timeout);
            else
                throw new InvalidOperationException($"{nameof(PageTitle)} cannot be empty");
        }

        protected void EnterText(in Query textEntryQuery, in string text, in bool shouldDismissKeyboard = true)
        {
            App.ClearText(textEntryQuery);
            App.EnterText(textEntryQuery, text);

            if (shouldDismissKeyboard)
                App.DismissKeyboard();
        }
    }
}

