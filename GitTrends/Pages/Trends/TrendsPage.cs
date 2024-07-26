using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using GitTrends.Shared;
using CommunityToolkit.Maui.Markup;

namespace GitTrends;

sealed class TrendsPage : BaseCarouselViewPage<TrendsViewModel>
{
	readonly IDeviceInfo _deviceInfo;

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

		await Shell.Current.GoToAsync(AppShell.GetPageRoute<ReferringSitesPage>());
	}
}