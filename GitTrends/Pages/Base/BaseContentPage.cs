using CommunityToolkit.Maui.Markup;
using GitTrends.Common;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace GitTrends;

public abstract class BaseContentPage : ContentPage
{
	protected BaseContentPage(in IAnalyticsService analyticsService,
								in bool shouldUseSafeArea = false)
	{
		AnalyticsService = analyticsService;

		this.DynamicResource(BackgroundColorProperty, nameof(BaseTheme.PageBackgroundColor));

		On<iOS>().SetUseSafeArea(shouldUseSafeArea);
		On<iOS>().SetModalPresentationStyle(UIModalPresentationStyle.FormSheet);
	}

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
}

public abstract class BaseContentPage<T> : BaseContentPage where T : BaseViewModel
{
	protected BaseContentPage(in T viewModel, in IAnalyticsService analyticsService, in bool shouldUseSafeArea = false)
		: base(analyticsService, shouldUseSafeArea)
	{
		base.BindingContext = viewModel;
	}

	protected new T BindingContext => (T)base.BindingContext;
}