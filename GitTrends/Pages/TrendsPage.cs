using System;
using System.Collections.Generic;
using Autofac;
using GitTrends.Mobile.Shared;
using GitTrends.Shared;
using Syncfusion.SfChart.XForms;
using Xamarin.Forms;
using Xamarin.Forms.Markup;
using Xamarin.Forms.PancakeView;
using static GitTrends.XamarinFormsService;
using static Xamarin.Forms.Markup.GridRowsColumns;

namespace GitTrends
{
    class TrendsPage : BaseContentPage<TrendsViewModel>
    {
        static readonly Lazy<GitHubTrendsChart> _trendsChartHolder = new Lazy<GitHubTrendsChart>(new GitHubTrendsChart());

        readonly Repository _repository;

        public TrendsPage(TrendsViewModel trendsViewModel,
                            TrendsChartSettingsService trendsChartSettingsService,
                            Repository repository,
                            AnalyticsService analyticsService) : base(trendsViewModel, analyticsService, repository.Name)
        {
            _repository = repository;

            var referringSitesToolbarItem = new ToolbarItem
            {
                Text = "Referring Sites",
                IconImageSource = "ReferringSitesIcon",
                AutomationId = TrendsPageAutomationIds.ReferringSitesButton
            };
            referringSitesToolbarItem.Clicked += HandleReferringSitesToolbarItemClicked;
            ToolbarItems.Add(referringSitesToolbarItem);

            TrendsChart.TotalViewsSeries.IsVisible = trendsChartSettingsService.ShouldShowViewsByDefault;
            TrendsChart.TotalUniqueViewsSeries.IsVisible = trendsChartSettingsService.ShouldShowUniqueViewsByDefault;
            TrendsChart.TotalClonesSeries.IsVisible = trendsChartSettingsService.ShouldShowClonesByDefault;
            TrendsChart.TotalUniqueClonesSeries.IsVisible = trendsChartSettingsService.ShouldShowUniqueClonesByDefault;

            Content = new GridContainer();

            ViewModel.FetchDataCommand.Execute(_repository);
        }

        enum GridContainerRow { ViewsStats, ClonesStats, Chart }
        enum GridContainerColumn { Total, Unique }

        class GridContainer : Grid
        {
            public GridContainer()
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

                Children.Add(new CardView(CreateCardViewContent("Views", 5000000000, "total_views.svg", nameof(BaseTheme.CardViewsStatsIconColor)), TrendsChart.TotalViewsSeries)
                    .Row(GridContainerRow.ViewsStats).Column(GridContainerColumn.Total));
                Children.Add(new CardView(CreateCardViewContent("Unique Views", 32000, "unique_views.svg", nameof(BaseTheme.CardUniqueViewsStatsIconColor)), TrendsChart.TotalUniqueViewsSeries)
                    .Row(GridContainerRow.ViewsStats).Column(GridContainerColumn.Unique));
                Children.Add(new CardView(CreateCardViewContent("Clones", 200, "total_clones.svg", nameof(BaseTheme.CardClonesStatsIconColor)), TrendsChart.TotalClonesSeries)
                    .Row(GridContainerRow.ClonesStats).Column(GridContainerColumn.Total));
                Children.Add(new CardView(CreateCardViewContent("Unique Clones", 130, "unique_clones.svg", nameof(BaseTheme.CardUniqueClonesStatsIconColor)), TrendsChart.TotalUniqueClonesSeries)
                    .Row(GridContainerRow.ClonesStats).Column(GridContainerColumn.Unique));
                Children.Add(new TrendsChartActivityIndicator()
                    .Row(GridContainerRow.Chart).Column(GridContainerColumn.Total).ColumnSpan(2));
                Children.Add(TrendsChart
                    .Row(GridContainerRow.Chart).Column(GridContainerColumn.Total).ColumnSpan(2));

                SetDynamicResource(BackgroundColorProperty, nameof(BaseTheme.PageBackgroundColor));
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

