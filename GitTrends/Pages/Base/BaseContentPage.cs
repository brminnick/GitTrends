using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace GitTrends
{
    public abstract class BaseContentPage : ContentPage
    {
        protected BaseContentPage(in AnalyticsService analyticsService, in string title = "", bool shouldUseSafeArea = false)
        {
            AnalyticsService = analyticsService;
            Title = title;

            SetDynamicResource(BackgroundColorProperty, nameof(BaseTheme.PageBackgroundColor));

            On<iOS>().SetUseSafeArea(shouldUseSafeArea);
            On<iOS>().SetModalPresentationStyle(UIModalPresentationStyle.FormSheet);
        }

        protected AnalyticsService AnalyticsService { get; }

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

    public abstract class BaseContentPage<T> : BaseContentPage where T : BaseViewModel
    {
        protected BaseContentPage(in T viewModel, AnalyticsService analyticsService, in string title = "", bool shouldUseSafeArea = false)
            : base(analyticsService, title, shouldUseSafeArea)
        {
            BindingContext = ViewModel = viewModel;
        }

        protected T ViewModel { get; }
    }
}
