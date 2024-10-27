using CommunityToolkit.Maui.Markup;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using Sharpnado.MaterialFrame;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace GitTrends;

class ViewsClonesStatisticsGrid : Grid
{
	public const int StatisticsGridHeight = _rowSpacing + _rowHeight * 2;

	const int _rowSpacing = 8;
	const int _columnSpacing = 8;
	const int _rowHeight = 96;

	public ViewsClonesStatisticsGrid(IDeviceInfo deviceInfo)
	{
		ColumnSpacing = _columnSpacing;
		RowSpacing = _rowSpacing;

		Padding = new Thickness(16, 0);

		RowDefinitions = Rows.Define(
			(Row.ViewsStats, _rowHeight),
			(Row.ClonesStats, _rowHeight));

		ColumnDefinitions = Columns.Define(
			(Column.Total, Stars(1)),
			(Column.Unique, Stars(1)));

		Children.Add(new StatisticsCard(SortingConstants.Views, "total_views.svg", nameof(BaseTheme.CardViewsStatsIconColor), TrendsPageAutomationIds.ViewsCard, TrendsPageAutomationIds.ViewsStatisticsLabel, deviceInfo)
			.Row(Row.ViewsStats).Column(Column.Total)
			.Bind(StatisticsCard.IsSeriesVisibleProperty,
				nameof(TrendsViewModel.IsViewsSeriesVisible),
				source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext, typeof(TrendsViewModel)))
			.Bind(StatisticsCard.TextProperty,
				nameof(TrendsViewModel.ViewsStatisticsText),
				source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext, typeof(TrendsViewModel)))
			.BindTapGesture(nameof(TrendsViewModel.ViewsCardTappedCommand),
				commandSource: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext, typeof(TrendsViewModel))));

		Children.Add(new StatisticsCard(SortingConstants.UniqueViews, "unique_views.svg", nameof(BaseTheme.CardUniqueViewsStatsIconColor), TrendsPageAutomationIds.UniqueViewsCard, TrendsPageAutomationIds.UniqueViewsStatisticsLabel, deviceInfo)
			.Row(Row.ViewsStats).Column(Column.Unique)
			.Bind(StatisticsCard.IsSeriesVisibleProperty,
				nameof(TrendsViewModel.IsUniqueViewsSeriesVisible),
				source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext, typeof(TrendsViewModel)))
			.Bind(StatisticsCard.TextProperty,
				nameof(TrendsViewModel.UniqueViewsStatisticsText),
				source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext, typeof(TrendsViewModel)))
			.BindTapGesture(nameof(TrendsViewModel.UniqueViewsCardTappedCommand),
				commandSource: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext, typeof(TrendsViewModel))));

		Children.Add(new StatisticsCard(SortingConstants.Clones, "total_clones.svg", nameof(BaseTheme.CardClonesStatsIconColor), TrendsPageAutomationIds.ClonesCard, TrendsPageAutomationIds.ClonesStatisticsLabel, deviceInfo)
			.Row(Row.ClonesStats).Column(Column.Total)
			.Bind(StatisticsCard.IsSeriesVisibleProperty,
				nameof(TrendsViewModel.IsClonesSeriesVisible),
				source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext, typeof(TrendsViewModel)))
			.Bind(StatisticsCard.TextProperty,
				nameof(TrendsViewModel.ClonesStatisticsText),
				source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext, typeof(TrendsViewModel)))
			.BindTapGesture(nameof(TrendsViewModel.ClonesCardTappedCommand),
				commandSource: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext, typeof(TrendsViewModel))));

		Children.Add(new StatisticsCard(SortingConstants.UniqueClones, "unique_clones.svg", nameof(BaseTheme.CardUniqueClonesStatsIconColor), TrendsPageAutomationIds.UniqueClonesCard, TrendsPageAutomationIds.UniqueClonesStatisticsLabel, deviceInfo)
			.Row(Row.ClonesStats).Column(Column.Unique)
			.Bind(StatisticsCard.IsSeriesVisibleProperty,
				nameof(TrendsViewModel.IsUniqueClonesSeriesVisible),
				source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext, typeof(TrendsViewModel)))
			.Bind(StatisticsCard.TextProperty,
				nameof(TrendsViewModel.UniqueClonesStatisticsText),
				source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext, typeof(TrendsViewModel)))
			.BindTapGesture(nameof(TrendsViewModel.UniqueClonesCardTappedCommand),
				commandSource: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext, typeof(TrendsViewModel))));
	}

	enum Row { ViewsStats, ClonesStats, Chart }
	enum Column { Total, Unique }
}