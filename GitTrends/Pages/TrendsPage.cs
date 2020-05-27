using System;
using System.Threading;
using Autofac;
using GitTrends.Mobile.Shared;
using GitTrends.Shared;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;
using Xamarin.Forms.Markup;
using static GitTrends.XamarinFormsService;
using static Xamarin.Forms.Markup.GridRowsColumns;

namespace GitTrends
{
    class TrendsPage : BaseContentPage<TrendsViewModel>
    {
        readonly CancellationTokenSource _fetchDataCancellationTokenSource = new CancellationTokenSource();
        readonly Repository _repository;

        public TrendsPage(TrendsViewModel trendsViewModel,
                            Repository repository,
                            IAnalyticsService analyticsService,
                            IMainThread mainThread) : base(trendsViewModel, analyticsService, mainThread, repository.Name, true)
        {
            _repository = repository;

            ViewModel.FetchDataCommand.Execute((repository, _fetchDataCancellationTokenSource.Token));

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
                RowSpacing = 12,
                Padding = new Thickness(0, 16),

                RowDefinitions = Rows.Define(
                    (Row.Statistics, AbsoluteGridLength(StatisticsGrid.StatisticsGridHeight)),
                    (Row.Chart, StarGridLength(1))),

                Children =
                {
                    new StatisticsGrid()
                        .Row(Row.Statistics),
                    new TrendsChart()
                        .Row(Row.Chart),
                    new EmptyDataView("EmptyInsightsChart", TrendsPageAutomationIds.EmptyDataView)
                        .Row(Row.Chart)
                        .Bind(IsVisibleProperty, nameof(TrendsViewModel.IsEmptyDataViewVisible))
                        .Bind(EmptyDataView.TitleProperty, nameof(TrendsViewModel.EmptyDataViewTitle)),
                    new TrendsChartActivityIndicator()
                        .Row(Row.Chart),
                }
            };
        }

        enum Row { Statistics, Chart }

        protected override void OnDisappearing()
        {
            _fetchDataCancellationTokenSource.Cancel();

            base.OnDisappearing();
        }

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

                SetDynamicResource(ColorProperty, nameof(BaseTheme.ActivityIndicatorColor));

                this.SetBinding(IsVisibleProperty, nameof(TrendsViewModel.IsFetchingData));
                this.SetBinding(IsRunningProperty, nameof(TrendsViewModel.IsFetchingData));
            }
        }
    }
}
