using GitTrends.Shared;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;

namespace GitTrends
{
    public class BaseCarouselPage<T> : CarouselPage where T : BaseViewModel
    {
        public BaseCarouselPage(T viewModel, IMainThread mainThread, IAnalyticsService analyticsService)
        {
            MainThread = mainThread;
            BindingContext = viewModel;
            AnalyticsService = analyticsService;

            ChildAdded += HandleChildAdded;
            ChildRemoved += HandleChildRemoved;
        }

        public int PageCount => Children.Count;

        protected IMainThread MainThread { get; }
        protected IAnalyticsService AnalyticsService { get; }

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

        void HandleChildAdded(object sender, ElementEventArgs e) => OnPropertyChanged(nameof(PageCount));
        void HandleChildRemoved(object sender, ElementEventArgs e) => OnPropertyChanged(nameof(PageCount));
    }
}
