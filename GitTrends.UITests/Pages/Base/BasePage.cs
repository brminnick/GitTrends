using System;
using System.Linq;
using System.Threading.Tasks;
using GitTrends.Mobile.Common;
using Xamarin.UITest;
using Xamarin.UITest.iOS;
using Query = System.Func<Xamarin.UITest.Queries.AppQuery, Xamarin.UITest.Queries.AppQuery>;

namespace GitTrends.UITests
{
    abstract class BasePage
    {
        readonly Query _iOSSafariView, _iOSEmailView;
        readonly Func<string>? _getPageTitle;
        const string _syncfusionLicenseWarningTitle = "Syncfusion License";

        protected BasePage(IApp app, Func<string>? getPageTitle = null)
        {
            App = app;
            _getPageTitle = getPageTitle;

            _iOSSafariView = x => x.Class("SFSafariView");
            _iOSEmailView = GenerateMarkedQuery("RemoteViewBridge");
        }

        public bool AreNotificationsEnabled => App.InvokeBackdoorMethod<bool>(BackdoorMethodConstants.AreNotificationsEnabled);

        public bool IsEmailOpen => App switch
        {
            iOSApp iOSApp => iOSApp.Query(_iOSEmailView).Any(),
            _ => throw new NotSupportedException("Browser Can Only Be Verified on iOS")
        };

        public bool IsBrowserOpen => App switch
        {
            iOSApp iOSApp => iOSApp.Query(_iOSSafariView).Any(),
            _ => throw new NotSupportedException("Browser Can Only Be Verified on iOS")
        };

        public string PageTitle => _getPageTitle?.Invoke() ?? string.Empty;
        protected IApp App { get; }

        public void WaitForEmailToOpen()
        {
            if (App is iOSApp iOSApp)
                iOSApp.WaitForElement(_iOSEmailView);
            else
                throw new NotSupportedException("Email Can Only Be Verified on iOS");

            App.Screenshot("Browser Opened");
        }

        public void WaitForBrowserToOpen()
        {
            if (App is iOSApp iOSApp)
                iOSApp.WaitForElement(_iOSSafariView);
            else
                throw new NotSupportedException("Browser Can Only Be Verified on iOS");

            App.Screenshot("Browser Opened");
        }

        public void DismissSyncfusionLicensePopup()
        {
            try
            {
                App.WaitForElement(_syncfusionLicenseWarningTitle, timeout: TimeSpan.FromSeconds(1));
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

        protected void ScrollTo(in Query query)
        {
            try
            {
                App.ScrollDownTo(query);
            }
            catch
            {
                App.ScrollUpTo(query);
            }
        }

        protected string GetText(in Query query)
        {
            ScrollTo(query);
            return App.Query(query).First().Text ?? App.Query(query).First().Label;
        }
    }
}

