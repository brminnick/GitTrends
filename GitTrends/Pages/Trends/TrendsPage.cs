﻿using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using GitTrends.Shared;
using CommunityToolkit.Maui.Markup;

namespace GitTrends;

sealed class TrendsPage : BaseCarouselViewPage<TrendsViewModel>, IQueryAttributable
{
	Repository? _repository;

	public TrendsPage(
		TrendsViewModel trendsViewModel,
		StarsTrendsView starsTrendsView,
		IAnalyticsService analyticsService,
		ViewsClonesTrendsView viewsClonesTrendsView) : base(trendsViewModel, analyticsService)
	{
		ToolbarItems.Add(new ToolbarItem
		{
			Text = PageTitles.ReferringSitesPage,
			IconImageSource = "ReferringSitesIcon",
			AutomationId = TrendsPageAutomationIds.ReferringSitesButton
		}.Invoke(referringSitesToolbarItem => referringSitesToolbarItem.Clicked += HandleReferringSitesToolbarItemClicked));

		this.DynamicResource(BackgroundColorProperty, nameof(BaseTheme.PageBackgroundColor));
		
		Content.ItemsSource = Enumerable.Range(0, 1);
		Content.ItemTemplate = new TrendsCarouselDataTemplateSelector(starsTrendsView, viewsClonesTrendsView);
	}

	async void HandleReferringSitesToolbarItemClicked(object? sender, EventArgs e)
	{
		AnalyticsService.Track("Referring Sites Button Tapped");

		if (_repository is null)
			throw new InvalidOperationException($"{nameof(_repository)} cannot be null");

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