using CommunityToolkit.Maui.Markup;
using GitTrends.Common;
using GitTrends.Mobile.Common;
using GitTrends.Resources;

namespace GitTrends;

class RepositoryDataTemplateSelector(in IDeviceInfo deviceInfo, in MobileSortingService sortingService) : DataTemplateSelector
{
	readonly MobileSortingService _sortingService = sortingService;

	readonly ViewsDataTemplate _viewsDataTemplate = new(deviceInfo);
	readonly ClonesDataTemplate _clonesDataTemplate = new(deviceInfo);
	readonly IssuesForksDataTemplate _issuesForksDataTemplate = new(deviceInfo);

	protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
	{
		var category = MobileSortingService.GetSortingCategory(_sortingService.CurrentOption);

		return category switch
		{
			SortingCategory.Views => _viewsDataTemplate,
			SortingCategory.Clones => _clonesDataTemplate,
			SortingCategory.IssuesForks => _issuesForksDataTemplate,
			_ => throw new NotSupportedException()
		};
	}

	static bool IsStatisticsLabelVisible(long? item) => item is not null;

	sealed class ClonesDataTemplate(IDeviceInfo deviceInfo) : BaseRepositoryDataTemplate(() => new CardView(deviceInfo, CreateClonesDataTemplateViews(deviceInfo)))
	{
		static IEnumerable<View> CreateClonesDataTemplateViews(IDeviceInfo deviceInfo) =>
		[
			new StatisticsSvgImage(deviceInfo, "total_clones.svg", nameof(BaseTheme.CardClonesStatsIconColor))
				.Row(Row.Statistics).Column(Column.Emoji1),

			//Only display the value when the Repository Data finishes loading. This avoids showing '0' while the data is loading.
			new StatisticsLabel(nameof(BaseTheme.CardClonesStatsTextColor))
				.Row(Row.Statistics).Column(Column.Statistic1)
				.Bind(Label.IsVisibleProperty,
					getter: static (Repository repository) => repository.TotalClones,
					mode: BindingMode.OneWay,
					convert: static totalClones => IsStatisticsLabelVisible(totalClones))
				.Bind(Label.TextProperty,
					getter: static (Repository repository) => repository.TotalClones,
					mode: BindingMode.OneTime,
					convert: static totalClones => totalClones.ToAbbreviatedText()),

			//Display an activity indicator while the Data is loading
			new StatisticsActivityIndicator()
				.Row(Row.Statistics).Column(Column.Statistic1)
				.Bind(ActivityIndicator.IsVisibleProperty,
					getter: static (Repository repository) => repository.TotalClones,
					mode: BindingMode.OneTime,
					convert: static totalClones => !IsStatisticsLabelVisible(totalClones)),

			new StatisticsSvgImage(deviceInfo, "unique_clones.svg", nameof(BaseTheme.CardUniqueClonesStatsIconColor))
				.Row(Row.Statistics).Column(Column.Emoji2),

			//Only display the value when the Repository Data finishes loading. This avoids showing '0' while the data is loading.
			new StatisticsLabel(nameof(BaseTheme.CardUniqueClonesStatsTextColor))
				.Row(Row.Statistics).Column(Column.Statistic2)
				.Bind(Label.IsVisibleProperty,
					getter: static (Repository repository) => repository.TotalUniqueClones,
					mode: BindingMode.OneTime,
					convert: static totalUniqueClones => IsStatisticsLabelVisible(totalUniqueClones))
				.Bind(Label.TextProperty,
					getter: static (Repository repository) => repository.TotalUniqueClones,
					mode: BindingMode.OneTime,
					convert: static totalUniqueClones => totalUniqueClones.ToAbbreviatedText()),

			//Display an activity indicator while the Data is loading
			new StatisticsActivityIndicator()
				.Row(Row.Statistics).Column(Column.Statistic2)
				.Bind(ActivityIndicator.IsVisibleProperty,
					getter: static (Repository repository) => repository.TotalUniqueClones,
					mode: BindingMode.OneTime,
					convert: static totalUniqueClones => !IsStatisticsLabelVisible(totalUniqueClones)),

			new StatisticsSvgImage(deviceInfo, "star.svg", nameof(BaseTheme.CardStarsStatsIconColor))
				.Row(Row.Statistics).Column(Column.Emoji3),

			//Only display the value when the Repository Data finishes loading. This avoids showing '0' while the data is loading.
			new StatisticsLabel(nameof(BaseTheme.CardStarsStatsTextColor))
				.Row(Row.Statistics).Column(Column.Statistic3)
				.Bind(Label.IsVisibleProperty,
					getter: static (Repository repository) => repository.StarCount,
					mode: BindingMode.OneTime,
					convert: static (long? starCount) => IsStatisticsLabelVisible(starCount))
				.Bind(Label.TextProperty,
					getter: static (Repository repository) => repository.StarCount,
					mode: BindingMode.OneTime,
					convert: static (long? starCount) => starCount.ToAbbreviatedText()),

			//Display an activity indicator while the Data is loading
			new StatisticsActivityIndicator()
				.Row(Row.Statistics).Column(Column.Statistic3)
				.Bind(ActivityIndicator.IsVisibleProperty,
					getter: static (Repository repository) => repository.StarCount,
					mode: BindingMode.OneTime,
					convert: static (long? starCount) => !IsStatisticsLabelVisible(starCount)),
		];
	}

