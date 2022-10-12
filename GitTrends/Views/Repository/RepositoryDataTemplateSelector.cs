using System;
using System.Collections.Generic;
using GitTrends.Mobile.Common;
using GitTrends.Shared;
using Xamarin.CommunityToolkit.Markup;
using Xamarin.Forms;

namespace GitTrends
{
	class RepositoryDataTemplateSelector : DataTemplateSelector
	{
		readonly MobileSortingService _sortingService;

		readonly ViewsDataTemplate _viewsDataTemplate = new();
		readonly ClonesDataTemplate _clonesDataTemplate = new();
		readonly IssuesForksDataTemplate _issuesForksDataTemplate = new();

		public RepositoryDataTemplateSelector(in MobileSortingService sortingService)
		{
			_sortingService = sortingService;
		}

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

		class ClonesDataTemplate : BaseRepositoryDataTemplate
		{
			public ClonesDataTemplate() : base(() => new CardView(CreateClonesDataTemplateViews()))
			{

			}

			static IEnumerable<View> CreateClonesDataTemplateViews() => new View[]
			{
				new StatisticsSvgImage("total_clones.svg", nameof(BaseTheme.CardClonesStatsIconColor))
					.Row(Row.Statistics).Column(Column.Emoji1),

                //Only display the value when the Repository Data finishes loading. This avoid showing '0' while the data is loading.
                new StatisticsLabel(nameof(BaseTheme.CardClonesStatsTextColor))
					.Row(Row.Statistics).Column(Column.Statistic1)
					.Bind(Label.IsVisibleProperty, nameof(Repository.TotalClones), BindingMode.OneWay, convert: static (long? totalClones) => IsStatisticsLabelVisible(totalClones))
					.Bind(Label.TextProperty, nameof(Repository.TotalClones), BindingMode.OneTime, convert:  static (long? totalClones) => totalClones.ToAbbreviatedText()),

                //Display an activity indicator while the Data is loading
                new StatisticsActivityIndicator()
					.Row(Row.Statistics).Column(Column.Statistic1)
					.Bind(Label.IsVisibleProperty, nameof(Repository.TotalClones), BindingMode.OneTime, convert: static (long? totalClones) => !IsStatisticsLabelVisible(totalClones)),

				new StatisticsSvgImage("unique_clones.svg", nameof(BaseTheme.CardUniqueClonesStatsIconColor))
					.Row(Row.Statistics).Column(Column.Emoji2),

                //Only display the value when the Repository Data finishes loading. This avoid showing '0' while the data is loading.
                new StatisticsLabel(nameof(BaseTheme.CardUniqueClonesStatsTextColor))
					.Row(Row.Statistics).Column(Column.Statistic2)
					.Bind(Label.IsVisibleProperty, nameof(Repository.TotalUniqueClones), BindingMode.OneTime, convert: static (long? totalUniqueClones) => IsStatisticsLabelVisible(totalUniqueClones))
					.Bind(Label.TextProperty, nameof(Repository.TotalUniqueClones), BindingMode.OneTime, convert: static (long? totalUniqueClones) => totalUniqueClones.ToAbbreviatedText()),

                //Display an activity indicator while the Data is loading
                new StatisticsActivityIndicator()
					.Row(Row.Statistics).Column(Column.Statistic2)
					.Bind(Label.IsVisibleProperty, nameof(Repository.TotalUniqueClones), BindingMode.OneTime, convert: static (long? totalUniqueClones) => !IsStatisticsLabelVisible(totalUniqueClones)),

				new StatisticsSvgImage("star.svg", nameof(BaseTheme.CardStarsStatsIconColor))
					.Row(Row.Statistics).Column(Column.Emoji3),

                //Only display the value when the Repository Data finishes loading. This avoid showing '0' while the data is loading.
                new StatisticsLabel(nameof(BaseTheme.CardStarsStatsTextColor))
					.Row(Row.Statistics).Column(Column.Statistic3)
					.Bind(Label.IsVisibleProperty, nameof(Repository.StarCount), BindingMode.OneTime, convert: static (long? starCount) => IsStatisticsLabelVisible(starCount))
					.Bind(Label.TextProperty, nameof(Repository.StarCount), BindingMode.OneTime, convert: static (long? starCount) => starCount.ToAbbreviatedText()),

                //Display an activity indicator while the Data is loading
                new StatisticsActivityIndicator()
					.Row(Row.Statistics).Column(Column.Statistic3)
					.Bind(Label.IsVisibleProperty, nameof(Repository.StarCount), BindingMode.OneTime, convert: static (long? starCount) => !IsStatisticsLabelVisible(starCount)),
			};
		}

