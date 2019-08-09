using System;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace GitTrends
{
    public abstract class BaseContentPage<T> : ContentPage where T : BaseViewModel
    {
        protected BaseContentPage(string title, T viewModel)
        {
            BindingContext = ViewModel = viewModel;
            BackgroundColor = ColorConstants.LightBlue;
            Title = title;
        }

        protected T ViewModel { get; }

        protected Task OpenBrowser(Uri uri) => OpenBrowser(uri.ToString());

        protected Task OpenBrowser(string url)
        {
            return Device.InvokeOnMainThreadAsync(() =>
            {
                var browserOptions = new BrowserLaunchOptions
                {
                    PreferredToolbarColor = ColorConstants.LightBlue,
                    PreferredControlColor = ColorConstants.DarkBlue
                };

                return Browser.OpenAsync(url, browserOptions);
            });
        }
    }
}
