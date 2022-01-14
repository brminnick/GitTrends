using System;
using System.Threading;
using Autofac;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using GitTrends.Shared;
using Xamarin.CommunityToolkit.Markup;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;

namespace GitTrends
{
	class TrendsCarouselPage : BaseCarouselPage<TrendsViewModel>
	{
		readonly CancellationTokenSource _fetchDataCancellationTokenSource = new();

		readonly Repository _repository;

		public TrendsCarouselPage(Repository repository,
									IMainThread mainThread,
									StarsTrendsPage starsTrendsPage,
									TrendsViewModel trendsViewModel,
									IAnalyticsService analyticsService,
									ViewsClonesTrendsPage viewsClonesTrendsPage) : base(trendsViewModel, mainThread, analyticsService)
		{
			_repository = repository;

			Title = repository.Name;

			ToolbarItems.Add(new ToolbarItem
			{
				Text = PageTitles.ReferringSitesPage,
				IconImageSource = "ReferringSitesIcon",
				AutomationId = TrendsPageAutomationIds.ReferringSitesButton
			}.Invoke(referringSitesToolbarItem => referringSitesToolbarItem.Clicked += HandleReferringSitesToolbarItemClicked));

			Children.Add(viewsClonesTrendsPage);
			Children.Add(starsTrendsPage);

			this.DynamicResource(BackgroundColorProperty, nameof(BaseTheme.PageBackgroundColor));

			trendsViewModel.FetchDataCommand.Execute((repository, _fetchDataCancellationTokenSource.Token));
		}

		protected override void OnDisappearing()
		{
			_fetchDataCancellationTokenSource.Cancel();

			base.OnDisappearing();
		}

		async void HandleReferringSitesToolbarItemClicked(object sender, EventArgs e)
		{
			AnalyticsService.Track("Referring Sites Button Tapped");

			var referringSitesPage = ContainerService.Container.Resolve<ReferringSitesPage>(new TypedParameter(typeof(Repository), _repository));

			if (Device.RuntimePlatform is Device.iOS)
				await Navigation.PushModalAsync(referringSitesPage);
			else
				await Navigation.PushAsync(referringSitesPage);
		}
	}
}