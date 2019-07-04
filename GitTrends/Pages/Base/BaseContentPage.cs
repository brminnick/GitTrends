using System;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace GitTrends
{
    public abstract class BaseContentPage<T> : ContentPage where T : BaseViewModel, new()
    {
        protected BaseContentPage(string title)
        {
            BindingContext = ViewModel;
            BackgroundColor = ColorConstants.LightBlue;
            Title = title;
        }

        protected T ViewModel { get; } = new T();

        protected Task OpenBrowser(Uri uri) => OpenBrowser(uri.ToString());

        protected Task OpenBrowser(string url)
        {
            return XamarinFormsService.BeginInvokeOnMainThreadAsync(() =>
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
