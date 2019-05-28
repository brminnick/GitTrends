using System;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace GitTrends
{
    abstract class BaseContentPage<T> : ContentPage where T : BaseViewModel, new()
    {
        protected BaseContentPage() => BindingContext = ViewModel;

        protected T ViewModel { get; } = new T();

        protected Task OpenBrowser(Uri uri) => OpenBrowser(uri.ToString());

        protected Task OpenBrowser(string url)
        {
            var browserOptions = new BrowserLaunchOptions
            {
                PreferredToolbarColor = ColorConstants.LightBlue,
                PreferredControlColor = ColorConstants.DarkBlue
            };

            return Browser.OpenAsync(url, browserOptions);
        }
    }
}
