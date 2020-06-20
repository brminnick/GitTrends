using GitTrends.Shared;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace GitTrends
{
    public abstract class BaseContentPage : ContentPage
    {

        protected BaseContentPage(in IAnalyticsService analyticsService,
                                    IMainThread mainThread,
                                    in string title = "",
                                    bool shouldUseSafeArea = false)
        {
            MainThread = mainThread;
            AnalyticsService = analyticsService;
            Title = title;

            SetDynamicResource(BackgroundColorProperty, nameof(BaseTheme.PageBackgroundColor));

            On<iOS>().SetUseSafeArea(shouldUseSafeArea);
            On<iOS>().SetModalPresentationStyle(UIModalPresentationStyle.FormSheet);
        }

        protected IAnalyticsService AnalyticsService { get; }
        protected IMainThread MainThread { get; }

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
        protected BaseContentPage(in T viewModel, IAnalyticsService analyticsService, IMainThread mainThread, in string title = "", bool shouldUseSafeArea = false)
            : base(analyticsService, mainThread, title, shouldUseSafeArea)
        {
            BindingContext = ViewModel = viewModel;
        }

        protected T ViewModel { get; }
    }
}