	sealed class ViewsDataTemplate(IDeviceInfo deviceInfo) : BaseRepositoryDataTemplate(() => new CardView(deviceInfo, CreateViewsDataTemplateViews(deviceInfo)))
	{
		static IEnumerable<View> CreateViewsDataTemplateViews(IDeviceInfo deviceInfo) =>
		[
			new StatisticsSvgImage(deviceInfo, "total_views.svg", nameof(BaseTheme.CardViewsStatsIconColor))
				.Row(Row.Statistics).Column(Column.Emoji1),

			//Only display the value when the Repository Data finishes loading. This avoids showing '0' while the data is loading.
			new StatisticsLabel(nameof(BaseTheme.CardViewsStatsTextColor))
				.Row(Row.Statistics).Column(Column.Statistic1)
				.Bind(Label.IsVisibleProperty,
					getter: static (Repository repository) => repository.TotalViews,
					mode: BindingMode.OneTime,
					convert: static totalViews => IsStatisticsLabelVisible(totalViews))
				.Bind(Label.TextProperty,
					getter: static (Repository repository) => repository.TotalViews,
					mode: BindingMode.OneTime,
					convert: static totalViews => totalViews.ToAbbreviatedText()),

			//Display an activity indicator while the Data is loading
			new StatisticsActivityIndicator()
				.Row(Row.Statistics).Column(Column.Statistic1)
				.Bind(ActivityIndicator.IsVisibleProperty,
					getter: static (Repository repository) => repository.TotalViews,
					mode: BindingMode.OneTime,
					convert: static totalViews => !IsStatisticsLabelVisible(totalViews)),

			new StatisticsSvgImage(deviceInfo, "unique_views.svg", nameof(BaseTheme.CardUniqueViewsStatsIconColor))
				.Row(Row.Statistics).Column(Column.Emoji2),

			//Only display the value when the Repository Data finishes loading. This avoids showing '0' while the data is loading.
			new StatisticsLabel(nameof(BaseTheme.CardUniqueViewsStatsTextColor))
				.Row(Row.Statistics).Column(Column.Statistic2)
				.Bind(Label.IsVisibleProperty,
					getter: static (Repository repository) => repository.TotalUniqueViews,
					mode: BindingMode.OneTime,
					convert: static totalUniqueViews => IsStatisticsLabelVisible(totalUniqueViews))
				.Bind(Label.TextProperty,
					getter: static (Repository repository) => repository.TotalUniqueViews,
					mode: BindingMode.OneTime,
					convert: static totalUniqueViews => totalUniqueViews.ToAbbreviatedText()),

			//Display an activity indicator while the Data is loading
			new StatisticsActivityIndicator()
				.Row(Row.Statistics).Column(Column.Statistic2)
				.Bind(ActivityIndicator.IsVisibleProperty,
					getter: static (Repository repository) => repository.TotalUniqueViews,
					mode: BindingMode.OneTime,
					convert: static totalUniqueViews => !IsStatisticsLabelVisible(totalUniqueViews)),

			new StatisticsSvgImage(deviceInfo, "star.svg", nameof(BaseTheme.CardStarsStatsIconColor))
				.Row(Row.Statistics).Column(Column.Emoji3),

			//Only display the value when the Repository Data finishes loading. This avoids showing '0' while the data is loading.
			new StatisticsLabel(nameof(BaseTheme.CardStarsStatsTextColor))
				.Row(Row.Statistics).Column(Column.Statistic3)
				.Bind(Label.IsVisibleProperty,
					getter: static (Repository repository) => repository.StarCount,
					mode: BindingMode.OneTime,
					convert: static (long? starCount) => IsStatisticsLabelVisible(starCount))
				.Bind(Label.TextProperty,
					getter: static (Repository repository) => repository.StarCount,
					mode: BindingMode.OneTime,
					convert: static (long? starCount) => starCount.ToAbbreviatedText()),

			//Display an activity indicator while the Data is loading
			new StatisticsActivityIndicator()
				.Row(Row.Statistics).Column(Column.Statistic3)
				.Bind(ActivityIndicator.IsVisibleProperty,
					getter: static (Repository repository) => repository.StarCount,
					mode: BindingMode.OneTime,
					convert: static (long? starCount) => !IsStatisticsLabelVisible(starCount)),
		];
	}

