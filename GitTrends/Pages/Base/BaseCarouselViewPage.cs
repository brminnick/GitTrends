using System.ComponentModel;
using GitTrends.Common;

namespace GitTrends;

public abstract class BaseCarouselViewPage<T> : BaseContentPage<T> where T : BaseViewModel
{
	protected BaseCarouselViewPage(T viewModel, IAnalyticsService analyticsService) : base(viewModel, analyticsService)
	{
		base.Content = new BaseCarouselView(analyticsService);

		Content.PropertyChanged += HandleCarouselViewPropertyChanged;
	}

	public int PageCount { get; private set; }

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

	void HandleCarouselViewPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == ItemsView.ItemsSourceProperty.PropertyName)
		{
			ArgumentNullException.ThrowIfNull(sender);
			var carouselView = (CarouselView)sender;

			PageCount = ((IEnumerable<int>)carouselView.ItemsSource).Count();

			OnPropertyChanged(nameof(PageCount));
		}
	}
}