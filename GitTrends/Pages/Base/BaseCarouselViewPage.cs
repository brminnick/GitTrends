using GitTrends.Shared;

namespace GitTrends;

public abstract class BaseCarouselViewPage<T> : BaseContentPage<T> where T : BaseViewModel
{
	protected BaseCarouselViewPage(T viewModel, IAnalyticsService analyticsService) : base(viewModel, analyticsService)
	{
		base.Content = new CarouselView();

		Content.ChildAdded += HandleChildAdded;
		Content.ChildRemoved += HandleChildRemoved;
	}

	public int PageCount => Children.Count;

	protected IList<View> Children => Content.VisibleViews;

	protected new CarouselView Content => (CarouselView)base.Content;

	protected override void OnNavigatedTo(NavigatedToEventArgs args)
	{
		base.OnNavigatedTo(args);

		AnalyticsService.Track($"{GetType().Name} Appeared");
	}

	protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
	{
		base.OnNavigatedFrom(args);

		AnalyticsService.Track($"{GetType().Name} Disappeared");
	}

	void HandleChildAdded(object? sender, ElementEventArgs e) => OnPropertyChanged(nameof(PageCount));
	void HandleChildRemoved(object? sender, ElementEventArgs e) => OnPropertyChanged(nameof(PageCount));
}