using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using Sharpnado.MaterialFrame;
using Xamarin.Forms;
using Xamarin.Forms.Markup;
using static GitTrends.MarkupExtensions;
using static Xamarin.Forms.Markup.GridRowsColumns;

namespace GitTrends
{
    class StatisticsGrid : Grid
    {
        public const int StatisticsGridHeight = _rowSpacing + _rowHeight * 2;

        const int _rowSpacing = 8;
        const int _columnSpacing = 8;
        const int _rowHeight = 96;

        public StatisticsGrid()
        {
            ColumnSpacing = _columnSpacing;
            RowSpacing = _rowSpacing;

            Padding = new Thickness(16, 0);

            RowDefinitions = Rows.Define(
                (Row.ViewsStats, AbsoluteGridLength(_rowHeight)),
                (Row.ClonesStats, AbsoluteGridLength(_rowHeight)));

            ColumnDefinitions = Columns.Define(
                (Column.Total, StarGridLength(1)),
                (Column.Unique, StarGridLength(1)));

            Children.Add(new StatisticsCard(SortingConstants.Views, "total_views.svg", nameof(BaseTheme.CardViewsStatsIconColor), TrendsPageAutomationIds.ViewsCard, TrendsPageAutomationIds.ViewsStatisticsLabel)
                .Row(Row.ViewsStats).Column(Column.Total)
                .Bind<StatisticsCard, bool, double>(MaterialFrame.ElevationProperty, nameof(TrendsViewModel.IsViewsSeriesVisible), convert: convertElevation)
                .Bind(StatisticsCard.IsSeriesVisibleProperty, nameof(TrendsViewModel.IsViewsSeriesVisible))
                .Bind(StatisticsCard.TextProperty, nameof(TrendsViewModel.ViewsStatisticsText))
                .BindTapGesture(nameof(TrendsViewModel.ViewsCardTappedCommand)));
            Children.Add(new StatisticsCard(SortingConstants.UniqueViews, "unique_views.svg", nameof(BaseTheme.CardUniqueViewsStatsIconColor), TrendsPageAutomationIds.UniqueViewsCard, TrendsPageAutomationIds.UniqueViewsStatisticsLabel)
                .Row(Row.ViewsStats).Column(Column.Unique)
                .Bind<StatisticsCard, bool, double>(MaterialFrame.ElevationProperty, nameof(TrendsViewModel.IsUniqueViewsSeriesVisible), convert: convertElevation)
                .Bind(StatisticsCard.IsSeriesVisibleProperty, nameof(TrendsViewModel.IsUniqueViewsSeriesVisible))
                .Bind(StatisticsCard.TextProperty, nameof(TrendsViewModel.UniqueViewsStatisticsText))
                .BindTapGesture(nameof(TrendsViewModel.UniqueViewsCardTappedCommand)));
            Children.Add(new StatisticsCard(SortingConstants.Clones, "total_clones.svg", nameof(BaseTheme.CardClonesStatsIconColor), TrendsPageAutomationIds.ClonesCard, TrendsPageAutomationIds.ClonesStatisticsLabel)
                .Row(Row.ClonesStats).Column(Column.Total)
                .Bind<StatisticsCard, bool, double>(MaterialFrame.ElevationProperty, nameof(TrendsViewModel.IsClonesSeriesVisible), convert: convertElevation)
                .Bind(StatisticsCard.IsSeriesVisibleProperty, nameof(TrendsViewModel.IsClonesSeriesVisible))
                .Bind(StatisticsCard.TextProperty, nameof(TrendsViewModel.ClonesStatisticsText))
                .BindTapGesture(nameof(TrendsViewModel.ClonesCardTappedCommand)));
            Children.Add(new StatisticsCard(SortingConstants.UniqueClones, "unique_clones.svg", nameof(BaseTheme.CardUniqueClonesStatsIconColor), TrendsPageAutomationIds.UniqueClonesCard, TrendsPageAutomationIds.UniqueClonesStatisticsLabel)
                .Row(Row.ClonesStats).Column(Column.Unique)
                .Bind<StatisticsCard, bool, double>(MaterialFrame.ElevationProperty, nameof(TrendsViewModel.IsUniqueClonesSeriesVisible), convert: convertElevation)
                .Bind(StatisticsCard.IsSeriesVisibleProperty, nameof(TrendsViewModel.IsUniqueClonesSeriesVisible))
                .Bind(StatisticsCard.TextProperty, nameof(TrendsViewModel.UniqueClonesStatisticsText))
                .BindTapGesture(nameof(TrendsViewModel.UniqueClonesCardTappedCommand)));

            static double convertElevation(bool isEnabled) => isEnabled ? 4 : 1;
        }

        enum Row { ViewsStats, ClonesStats, Chart }
        enum Column { Total, Unique }
    }
}