            class CardView : PancakeView
            {
                public CardView(in IEnumerable<View> children, AreaSeries series)
                {
                    Padding = new Thickness(16, 12);
                    BorderThickness = 2;
                    CornerRadius = 4;
                    HasShadow = false;
                    Content = new ContentGrid(children);

                    var tapGestureRecognizer = new TapGestureRecognizer
                    {
                        Command = new Command(() => series.IsVisible = !series.IsVisible)
                    };

                    GestureRecognizers.Add(tapGestureRecognizer);

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

        static GitHubTrendsChart TrendsChart => _trendsChartHolder.Value;

        static IEnumerable<View> CreateCardViewContent(in string title, in long number, in string icon, in string baseIconThemeColor) => new View[]
        {
            new PrimaryColorLabel(14, title).Row(CardViewRow.StatsTitle).Column(CardViewColumn.Stats),
            new TrendsStatisticsLabel(34, number, nameof(BaseTheme.PrimaryTextColor)).Row(CardViewRow.StatsNumber).Column(CardViewColumn.Stats).ColumnSpan(2),
            new RepositoryStatSVGImage(icon, baseIconThemeColor, 32, 32).Row(CardViewRow.StatsTitle).Column(CardViewColumn.Icon).RowSpan(2)
        };

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

        class RepositoryStatSVGImage : SvgImage
        {
            public RepositoryStatSVGImage(in string svgFileName, string baseThemeColor, in double width, in double height)
                : base(svgFileName, () => (Color)Application.Current.Resources[baseThemeColor], width, height)
            {
                VerticalOptions = LayoutOptions.CenterAndExpand;
                HorizontalOptions = LayoutOptions.EndAndExpand;
            }
        }

        class TrendsStatisticsLabel : StatisticsLabel
        {
            public TrendsStatisticsLabel(in double fontSize, in long number, in string textColorThemeName) : base(fontSize, number, textColorThemeName, FontFamilyConstants.RobotoMedium)
            {
                VerticalTextAlignment = TextAlignment.Start;
                VerticalOptions = LayoutOptions.Start;
                Opacity = 0.87;
                Margin = new Thickness(0, 4, 0, 0);

                this.SetBinding(IsVisibleProperty, nameof(TrendsViewModel.AreStatisticsVisible));
            }
        }

        class PrimaryColorLabel : Label
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

        class GitHubTrendsChart : SfChart
        {
            public GitHubTrendsChart()
            {
                AutomationId = TrendsPageAutomationIds.TrendsChart;

                Margin = new Thickness(0, 24, -16, 0);

                TotalViewsSeries = new TrendsAreaSeries(TrendsChartConstants.TotalViewsTitle, nameof(DailyViewsModel.LocalDay), nameof(DailyViewsModel.TotalViews), nameof(BaseTheme.TotalViewsColor));
                TotalViewsSeries.SetBinding(ChartSeries.ItemsSourceProperty, nameof(TrendsViewModel.DailyViewsList));

                TotalUniqueViewsSeries = new TrendsAreaSeries(TrendsChartConstants.UniqueViewsTitle, nameof(DailyViewsModel.LocalDay), nameof(DailyViewsModel.TotalUniqueViews), nameof(BaseTheme.TotalUniqueViewsColor));
                TotalUniqueViewsSeries.SetBinding(ChartSeries.ItemsSourceProperty, nameof(TrendsViewModel.DailyViewsList));

                TotalClonesSeries = new TrendsAreaSeries(TrendsChartConstants.TotalClonesTitle, nameof(DailyClonesModel.LocalDay), nameof(DailyClonesModel.TotalClones), nameof(BaseTheme.TotalClonesColor));
                TotalClonesSeries.SetBinding(ChartSeries.ItemsSourceProperty, nameof(TrendsViewModel.DailyClonesList));

                TotalUniqueClonesSeries = new TrendsAreaSeries(TrendsChartConstants.UniqueClonesTitle, nameof(DailyClonesModel.LocalDay), nameof(DailyClonesModel.TotalUniqueClones), nameof(BaseTheme.TotalUniqueClonesColor));
                TotalUniqueClonesSeries.SetBinding(ChartSeries.ItemsSourceProperty, nameof(TrendsViewModel.DailyClonesList));

                this.SetBinding(IsVisibleProperty, nameof(TrendsViewModel.AreStatisticsVisible));

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
                    FontFamily = FontFamilyConstants.RobotoRegular
                };
                chartLegendLabelStyle.SetDynamicResource(ChartLegendLabelStyle.TextColorProperty, nameof(BaseTheme.ChartAxisTextColor));

                Legend = new ChartLegend
                {
                    AutomationId = TrendsPageAutomationIds.TrendsChartLegend,
                    DockPosition = LegendPlacement.Bottom,
                    ToggleSeriesVisibility = true,
                    Margin = new Thickness(0, 8, 0, 0),
                    IconWidth = 20,
                    IconHeight = 20,
                    LabelStyle = chartLegendLabelStyle
                };

                var axisLabelStyle = new ChartAxisLabelStyle
                {
                    FontSize = 14,
                    FontFamily = FontFamilyConstants.RobotoRegular
                };
                axisLabelStyle.SetDynamicResource(ChartAxisLabelStyle.TextColorProperty, nameof(BaseTheme.ChartAxisTextColor));

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
