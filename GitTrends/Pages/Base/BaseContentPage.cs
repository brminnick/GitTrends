using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace GitTrends
{
    public abstract class BaseContentPage<T> : ContentPage where T : BaseViewModel
    {
        protected BaseContentPage(in T viewModel, AnalyticsService analyticsService, in string title = "", bool shouldUseSafeArea = true)
        {
            SetDynamicResource(BackgroundColorProperty, nameof(BaseTheme.PageBackgroundColor));
            BindingContext = ViewModel = viewModel;
            Title = title;

            AnalyticsService = analyticsService;

            On<iOS>().SetUseSafeArea(shouldUseSafeArea);
            On<iOS>().SetModalPresentationStyle(UIModalPresentationStyle.FormSheet);
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
    }
}
