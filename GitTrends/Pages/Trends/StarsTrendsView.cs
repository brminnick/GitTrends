﻿using CommunityToolkit.Maui.Markup;
using GitTrends.Mobile.Common;
using GitTrends.Resources;
using GitTrends.Shared;

namespace GitTrends;

class StarsTrendsView(IDeviceInfo deviceInfo, IAnalyticsService analyticsService) 
	: BaseTrendsDataTemplate(
		AppResources.GetResource<Color>(nameof(BaseTheme.CardStarsStatsIconColor)), 
		deviceInfo, 
		1, 
		() => new StarsStatisticsGrid(deviceInfo),
		analyticsService,
		() => new StarsChart(),
		CreateEmptyDataView,
		TrendsPageType.StarsTrendsPage) 
{
	
	static EmptyDataView CreateEmptyDataView() => new EmptyDataView(TrendsPageAutomationIds.StarsEmptyDataView)
		.Bind(EmptyDataView.IsVisibleProperty, 
			nameof(TrendsViewModel.IsStarsEmptyDataViewVisible),
			source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext, typeof(TrendsViewModel)))
		.Bind(EmptyDataView.TitleProperty, 
			nameof(TrendsViewModel.StarsEmptyDataViewTitleText),
			source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext, typeof(TrendsViewModel)))
		.Bind(EmptyDataView.ImageSourceProperty, 
			nameof(TrendsViewModel.StarsEmptyDataViewImage),
			source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext, typeof(TrendsViewModel)))
		.Bind(EmptyDataView.DescriptionProperty, 
			nameof(TrendsViewModel.StarsEmptyDataViewDescriptionText),
			source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext, typeof(TrendsViewModel)));
}