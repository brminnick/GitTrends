using System.ComponentModel;
using CommunityToolkit.Maui.Markup;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using GitTrends.Resources;
using Syncfusion.Maui.Charts;

namespace GitTrends;

class StarsChart(TrendsViewModel trendsViewModel) : BaseChartView(new StarsTrendsChart(trendsViewModel))
{
    sealed class StarsTrendsChart : BaseTrendsChart
    {
        //MinimumStarCount > MaximumDays > MaximumStarCount
        const int _maximumDays = 365;
        const int _minimumStarCount = 10;
        const int _maximumStarCount = 100;

        public StarsTrendsChart(TrendsViewModel trendsViewModel) : base(TrendsPageAutomationIds.StarsChart,
            new StarsChartPrimaryAxis(), new StarsChartSecondaryAxis(), trendsViewModel)
        {
            StarsSeries = new TrendsAreaSeries(TrendsChartTitleConstants.StarsTitle, nameof(DailyStarsModel.LocalDay),
                    nameof(DailyStarsModel.TotalStars), nameof(BaseTheme.CardStarsStatsIconColor))
                .Bind(ChartSeries.ItemsSourceProperty,
                    getter: static (TrendsViewModel vm) => vm.DailyStarsList);

            StarsSeries.PropertyChanged += HandleStarSeriesPropertyChanged;

            Series = [StarsSeries];

            this.Bind(IsVisibleProperty,
                getter: static (TrendsViewModel vm) => vm.IsStarsChartVisible);
        }

        public AreaSeries StarsSeries { get; }

        protected override void SetPaletteBrushColors()
        {
            PaletteBrushes = [AppResources.GetResource<Color>(nameof(BaseTheme.CardStarsStatsIconColor))];
        }

        async Task ZoomStarsChart(IReadOnlyList<DailyStarsModel> dailyStarsList)
        {
            if (dailyStarsList.Any())
            {
                var mostRecentDailyStarsModel = dailyStarsList[^1];

                var maximumDaysDateTime = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(_maximumDays));

                //Zoom to Maximum Stars
                if (dailyStarsList.Count >= _maximumStarCount)
                {
                    var maximumStarsDailyStarsModel = dailyStarsList[^_maximumStarCount];

                    await SetZoom(maximumStarsDailyStarsModel.LocalDay.ToOADate(),
                        mostRecentDailyStarsModel.LocalDay.ToOADate(),
                        maximumStarsDailyStarsModel.TotalStars,
                        mostRecentDailyStarsModel.TotalStars);
                }
                //Zoom to Maximum Days when Minimum Star Count has been met
                else if (dailyStarsList[0].Day <= maximumDaysDateTime)
                {
                    var nearestDailyStarsModel =
                        getNearestDailyStarsModelToTimeStamp(dailyStarsList, maximumDaysDateTime);

                    if (mostRecentDailyStarsModel.TotalStars - nearestDailyStarsModel.TotalStars > _minimumStarCount)
                    {
                        await SetZoom(maximumDaysDateTime.LocalDateTime.ToOADate(),
                            mostRecentDailyStarsModel.LocalDay.ToOADate(),
                            nearestDailyStarsModel.TotalStars,
                            mostRecentDailyStarsModel.TotalStars);
                    }
                }
            }

            //https://stackoverflow.com/a/1757221/5953643
            static DailyStarsModel getNearestDailyStarsModelToTimeStamp(
                in IReadOnlyList<DailyStarsModel> dailyStarsList, DateTimeOffset timeStamp)
            {
                var starsListOrderedByProximityToTimeStamp =
                    dailyStarsList.OrderBy(t => Math.Abs((t.Day - timeStamp).Ticks));

                foreach (var dailyStarsModel in starsListOrderedByProximityToTimeStamp)
                {
                    //Get the nearest DailyStarsModel before timeStamp
                    if (dailyStarsModel.Day < timeStamp)
                        return dailyStarsModel;
                }

                return starsListOrderedByProximityToTimeStamp.First();
            }
        }

        void HandleStarSeriesPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            ArgumentNullException.ThrowIfNull(sender);

            if (e.PropertyName == ChartSeries.ItemsSourceProperty.PropertyName)
            {
                var trendsAreaSeries = (TrendsAreaSeries)sender;
                var dailyStarsList = (IReadOnlyList<DailyStarsModel>)trendsAreaSeries.ItemsSource;

                if (dailyStarsList.Any())
                {
                    //Wait for SFChart to finish Rendering before Zooming
                    PropertyChanged += HandleSFChartPropertyChanged;
                }

                async void HandleSFChartPropertyChanged(object? sender, PropertyChangedEventArgs e)
                {
                    if (e.PropertyName is nameof(SeriesBounds))
                    {
                        PropertyChanged -= HandleSFChartPropertyChanged;

                        //Yield to the UI thread to allow the render to finish
                        await ZoomStarsChart(dailyStarsList).ConfigureAwait(ConfigureAwaitOptions.ForceYielding);
                    }
                }
            }
        }

        sealed class StarsChartPrimaryAxis : DateTimeAxis
        {
            public StarsChartPrimaryAxis()
            {
                LabelStyle = new ChartAxisLabelStyle
                {
                    FontSize = 9,
                    FontFamily = FontFamilyConstants.RobotoRegular,
                    Margin = new Thickness(2, 4, 2, 0)
                }.DynamicResource(ChartLabelStyle.TextColorProperty, nameof(BaseTheme.ChartAxisTextColor));

                AxisLineStyle = new AxisLineStyle();
                IntervalType = DateTimeIntervalType.Months;
                Interval = 1;
                RangePadding = DateTimeRangePadding.Round;
                MajorTickStyle = new ChartAxisTickStyle
                {
                    Stroke = Colors.Transparent
                };
                ShowMajorGridLines = false;


                this.Bind(MinimumProperty,
                        getter: static (TrendsViewModel vm) => vm.MinDailyStarsDate)
                    .Bind(MaximumProperty,
                        getter: static (TrendsViewModel vm) => vm.MaxDailyStarsDate);
            }
        }

        sealed class StarsChartSecondaryAxis : NumericalAxis
        {
            public StarsChartSecondaryAxis()
            {
                ShowMajorGridLines = false;

                AxisLineStyle = new AxisLineStyle();

                MajorTickStyle = new ChartAxisTickStyle().DynamicResource(ChartAxisTickStyle.StrokeProperty,
                    nameof(BaseTheme.ChartAxisLineColor));

                LabelStyle = new ChartAxisLabelStyle
                {
                    FontSize = 12,
                    FontFamily = FontFamilyConstants.RobotoRegular,
                }.DynamicResource(ChartLabelStyle.TextColorProperty, nameof(BaseTheme.ChartAxisTextColor));

                this.Bind(MinimumProperty,
                        getter: static (TrendsViewModel vm) => vm.MinDailyStarsValue)
                    .Bind(MaximumProperty,
                        getter: static (TrendsViewModel vm) => vm.MaxDailyStarsValue)
                    .Bind(IntervalProperty,
                        getter: static (TrendsViewModel vm) => vm.StarsChartYAxisInterval);
            }
        }

        sealed class AxisLineStyle : ChartLineStyle
        {
            public AxisLineStyle()
            {
                StrokeWidth = 1.51;
                this.DynamicResource(StrokeProperty, nameof(BaseTheme.ChartAxisLineColor));
            }
        }
    }
}