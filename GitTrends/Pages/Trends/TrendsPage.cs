using CommunityToolkit.Maui.Markup;
using CommunityToolkit.Mvvm.Input;
using GitTrends.Common;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;

namespace GitTrends;

sealed class TrendsPage : BaseCarouselViewPage<TrendsViewModel>, IQueryAttributable
{
	Repository? _repository;

	public TrendsPage(
		IDeviceInfo deviceInfo,
		TrendsViewModel trendsViewModel,
		IAnalyticsService analyticsService) : base(trendsViewModel, analyticsService)
	{
		ToolbarItems.Add(new ToolbarItem
		{
			Text = PageTitles.ReferringSitesPage,
			IconImageSource = "ReferringSitesIcon",
			AutomationId = TrendsPageAutomationIds.ReferringSitesButton,
			Command = new AsyncRelayCommand(HandleReferringSitesToolbarItemClicked)
		});

		this.DynamicResource(BackgroundColorProperty, nameof(BaseTheme.PageBackgroundColor));

		Content.ItemsSource = Enumerable.Range(0, 2);
		Content.ItemTemplate = new TrendsCarouselDataTemplateSelector(deviceInfo, analyticsService, trendsViewModel);
	}

	async Task HandleReferringSitesToolbarItemClicked()
	{
		AnalyticsService.Track("Referring Sites Button Tapped");

		if (_repository is null)
			throw new InvalidOperationException($"{nameof(_repository)} cannot be null");

		var parameters = new Dictionary<string, object>
		{
			{
				ReferringSitesViewModel.RepositoryQueryString, _repository
			}
		};
		await Shell.Current.GoToAsync(AppShell.GetPageRoute<ReferringSitesPage>(), parameters);
	}

	void IQueryAttributable.ApplyQueryAttributes(IDictionary<string, object> query)
	{
		var repository = (Repository)query[TrendsViewModel.RepositoryQueryString];
		_repository = repository;

		Title = repository.Name;
	}
}