using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using GitTrends.Shared;
using CommunityToolkit.Maui.Markup;

namespace GitTrends;

sealed class TrendsPage : BaseCarouselViewPage<TrendsViewModel>, IQueryAttributable
{
	readonly IDeviceInfo _deviceInfo;

	Repository? _repository;

	public TrendsPage(IDeviceInfo deviceInfo,
		StarsTrendsView starsTrendsView,
		TrendsViewModel trendsViewModel,
		IAnalyticsService analyticsService,
		ViewsClonesTrendsView viewsClonesTrendsView) : base(trendsViewModel, analyticsService)
	{
		_deviceInfo = deviceInfo;

		ToolbarItems.Add(new ToolbarItem
		{
			Text = PageTitles.ReferringSitesPage,
			IconImageSource = "ReferringSitesIcon",
			AutomationId = TrendsPageAutomationIds.ReferringSitesButton
		}.Invoke(referringSitesToolbarItem => referringSitesToolbarItem.Clicked += HandleReferringSitesToolbarItemClicked));

		this.DynamicResource(BackgroundColorProperty, nameof(BaseTheme.PageBackgroundColor));
	}

	async void HandleReferringSitesToolbarItemClicked(object? sender, EventArgs e)
	{
		AnalyticsService.Track("Referring Sites Button Tapped");

		var parameters = new Dictionary<string, object?>
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
	}
}