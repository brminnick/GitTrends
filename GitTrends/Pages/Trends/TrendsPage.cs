using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using GitTrends.Shared;
using CommunityToolkit.Maui.Markup;

namespace GitTrends;

sealed class TrendsPage : BaseCarouselViewPage<TrendsViewModel>, IDisposable
{
	readonly CancellationTokenSource _fetchDataCancellationTokenSource = new();

	readonly Repository _repository;
	readonly IDeviceInfo _deviceInfo;

	public TrendsPage(Repository repository,
		IDeviceInfo deviceInfo,
		StarsTrendsView starsTrendsView,
		TrendsViewModel trendsViewModel,
		IAnalyticsService analyticsService,
		ViewsClonesTrendsView viewsClonesTrendsView) : base(trendsViewModel, analyticsService)
	{
		_repository = repository;
		_deviceInfo = deviceInfo;

		Title = repository.Name;

		ToolbarItems.Add(new ToolbarItem
		{
			Text = PageTitles.ReferringSitesPage,
			IconImageSource = "ReferringSitesIcon",
			AutomationId = TrendsPageAutomationIds.ReferringSitesButton
		}.Invoke(referringSitesToolbarItem => referringSitesToolbarItem.Clicked += HandleReferringSitesToolbarItemClicked));

		Children.Add(viewsClonesTrendsView);
		Children.Add(starsTrendsView);

		this.DynamicResource(BackgroundColorProperty, nameof(BaseTheme.PageBackgroundColor));

		trendsViewModel.FetchDataCommand.Execute((repository, _fetchDataCancellationTokenSource.Token));
	}
	
	public void Dispose()
	{
		_fetchDataCancellationTokenSource.Dispose();
	}

	protected override void OnDisappearing()
	{
		_fetchDataCancellationTokenSource.Cancel();

		base.OnDisappearing();
	}

	async void HandleReferringSitesToolbarItemClicked(object? sender, EventArgs e)
	{
		AnalyticsService.Track("Referring Sites Button Tapped");

		var referringSitesPage = IPlatformApplication.Current?.Services.GetRequiredService<ReferringSitesPage>();

		if (_deviceInfo.Platform == DevicePlatform.iOS)
			await Navigation.PushModalAsync(referringSitesPage);
		else
			await Navigation.PushAsync(referringSitesPage);
	}
}