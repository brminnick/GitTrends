using GitTrends.Shared;
using Xamarin.CommunityToolkit.Markup;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace GitTrends
{
	public abstract class BaseContentPage : ContentPage
	{

		protected BaseContentPage(in IAnalyticsService analyticsService,
									in IMainThread mainThread,
									in bool shouldUseSafeArea = false)
		{
			MainThread = mainThread;
			AnalyticsService = analyticsService;

			this.DynamicResource(BackgroundColorProperty, nameof(BaseTheme.PageBackgroundColor));

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
		protected BaseContentPage(in T viewModel, in IAnalyticsService analyticsService, in IMainThread mainThread, in bool shouldUseSafeArea = false)
			: base(analyticsService, mainThread, shouldUseSafeArea)
		{
			base.BindingContext = viewModel;
		}

		protected new T BindingContext => (T)base.BindingContext;
	}
}