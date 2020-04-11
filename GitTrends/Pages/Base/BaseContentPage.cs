using System;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace GitTrends
{
    public abstract class BaseContentPage<T> : ContentPage where T : BaseViewModel
    {
        readonly bool _shouldUseSafeArea;

        protected BaseContentPage(in string title, in T viewModel, AnalyticsService analyticsService, bool shouldUseSafeArea = true)
        {
            _shouldUseSafeArea = shouldUseSafeArea;

            SetDynamicResource(BackgroundColorProperty, nameof(BaseTheme.PageBackgroundColor));
            BindingContext = ViewModel = viewModel;
            Title = title;

            AnalyticsService = analyticsService;

            On<iOS>().SetModalPresentationStyle(UIModalPresentationStyle.FormSheet);
            SetSafeArea();

            SizeChanged += HandlePageSizeChanged;
        }

        protected T ViewModel { get; }
        protected AnalyticsService AnalyticsService { get; }

        protected static double GetWidth(in RelativeLayout parent, in View view) => view.Measure(parent.Width, parent.Height).Request.Width;
        protected static double GetHeight(in RelativeLayout parent, in View view) => view.Measure(parent.Width, parent.Height).Request.Height;

        protected override void OnAppearing()
        {
            base.OnAppearing();

            AnalyticsService.Track($"{GetType().Name} Appeared");
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            AnalyticsService.Track($"{GetType().Name} Disappeared");
        }

        protected Task OpenBrowser(Uri uri) => OpenBrowser(uri.ToString());

        protected Task OpenBrowser(string url)
        {
            return MainThread.InvokeOnMainThreadAsync(() =>
            {
                var browserOptions = new BrowserLaunchOptions
                {
                    PreferredToolbarColor = (Color)Xamarin.Forms.Application.Current.Resources[nameof(BaseTheme.NavigationBarBackgroundColor)],
                    PreferredControlColor = (Color)Xamarin.Forms.Application.Current.Resources[nameof(BaseTheme.NavigationBarTextColor)],
                };

                return Browser.OpenAsync(url, browserOptions);
            });
        }

        protected virtual void HandlePageSizeChanged(object sender, EventArgs e)
        {
            AnalyticsService.Track("Page Size Changed",
                                    "Is Portrait",
                                    (DeviceDisplay.MainDisplayInfo.Height > DeviceDisplay.MainDisplayInfo.Width).ToString());

            SetSafeArea();
            ForceLayout();
        }

        void SetSafeArea()
        {
            var isLandscape = DeviceDisplay.MainDisplayInfo.Orientation is DisplayOrientation.Landscape;
            On<iOS>().SetUseSafeArea(isLandscape && _shouldUseSafeArea);
        }
    }
}
