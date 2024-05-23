using GitTrends.Shared;

namespace GitTrends;

public class BaseCarouselPage<T> : CarouselPage where T : BaseViewModel
{
	public BaseCarouselPage(T viewModel, IAnalyticsService analyticsService)
	{
		BindingContext = viewModel;
		AnalyticsService = analyticsService;

		ChildAdded += HandleChildAdded;
		ChildRemoved += HandleChildRemoved;
	}

	public int PageCount => Children.Count;
		
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

	void HandleChildAdded(object? sender, ElementEventArgs e) => OnPropertyChanged(nameof(PageCount));
	void HandleChildRemoved(object? sender, ElementEventArgs e) => OnPropertyChanged(nameof(PageCount));
}