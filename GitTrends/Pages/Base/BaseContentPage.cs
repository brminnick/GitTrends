using System;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace GitTrends
{
    public abstract class BaseContentPage<T> : ContentPage where T : BaseViewModel
    {
        protected BaseContentPage(in string title, in T viewModel)
        {
            SetDynamicResource(BackgroundColorProperty, nameof(BaseTheme.PageBackgroundColor));
            BindingContext = ViewModel = viewModel;
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
                    PreferredToolbarColor = (Color)Application.Current.Resources[nameof(BaseTheme.PageBackgroundColor)],
                    PreferredControlColor = (Color)Application.Current.Resources[nameof(BaseTheme.TextColor)],
                };

                return Browser.OpenAsync(url, browserOptions);
            });
        }
    }
}