		class ViewsDataTemplate : BaseRepositoryDataTemplate
		{
			public ViewsDataTemplate() : base(() => new CardView(CreateViewsDataTemplateViews()))
			{

			}

			static IEnumerable<View> CreateViewsDataTemplateViews() => new View[]
			{
				new StatisticsSvgImage("total_views.svg", nameof(BaseTheme.CardViewsStatsIconColor))
					.Row(Row.Statistics).Column(Column.Emoji1),

                //Only display the value when the Repository Data finishes loading. This avoid showing '0' while the data is loading.
                new StatisticsLabel(nameof(BaseTheme.CardViewsStatsTextColor))
					.Row(Row.Statistics).Column(Column.Statistic1)
					.Bind(Label.IsVisibleProperty, nameof(Repository.TotalViews), BindingMode.OneTime, convert: static (long? totalViews) => IsStatisticsLabelVisible(totalViews))
					.Bind(Label.TextProperty, nameof(Repository.TotalViews), BindingMode.OneTime, convert: static (long? totalViews) => totalViews.ToAbbreviatedText()),

                //Display an activity indicator while the Data is loading
                new StatisticsActivityIndicator()
					.Row(Row.Statistics).Column(Column.Statistic1)
					.Bind(Label.IsVisibleProperty, nameof(Repository.TotalViews), BindingMode.OneTime, convert: static (long? totalViews) => !IsStatisticsLabelVisible(totalViews)),

				new StatisticsSvgImage("unique_views.svg", nameof(BaseTheme.CardUniqueViewsStatsIconColor))
					.Row(Row.Statistics).Column(Column.Emoji2),

                //Only display the value when the Repository Data finishes loading. This avoid showing '0' while the data is loading.
                new StatisticsLabel(nameof(BaseTheme.CardUniqueViewsStatsTextColor))
					.Row(Row.Statistics).Column(Column.Statistic2)
					.Bind(Label.IsVisibleProperty, nameof(Repository.TotalUniqueViews), BindingMode.OneTime, convert: static (long? totalUniqueViews) => IsStatisticsLabelVisible(totalUniqueViews))
					.Bind(Label.TextProperty, nameof(Repository.TotalUniqueViews), BindingMode.OneTime, convert: static (long? totalUniqueViews) => totalUniqueViews.ToAbbreviatedText()),

                //Display an activity indicator while the Data is loading
                new StatisticsActivityIndicator()
					.Row(Row.Statistics).Column(Column.Statistic2)
					.Bind(Label.IsVisibleProperty, nameof(Repository.TotalUniqueViews), BindingMode.OneTime, convert: static (long? totalUniqueViews) => !IsStatisticsLabelVisible(totalUniqueViews)),

				new StatisticsSvgImage("star.svg", nameof(BaseTheme.CardStarsStatsIconColor))
					.Row(Row.Statistics).Column(Column.Emoji3),

                //Only display the value when the Repository Data finishes loading. This avoid showing '0' while the data is loading.
                new StatisticsLabel(nameof(BaseTheme.CardStarsStatsTextColor))
					.Row(Row.Statistics).Column(Column.Statistic3)
					.Bind(Label.IsVisibleProperty, nameof(Repository.StarCount), BindingMode.OneTime, convert: static (long? starCount) => IsStatisticsLabelVisible(starCount))
					.Bind(Label.TextProperty, nameof(Repository.StarCount), BindingMode.OneTime, convert: static (long? starCount) => starCount.ToAbbreviatedText()),

                //Display an activity indicator while the Data is loading
                new StatisticsActivityIndicator()
					.Row(Row.Statistics).Column(Column.Statistic3)
					.Bind(Label.IsVisibleProperty, nameof(Repository.StarCount), BindingMode.OneTime, convert: static (long? starCount) => !IsStatisticsLabelVisible(starCount)),
			};
		}

		class IssuesForksDataTemplate : BaseRepositoryDataTemplate
		{
			public IssuesForksDataTemplate() : base(() => new CardView(CreateIssuesForksDataTemplateViews()))
			{

			}