	sealed class IssuesForksDataTemplate(IDeviceInfo deviceInfo) : BaseRepositoryDataTemplate(() => new CardView(deviceInfo, CreateIssuesForksDataTemplateViews(deviceInfo)))
	{
		static IEnumerable<View> CreateIssuesForksDataTemplateViews(IDeviceInfo deviceInfo) =>
		[
			new StatisticsSvgImage(deviceInfo, "star.svg", nameof(BaseTheme.CardStarsStatsIconColor))
				.Row(Row.Statistics).Column(Column.Emoji1),

			//Only display the value when the Repository Data finishes loading. This avoids showing '0' while the data is loading.
			new StatisticsLabel(nameof(BaseTheme.CardStarsStatsTextColor))
				.Row(Row.Statistics).Column(Column.Statistic1)
				.Bind(Label.IsVisibleProperty,
					getter: static (Repository repository) => repository.StarCount,
					mode: BindingMode.OneTime,
					convert: static (long? starCount) => IsStatisticsLabelVisible(starCount))
				.Bind(Label.TextProperty,
					getter: static (Repository repository) => repository.StarCount,
					mode: BindingMode.OneTime,
					convert: static (long? starCount) => starCount.ToAbbreviatedText()),

			//Display an activity indicator while the Data is loading
			new StatisticsActivityIndicator()
				.Row(Row.Statistics).Column(Column.Statistic1)
				.Bind(ActivityIndicator.IsVisibleProperty,
					getter: static (Repository repository) => repository.StarCount,
					mode: BindingMode.OneTime,
					convert: static (long? starCount) => !IsStatisticsLabelVisible(starCount)),

			new StatisticsSvgImage(deviceInfo, "repo_forked.svg", nameof(BaseTheme.CardForksStatsIconColor))
				.Row(Row.Statistics).Column(Column.Emoji2),

			//Only display the value when the Repository Data finishes loading. This avoids showing '0' while the data is loading.
			new StatisticsLabel(nameof(BaseTheme.CardForksStatsTextColor))
				.Row(Row.Statistics).Column(Column.Statistic2)
				.Bind(Label.IsVisibleProperty,
					getter: static (Repository repository) => repository.ForkCount,
					mode: BindingMode.OneTime,
					convert: static (long? forkCount) => IsStatisticsLabelVisible(forkCount))
				.Bind(Label.TextProperty,
					getter: static (Repository repository) => repository.ForkCount,
					mode: BindingMode.OneTime,
					convert: static (long? forkCount) => forkCount.ToAbbreviatedText()),

			//Display an activity indicator while the Data is loading
			new StatisticsActivityIndicator()
				.Row(Row.Statistics).Column(Column.Statistic2)
				.Bind(ActivityIndicator.IsVisibleProperty,
					getter: static (Repository repository) => repository.ForkCount,
					mode: BindingMode.OneTime,
					convert: static (long? forkCount) => !IsStatisticsLabelVisible(forkCount)),

			new StatisticsSvgImage(deviceInfo, "issue_opened.svg", nameof(BaseTheme.CardIssuesStatsIconColor))
				.Row(Row.Statistics).Column(Column.Emoji3),

			//Only display the value when the Repository Data finishes loading. This avoids showing '0' while the data is loading.
			new StatisticsLabel(nameof(BaseTheme.CardIssuesStatsTextColor))
				.Row(Row.Statistics).Column(Column.Statistic3)
				.Bind(Label.IsVisibleProperty,
					getter: static (Repository repository) => repository.IssuesCount,
					mode: BindingMode.OneTime,
					convert: static (long? issuesCount) => IsStatisticsLabelVisible(issuesCount))
				.Bind(Label.TextProperty,
					getter: static (Repository repository) => repository.IssuesCount,
					mode: BindingMode.OneTime,
					convert: static (long? issuesCount) => issuesCount.ToAbbreviatedText()),

			//Display an activity indicator while the Data is loading
			new StatisticsActivityIndicator()
				.Row(Row.Statistics).Column(Column.Statistic3)
				.Bind(ActivityIndicator.IsVisibleProperty,
					getter: static (Repository repository) => repository.IssuesCount,
					mode: BindingMode.OneTime,
					convert: static (long? issuesCount) => !IsStatisticsLabelVisible(issuesCount)),
		];
	}

	sealed class StatisticsSvgImage : SvgImage
	{
		public StatisticsSvgImage(IDeviceInfo deviceInfo, string fileName, string baseThemeColor)
			: base(deviceInfo, fileName, () => AppResources.GetResource<Color>(baseThemeColor), 14, 14)
		{
			VerticalOptions = LayoutOptions.Center;
			HorizontalOptions = LayoutOptions.Center;
		}
	}

	sealed class StatisticsActivityIndicator : ActivityIndicator
	{
		public StatisticsActivityIndicator()
		{
			Margin = 2;

			IsRunning = true;

			HorizontalOptions = LayoutOptions.Start;
			VerticalOptions = LayoutOptions.Center;

			SetDynamicResource(ColorProperty, nameof(BaseTheme.PrimaryTextColor));
		}
	}
}