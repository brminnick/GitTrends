using Xamarin.UITest;

using Query = System.Func<Xamarin.UITest.Queries.AppQuery, Xamarin.UITest.Queries.AppQuery>;

namespace GitTrends.UITests
{
    abstract class BasePage
    {
        protected BasePage(IApp app, string pageTitle)
        {
            App = app;
            PageTitle = pageTitle;
        }

        public string PageTitle { get; }
        protected IApp App { get; }

        public virtual void WaitForPageToLoad() => App.WaitForElement(x => x.Marked(PageTitle));

        protected void EnterText(in Query textEntryQuery, in string text, in bool shouldDismissKeyboard = true)
        {
            App.ClearText(textEntryQuery);
            App.EnterText(textEntryQuery, text);

            if (shouldDismissKeyboard)
                App.DismissKeyboard();
        }
    }
}

