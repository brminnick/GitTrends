using System;
using Autofac;
using GitTrends.Mobile.Shared;
using GitTrends.Shared;
using Syncfusion.SfChart.XForms;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace GitTrends
{
    class TrendsPage : BaseContentPage<TrendsViewModel>
    {
        readonly Repository _repository;
        static readonly Lazy<GitHubTrendsChart> _trendsChartHolder = new Lazy<GitHubTrendsChart>(() => new GitHubTrendsChart());

        public TrendsPage(TrendsViewModel trendsViewModel,
                            TrendsChartSettingsService trendsChartSettingsService,
                            Repository repository,
                            AnalyticsService analyticsService) : base(repository.Name, trendsViewModel, analyticsService)
        {
            _repository = repository;

            var referringSitesToolbarItem = new ToolbarItem
            {
                Text = "Referring Sites",
                AutomationId = TrendsPageAutomationIds.ReferringSitesButton
            };
            referringSitesToolbarItem.Clicked += HandleReferringSitesToolbarItemClicked;
            ToolbarItems.Add(referringSitesToolbarItem);

            TrendsChart.TotalViewsSeries.IsVisible = trendsChartSettingsService.ShouldShowViewsByDefault;
            TrendsChart.TotalUniqueViewsSeries.IsVisible = trendsChartSettingsService.ShouldShowUniqueViewsByDefault;
            TrendsChart.TotalClonesSeries.IsVisible = trendsChartSettingsService.ShouldShowClonesByDefault;
            TrendsChart.TotalUniqueClonesSeries.IsVisible = trendsChartSettingsService.ShouldShowUniqueClonesByDefault;

            var activityIndicator = new ActivityIndicator
            {
                AutomationId = TrendsPageAutomationIds.ActivityIndicator
            };
            activityIndicator.SetDynamicResource(ActivityIndicator.ColorProperty, nameof(BaseTheme.RefreshControlColor));
            activityIndicator.SetBinding(IsVisibleProperty, nameof(TrendsViewModel.IsFetchingData));
            activityIndicator.SetBinding(ActivityIndicator.IsRunningProperty, nameof(TrendsViewModel.IsFetchingData));

            var absoluteLayout = new AbsoluteLayout();
            absoluteLayout.Children.Add(activityIndicator, new Rectangle(.5, .5, -1, -1), AbsoluteLayoutFlags.PositionProportional);
            absoluteLayout.Children.Add(TrendsChart, new Rectangle(0, 0, 1, 1), AbsoluteLayoutFlags.All);

            Content = absoluteLayout;

            ViewModel.FetchDataCommand.Execute((_repository.OwnerLogin, _repository.Name));
        }

        static GitHubTrendsChart TrendsChart => _trendsChartHolder.Value;

        protected override void HandlePageSizeChanged(object sender, EventArgs e)
        {
            Padding = GetPadding();

            base.HandlePageSizeChanged(sender, e);
        }

        Thickness GetPadding()
        {
            if (DeviceDisplay.MainDisplayInfo.Orientation is DisplayOrientation.Landscape)
                return new Thickness(0, 5, 0, 0);
            else
                return Device.RuntimePlatform is Device.iOS ? new Thickness(0, 5, 0, 15) : new Thickness(0, 5, 0, 0);
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

        class GitHubTrendsChart : SfChart
        {
            public GitHubTrendsChart()
            {
                AutomationId = TrendsPageAutomationIds.TrendsChart;

                TotalViewsSeries = new TrendsAreaSeries("Views", nameof(DailyViewsModel.LocalDay), nameof(DailyViewsModel.TotalViews), nameof(BaseTheme.TotalViewsColor), TrendsPageAutomationIds.TotalViewsSeries);
                TotalViewsSeries.SetBinding(ChartSeries.ItemsSourceProperty, nameof(TrendsViewModel.DailyViewsList));

                TotalUniqueViewsSeries = new TrendsAreaSeries("Unique Views", nameof(DailyViewsModel.LocalDay), nameof(DailyViewsModel.TotalUniqueViews), nameof(BaseTheme.TotalUniqueViewsColor), TrendsPageAutomationIds.TotalUniqueViewsSeries);
                TotalUniqueViewsSeries.SetBinding(ChartSeries.ItemsSourceProperty, nameof(TrendsViewModel.DailyViewsList));

                TotalClonesSeries = new TrendsAreaSeries("Clones", nameof(DailyClonesModel.LocalDay), nameof(DailyClonesModel.TotalClones), nameof(BaseTheme.TotalClonesColor), TrendsPageAutomationIds.TotalClonesSeries);
                TotalClonesSeries.SetBinding(ChartSeries.ItemsSourceProperty, nameof(TrendsViewModel.DailyClonesList));

                TotalUniqueClonesSeries = new TrendsAreaSeries("Unique Clones", nameof(DailyClonesModel.LocalDay), nameof(DailyClonesModel.TotalUniqueClones), nameof(BaseTheme.TotalUniqueClonesColor), TrendsPageAutomationIds.TotalUniqueClonesSeries);
                TotalUniqueClonesSeries.SetBinding(ChartSeries.ItemsSourceProperty, nameof(TrendsViewModel.DailyClonesList));

                this.SetBinding(IsVisibleProperty, nameof(TrendsViewModel.IsChartVisible));

                ChartBehaviors = new ChartBehaviorCollection
                {
                    new ChartZoomPanBehavior(),
                    new ChartTrackballBehavior()
                };

                Series = new ChartSeriesCollection
                {
                    TotalViewsSeries,
                    TotalUniqueViewsSeries,
                    TotalClonesSeries,
                    TotalUniqueClonesSeries
                };

                var chartLegendLabelStyle = new ChartLegendLabelStyle();
                chartLegendLabelStyle.SetDynamicResource(ChartLegendLabelStyle.TextColorProperty, nameof(BaseTheme.ChartAxisTextColor));

                Legend = new ChartLegend
                {
                    AutomationId = TrendsPageAutomationIds.TrendsChartLegend,
                    DockPosition = LegendPlacement.Bottom,
                    ToggleSeriesVisibility = true,
                    IconWidth = 20,
                    IconHeight = 20,
                    LabelStyle = chartLegendLabelStyle
                };

                var axisLabelStyle = new ChartAxisLabelStyle
                {
                    FontSize = 14
                };
                axisLabelStyle.SetDynamicResource(ChartAxisLabelStyle.TextColorProperty, nameof(BaseTheme.ChartAxisTextColor));

                var axisLineStyle = new ChartLineStyle();
                axisLineStyle.SetDynamicResource(ChartLineStyle.StrokeColorProperty, nameof(BaseTheme.ChartAxisLineColor));

                PrimaryAxis = new DateTimeAxis
                {
                    AutomationId = TrendsPageAutomationIds.TrendsChartPrimaryAxis,
                    IntervalType = DateTimeIntervalType.Days,
                    Interval = 1,
                    RangePadding = DateTimeRangePadding.Round,
                    LabelStyle = axisLabelStyle,
                    AxisLineStyle = axisLineStyle,
                    MajorTickStyle = new ChartAxisTickStyle { StrokeColor = Color.Transparent },
                    ShowMajorGridLines = false
                };
                PrimaryAxis.SetBinding(DateTimeAxis.MinimumProperty, nameof(TrendsViewModel.MinDateValue));
                PrimaryAxis.SetBinding(DateTimeAxis.MaximumProperty, nameof(TrendsViewModel.MaxDateValue));

                var secondaryAxisMajorTickStyle = new ChartAxisTickStyle();
                secondaryAxisMajorTickStyle.SetDynamicResource(ChartAxisTickStyle.StrokeColorProperty, nameof(BaseTheme.ChartAxisLineColor));

                SecondaryAxis = new NumericalAxis
                {
                    AutomationId = TrendsPageAutomationIds.TrendsChartSecondaryAxis,
                    LabelStyle = axisLabelStyle,
                    AxisLineStyle = axisLineStyle,
                    MajorTickStyle = secondaryAxisMajorTickStyle,
                    ShowMajorGridLines = false
                };
                SecondaryAxis.SetBinding(NumericalAxis.MinimumProperty, nameof(TrendsViewModel.DailyViewsClonesMinValue));
                SecondaryAxis.SetBinding(NumericalAxis.MaximumProperty, nameof(TrendsViewModel.DailyViewsClonesMaxValue));

                BackgroundColor = Color.Transparent;

                ChartPadding = new Thickness(0, 5, 0, 0);
            }

            public AreaSeries TotalViewsSeries { get; }
            public AreaSeries TotalUniqueViewsSeries { get; }
            public AreaSeries TotalClonesSeries { get; }
            public AreaSeries TotalUniqueClonesSeries { get; }

            class TrendsAreaSeries : AreaSeries
            {
                public TrendsAreaSeries(in string title, in string xDataTitle, in string yDataTitle, in string colorResource, in string automationId)
                {
                    AutomationId = automationId;
                    Opacity = 0.9;
                    Label = title;
                    XBindingPath = xDataTitle;
                    YBindingPath = yDataTitle;
                    LegendIcon = ChartLegendIcon.SeriesType;

                    SetDynamicResource(ColorProperty, colorResource);
                }
            }
        }
    }
}
