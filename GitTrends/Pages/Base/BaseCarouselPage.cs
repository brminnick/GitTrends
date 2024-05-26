using GitTrends.Shared;

namespace GitTrends;

public class BaseCarouselPage<T> : ContentPage where T : BaseViewModel
{
	public BaseCarouselPage(T viewModel, IAnalyticsService analyticsService)
	{
		BindingContext = viewModel;
		AnalyticsService = analyticsService;

		ChildAdded += HandleChildAdded;
		ChildRemoved += HandleChildRemoved;

		base.Content = new Grid();
	}

	public int PageCount => Children.Count;

	protected IView CurrentPage
	{
		get => throw new NotImplementedException();
		set => throw new NotImplementedException();
	}

	protected IList<IView> Children => Content.Children;
		
	protected IAnalyticsService AnalyticsService { get; }

	new Grid Content => (Grid)base.Content;

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