			static IEnumerable<View> CreateIssuesForksDataTemplateViews() => new View[]
			{
				new StatisticsSvgImage("star.svg", nameof(BaseTheme.CardStarsStatsIconColor))
					.Row(Row.Statistics).Column(Column.Emoji1),

                //Only display the value when the Repository Data finishes loading. This avoid showing '0' while the data is loading.
                new StatisticsLabel(nameof(BaseTheme.CardStarsStatsTextColor))
					.Row(Row.Statistics).Column(Column.Statistic1)
					.Bind(Label.IsVisibleProperty, nameof(Repository.StarCount), BindingMode.OneTime, convert: static (long? starCount) => IsStatisticsLabelVisible(starCount))
					.Bind(Label.TextProperty, nameof(Repository.StarCount), BindingMode.OneTime, convert: static (long? starCount) => starCount.ToAbbreviatedText()),

                //Display an activity indicator while the Data is loading
                new StatisticsActivityIndicator()
					.Row(Row.Statistics).Column(Column.Statistic1)
					.Bind(Label.IsVisibleProperty, nameof(Repository.StarCount), BindingMode.OneTime, convert: static (long? starCount) => !IsStatisticsLabelVisible(starCount)),

				new StatisticsSvgImage("repo_forked.svg", nameof(BaseTheme.CardForksStatsIconColor))
					.Row(Row.Statistics).Column(Column.Emoji2),

                //Only display the value when the Repository Data finishes loading. This avoid showing '0' while the data is loading.
                new StatisticsLabel(nameof(BaseTheme.CardForksStatsTextColor))
					.Row(Row.Statistics).Column(Column.Statistic2)
					.Bind(Label.IsVisibleProperty, nameof(Repository.ForkCount), BindingMode.OneTime, convert: static (long? forkCount) => IsStatisticsLabelVisible(forkCount))
					.Bind(Label.TextProperty, nameof(Repository.ForkCount), BindingMode.OneTime, convert: static (long? forkCount) => forkCount.ToAbbreviatedText()),

                //Display an activity indicator while the Data is loading
                new StatisticsActivityIndicator()
					.Row(Row.Statistics).Column(Column.Statistic2)
					.Bind(Label.IsVisibleProperty, nameof(Repository.ForkCount), BindingMode.OneTime, convert: static (long? forkCount) => !IsStatisticsLabelVisible(forkCount)),

				new StatisticsSvgImage("issue_opened.svg", nameof(BaseTheme.CardIssuesStatsIconColor))
					.Row(Row.Statistics).Column(Column.Emoji3),

                //Only display the value when the Repository Data finishes loading. This avoid showing '0' while the data is loading.
                new StatisticsLabel(nameof(BaseTheme.CardIssuesStatsTextColor))
					.Row(Row.Statistics).Column(Column.Statistic3)
					.Bind(Label.IsVisibleProperty, nameof(Repository.IssuesCount), BindingMode.OneTime, convert: static (long? issuesCount) => IsStatisticsLabelVisible(issuesCount))
					.Bind(Label.TextProperty, nameof(Repository.IssuesCount), BindingMode.OneTime, convert: static (long? issuesCount) => issuesCount.ToAbbreviatedText()),

                //Display an activity indicator while the Data is loading
                new StatisticsActivityIndicator()
					.Row(Row.Statistics).Column(Column.Statistic3)
					.Bind(Label.IsVisibleProperty, nameof(Repository.IssuesCount), BindingMode.OneTime, convert: static (long? issuesCount) => !IsStatisticsLabelVisible(issuesCount)),
			};
		}

		class StatisticsSvgImage : SvgImage
		{
			public StatisticsSvgImage(string fileName, string baseThemeColor, in double widthRequest = 24, in double heightRequest = 24)
				: base(fileName, () => (Color)Application.Current.Resources[baseThemeColor], widthRequest, heightRequest)
			{
				VerticalOptions = LayoutOptions.CenterAndExpand;
				HorizontalOptions = LayoutOptions.EndAndExpand;
			}
		}

		class StatisticsActivityIndicator : ActivityIndicator
		{
			public StatisticsActivityIndicator()
			{
				IsRunning = true;

				Scale = 0.67;

				HorizontalOptions = LayoutOptions.Start;
				VerticalOptions = LayoutOptions.Center;

				SetDynamicResource(ActivityIndicator.ColorProperty, nameof(BaseTheme.PrimaryTextColor));
			}
		}
	}
}