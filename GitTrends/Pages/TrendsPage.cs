using System;
using Autofac;
using GitTrends.Mobile.Shared;
using GitTrends.Shared;
using Syncfusion.SfChart.XForms;
using Xamarin.Essentials;
using System.Collections.Generic;
using FFImageLoading.Svg.Forms;
using Xamarin.Forms;
using Xamarin.Forms.Markup;
using Xamarin.Forms.PancakeView;
using static GitTrends.XamarinFormsService;
using static Xamarin.Forms.Markup.GridRowsColumns;
using GitTrends.Views.Base;

namespace GitTrends
{
    class TrendsPage : BaseContentPage<TrendsViewModel>
    {
        static readonly Lazy<GitHubTrendsChart> _trendsChartHolder = new Lazy<GitHubTrendsChart>(() => new GitHubTrendsChart()
        .Row(GridContainerRow.Chart).Column(GridContainerColumn.Total).ColumnSpan(2));

        readonly Repository _repository;

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

            TrendsChart.Margin(new Thickness(0, 24, -16, 24));
            TrendsChart.ChartPadding = new Thickness(0,24,0,0);
            TrendsChart.TotalViewsSeries.IsVisible = trendsChartSettingsService.ShouldShowViewsByDefault;
            TrendsChart.TotalUniqueViewsSeries.IsVisible = trendsChartSettingsService.ShouldShowUniqueViewsByDefault;
            TrendsChart.TotalClonesSeries.IsVisible = trendsChartSettingsService.ShouldShowClonesByDefault;
            TrendsChart.TotalUniqueClonesSeries.IsVisible = trendsChartSettingsService.ShouldShowUniqueClonesByDefault;
            

            var activityIndicator = new ActivityIndicator
            {
                AutomationId = TrendsPageAutomationIds.ActivityIndicator
            }.Row(GridContainerRow.ViewsStats).Column(GridContainerColumn.Total).RowSpan(3).ColumnSpan(2).Center();

            activityIndicator.SetDynamicResource(ActivityIndicator.ColorProperty, nameof(BaseTheme.RefreshControlColor));
            activityIndicator.SetBinding(IsVisibleProperty, nameof(TrendsViewModel.IsFetchingData));
            activityIndicator.SetBinding(ActivityIndicator.IsRunningProperty, nameof(TrendsViewModel.IsFetchingData));

            Content = new GridContainer(TrendsChart, activityIndicator);

            ViewModel.FetchDataCommand.Execute(_repository);
        }

        enum GridContainerRow { ViewsStats, ClonesStats, Chart }
        enum GridContainerColumn { Total, Unique }

        class GridContainer : Grid
        {
            public GridContainer(in View chart, in View activityIndicator)
            {
                ColumnSpacing = 8;
                RowSpacing = 8;
                Padding = new Thickness(16);

                RowDefinitions = Rows.Define(
                    (GridContainerRow.ViewsStats, AbsoluteGridLength(96)),
                    (GridContainerRow.ClonesStats, AbsoluteGridLength(96)),
                    (GridContainerRow.Chart, StarGridLength(1)));

                ColumnDefinitions = Columns.Define(
                    (GridContainerColumn.Total, StarGridLength(1)),
                    (GridContainerColumn.Unique, StarGridLength(1)));

                Children.Add(new CardView(CreateViews("Views", 5000000000, "total_views.svg", nameof(BaseTheme.CardViewsStatsIconColor)))
                    .Row(GridContainerRow.ViewsStats).Column(GridContainerColumn.Total));
                Children.Add(new CardView(CreateViews("Unique Views", 32000, "unique_views.svg", nameof(BaseTheme.CardUniqueViewsStatsIconColor)))
                    .Row(GridContainerRow.ViewsStats).Column(GridContainerColumn.Unique));
                Children.Add(new CardView(CreateViews("Clones", 200, "total_clones.svg", nameof(BaseTheme.CardClonesStatsIconColor)))
                    .Row(GridContainerRow.ClonesStats).Column(GridContainerColumn.Total));
                Children.Add(new CardView(CreateViews("Unique Clones", 130, "unique_clones.svg", nameof(BaseTheme.CardUniqueClonesStatsIconColor)))
                    .Row(GridContainerRow.ClonesStats).Column(GridContainerColumn.Unique));

                Children.Add(chart);
                Children.Add(activityIndicator);

                SetDynamicResource(BackgroundColorProperty, nameof(BaseTheme.PageBackgroundColor));
            }

            class CardView : PancakeView
            {
                public CardView(in IEnumerable<View> children)
                {
                    Padding = new Thickness(16, 12);
                    BorderThickness = 2;
                    CornerRadius = 4;
                    HasShadow = false;
                    Visual = VisualMarker.Material;
                    Content = new ContentGrid(children);

                    SetDynamicResource(BorderColorProperty, nameof(BaseTheme.CardBorderColor));
                    SetDynamicResource(BackgroundColorProperty, nameof(BaseTheme.CardSurfaceColor));
                }

                class ContentGrid : Grid
                {
                    public ContentGrid(in IEnumerable<View> children)
                    {
                        HorizontalOptions = LayoutOptions.FillAndExpand;
                        VerticalOptions = LayoutOptions.FillAndExpand;
                        ColumnSpacing = 0;
                        RowSpacing = 0;

                        RowDefinitions = Rows.Define(
                            (CardViewRow.StatsTitle, Auto),
                            (CardViewRow.StatsNumber, StarGridLength(1)));

                        ColumnDefinitions = Columns.Define(
                            (CardViewColumn.Stats, StarGridLength(1)),
                            (CardViewColumn.Icon, AbsoluteGridLength(32)));

                        foreach (var child in children)
                        {
                            Children.Add(child);
                        }
                    }
                }
            }
        }

