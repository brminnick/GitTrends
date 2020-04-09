using GitTrends.Mobile.Shared;
using Xamarin.Forms;
using Xamarin.Forms.Markup;
using static GitTrends.XamarinFormsService;
using static Xamarin.Forms.Markup.GridRowsColumns;

namespace GitTrends
{
    class StatisticsGrid : Grid
    {
        public const int StatisticsViewHeight = _rowSpacing + _rowHeight * 2;

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

            Children.Add(new StatisticsCard("Views", "total_views.svg", nameof(BaseTheme.CardViewsStatsIconColor), nameof(TrendsViewModel.ViewsStatisticsText), nameof(TrendsViewModel.ViewsCardTappedCommand), TrendsPageAutomationIds.ViewsCard, TrendsPageAutomationIds.ViewsStatisticsLabel)
                .Row(Row.ViewsStats).Column(Column.Total));
            Children.Add(new StatisticsCard("Unique Views", "unique_views.svg", nameof(BaseTheme.CardUniqueViewsStatsIconColor), nameof(TrendsViewModel.UniqueViewsStatisticsText), nameof(TrendsViewModel.UniqueViewsCardTappedCommand), TrendsPageAutomationIds.UniqueViewsCard, TrendsPageAutomationIds.UniqueViewsStatisticsLabel)
                .Row(Row.ViewsStats).Column(Column.Unique)); ;
            Children.Add(new StatisticsCard("Clones", "total_clones.svg", nameof(BaseTheme.CardClonesStatsIconColor), nameof(TrendsViewModel.ClonesStatisticsText), nameof(TrendsViewModel.ClonesCardTappedCommand), TrendsPageAutomationIds.ClonesCard, TrendsPageAutomationIds.ClonesStatisticsLabel)
                .Row(Row.ClonesStats).Column(Column.Total));
            Children.Add(new StatisticsCard("Unique Clones", "unique_clones.svg", nameof(BaseTheme.CardUniqueClonesStatsIconColor), nameof(TrendsViewModel.UniqueClonesStatisticsText), nameof(TrendsViewModel.UniqueClonesCardTappedCommand), TrendsPageAutomationIds.UniqueClonesCard, TrendsPageAutomationIds.UniqueClonesStatisticsLabel)
                .Row(Row.ClonesStats).Column(Column.Unique));
        }

        enum Row { ViewsStats, ClonesStats, Chart }
        enum Column { Total, Unique }
    }
}
