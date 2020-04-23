using System;
using Autofac;
using GitTrends.Mobile.Shared;
using GitTrends.Shared;
using GitTrends.Views.Base;
using Xamarin.Forms;
using Xamarin.Forms.Markup;
using static GitTrends.XamarinFormsService;
using static Xamarin.Forms.Markup.GridRowsColumns;

namespace GitTrends
{
    class TrendsPage : BaseContentPage<TrendsViewModel>
    {
        readonly Repository _repository;

        public TrendsPage(TrendsViewModel trendsViewModel,
                            Repository repository,
                            AnalyticsService analyticsService) : base(trendsViewModel, analyticsService, repository.Name)
        {
            _repository = repository;

            ViewModel.FetchDataCommand.Execute(repository);

            var referringSitesToolbarItem = new ToolbarItem
            {
                Text = "Referring Sites",
                IconImageSource = "ReferringSitesIcon",
                AutomationId = TrendsPageAutomationIds.ReferringSitesButton
            };
            referringSitesToolbarItem.Clicked += HandleReferringSitesToolbarItemClicked;
            ToolbarItems.Add(referringSitesToolbarItem);

            Content = new Grid
            {
                ColumnSpacing = 8,
                RowSpacing = 16,
                Padding = new Thickness(0, 16),

                RowDefinitions = Rows.Define(
                    (Row.Statistics, AbsoluteGridLength(StatisticsGrid.StatisticsViewHeight)),
                    (Row.Chart, StarGridLength(1))),

                Children =
                {
                    new StatisticsGrid().Row(Row.Statistics),
                    new TrendsChartActivityIndicator().Row(Row.Chart),
                    new TrendsChart().Row(Row.Chart),
                    new ListEmptyState("EmptyInsightsChart", 250, 250, "No insights yet.", true).Row(Row.Chart)
                }
            };
        }

        enum Row { Statistics, Chart }

        async void HandleReferringSitesToolbarItemClicked(object sender, EventArgs e)
        {
            AnalyticsService.Track("Referring Sites Button Tapped");

            using var scope = ContainerService.Container.BeginLifetimeScope();
            var referringSitesPage = scope.Resolve<ReferringSitesPage>(new TypedParameter(typeof(Repository), _repository));

            if (Device.RuntimePlatform is Device.iOS)
                await Navigation.PushModalAsync(referringSitesPage);
            else
                await Navigation.PushAsync(referringSitesPage);
        }

        class TrendsChartActivityIndicator : ActivityIndicator
        {
            public TrendsChartActivityIndicator()
            {
                AutomationId = TrendsPageAutomationIds.ActivityIndicator;
                HorizontalOptions = LayoutOptions.Center;
                VerticalOptions = LayoutOptions.Center;

                SetDynamicResource(ColorProperty, nameof(BaseTheme.PullToRefreshColor));

                this.SetBinding(IsVisibleProperty, nameof(TrendsViewModel.IsFetchingData));
                this.SetBinding(IsRunningProperty, nameof(TrendsViewModel.IsFetchingData));
            }
        }
    }
}