        enum CardViewRow { StatsTitle, StatsNumber }
        enum CardViewColumn { Stats, Icon }

        static IEnumerable<View> CreateViews(in string title, in long number, in string icon, in string baseIconThemeColor)
        {
            return new View[]
            {
               new PrimaryColorLabel(14, title).Row(CardViewRow.StatsTitle).Column(CardViewColumn.Stats),
               new StatsLabel(34, number, nameof(BaseTheme.PrimaryTextColor), nameof(BaseTheme.RobotoMedium)).Row(CardViewRow.StatsNumber).Column(CardViewColumn.Stats).ColumnSpan(2),
               new RepositoryStatSVGImage(icon, baseIconThemeColor, 32, 32)
               {
                   VerticalOptions = LayoutOptions.StartAndExpand,
               }.Row(CardViewRow.StatsTitle).Column(CardViewColumn.Icon).RowSpan(2)
            };
        }

        public class StatsLabel : StatisticsLabel
        {
            public StatsLabel(in double fontSize, in long number, in string textColorThemeName, in string fontFamilyName) : base(fontSize, number, textColorThemeName, fontFamilyName)
            {
                HorizontalTextAlignment = TextAlignment.Start;
                VerticalTextAlignment = TextAlignment.Start;
                VerticalOptions = LayoutOptions.Start;
                Opacity = 0.87;
                Margin = new Thickness(0,4,0,0);
            }
        }

        public class PrimaryColorLabel : Label
        {
            public PrimaryColorLabel(in double fontSize, in string text)
            {
                FontSize = fontSize;
                Text = text;
                Opacity = 0.6;
                LineBreakMode = LineBreakMode.TailTruncation;
                HorizontalTextAlignment = TextAlignment.Start;
                VerticalOptions = LayoutOptions.Start;

                SetDynamicResource(TextColorProperty, nameof(BaseTheme.PrimaryTextColor));
            }
        }

        static GitHubTrendsChart TrendsChart => _trendsChartHolder.Value;

        protected override void HandlePageSizeChanged(object sender, EventArgs e)
        {
            Padding = GetPadding();

            base.HandlePageSizeChanged(sender, e);
        }

        Thickness GetPadding()
        {
            //Check if Device is in Landscape
            if (DeviceDisplay.MainDisplayInfo.Width > DeviceDisplay.MainDisplayInfo.Height)
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

                TotalViewsSeries = new TrendsAreaSeries(TrendsChartConstants.TotalViewsTitle, nameof(DailyViewsModel.LocalDay), nameof(DailyViewsModel.TotalViews), nameof(BaseTheme.TotalViewsColor));
                TotalViewsSeries.SetBinding(ChartSeries.ItemsSourceProperty, nameof(TrendsViewModel.DailyViewsList));

                TotalUniqueViewsSeries = new TrendsAreaSeries(TrendsChartConstants.UniqueViewsTitle, nameof(DailyViewsModel.LocalDay), nameof(DailyViewsModel.TotalUniqueViews), nameof(BaseTheme.TotalUniqueViewsColor));
                TotalUniqueViewsSeries.SetBinding(ChartSeries.ItemsSourceProperty, nameof(TrendsViewModel.DailyViewsList));

                TotalClonesSeries = new TrendsAreaSeries(TrendsChartConstants.TotalClonesTitle, nameof(DailyClonesModel.LocalDay), nameof(DailyClonesModel.TotalClones), nameof(BaseTheme.TotalClonesColor));
                TotalClonesSeries.SetBinding(ChartSeries.ItemsSourceProperty, nameof(TrendsViewModel.DailyClonesList));

                TotalUniqueClonesSeries = new TrendsAreaSeries(TrendsChartConstants.UniqueClonesTitle, nameof(DailyClonesModel.LocalDay), nameof(DailyClonesModel.TotalUniqueClones), nameof(BaseTheme.TotalUniqueClonesColor));
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

                var chartLegendLabelStyle = new ChartLegendLabelStyle()
                {
                    FontSize = 12,
                };
                chartLegendLabelStyle.SetDynamicResource(ChartLegendLabelStyle.TextColorProperty, nameof(BaseTheme.ChartAxisTextColor));
                chartLegendLabelStyle.SetDynamicResource(ChartLegendLabelStyle.FontFamilyProperty, nameof(BaseTheme.RobotoRegular));

                Legend = new ChartLegend
                {
                    AutomationId = TrendsPageAutomationIds.TrendsChartLegend,
                    DockPosition = LegendPlacement.Bottom,
                    ToggleSeriesVisibility = true,
                    Margin = new Thickness(0,8,0,0),
                    IconWidth = 20,
                    IconHeight = 20,
                    LabelStyle = chartLegendLabelStyle
                };

                var axisLabelStyle = new ChartAxisLabelStyle
                {
                    FontSize = 14,
                };
                axisLabelStyle.SetDynamicResource(ChartAxisLabelStyle.TextColorProperty, nameof(BaseTheme.ChartAxisTextColor));
                axisLabelStyle.SetDynamicResource(ChartAxisLabelStyle.FontFamilyProperty, nameof(BaseTheme.RobotoRegular));

                var axisLineStyle = new ChartLineStyle()
                {
                    StrokeWidth = 1.51
                };
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
                public TrendsAreaSeries(in string title, in string xDataTitle, in string yDataTitle, in string colorResource)
                {
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
