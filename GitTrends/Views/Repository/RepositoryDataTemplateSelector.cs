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
		readonly RepositoryViewModel _repositoryViewModel;

		public RepositoryDataTemplateSelector(in MobileSortingService sortingService, in RepositoryViewModel repositoryViewModel) =>
			(_sortingService, _repositoryViewModel) = (sortingService, repositoryViewModel);

		protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
		{
			var repository = (Repository)item;

			var sortingCategory = MobileSortingService.GetSortingCategory(_sortingService.CurrentOption);

			return sortingCategory switch
			{
				SortingCategory.Clones => new ClonesDataTemplate(_repositoryViewModel, repository),
				SortingCategory.Views => new ViewsDataTemplate(_repositoryViewModel, repository),
				SortingCategory.IssuesForks => new IssuesForksDataTemplate(_repositoryViewModel, repository),
				_ => throw new NotSupportedException()
			};
		}

		static bool IsStatisticsLabelVisible(object? item) => item is not null;

		class ClonesDataTemplate : BaseRepositoryDataTemplate
		{
			public ClonesDataTemplate(in RepositoryViewModel repositoryViewModel, in Repository repository) : base(CreateClonesDataTemplateViews(repository), repositoryViewModel, repository)
			{

			}

			static IEnumerable<View> CreateClonesDataTemplateViews(in Repository repository) => new View[]
			{
				new StatisticsSvgImage("total_clones.svg", nameof(BaseTheme.CardClonesStatsIconColor)).Row(Row.Statistics).Column(Column.Emoji1),

                //Only display the value when the Repository Data finishes loading. This avoid showing '0' while the data is loading.
                new StatisticsLabel(repository.TotalClones.ToAbbreviatedText(), IsStatisticsLabelVisible(repository.TotalClones), nameof(BaseTheme.CardClonesStatsTextColor)).Row(Row.Statistics).Column(Column.Statistic1),

                //Display an activity indicator while the Data is loading
                new StatisticsActivityIndicator(!IsStatisticsLabelVisible(repository.TotalClones)).Row(Row.Statistics).Column(Column.Statistic1),

				new StatisticsSvgImage("unique_clones.svg", nameof(BaseTheme.CardUniqueClonesStatsIconColor)).Row(Row.Statistics).Column(Column.Emoji2),

                //Only display the value when the Repository Data finishes loading. This avoid showing '0' while the data is loading.
                new StatisticsLabel(repository.TotalUniqueClones.ToAbbreviatedText(), IsStatisticsLabelVisible(repository.TotalUniqueClones), nameof(BaseTheme.CardUniqueClonesStatsTextColor)).Row(Row.Statistics).Column(Column.Statistic2),

                //Display an activity indicator while the Data is loading
                new StatisticsActivityIndicator(!IsStatisticsLabelVisible(repository.TotalUniqueClones)).Row(Row.Statistics).Column(Column.Statistic2),

				new StatisticsSvgImage("star.svg", nameof(BaseTheme.CardStarsStatsIconColor)).Row(Row.Statistics).Column(Column.Emoji3),

                //Only display the value when the Repository Data finishes loading. This avoid showing '0' while the data is loading.
                new StatisticsLabel(repository.StarCount.ToAbbreviatedText(), IsStatisticsLabelVisible(repository.StarCount), nameof(BaseTheme.CardStarsStatsTextColor)).Row(Row.Statistics).Column(Column.Statistic3),

                //Display an activity indicator while the Data is loading
                new StatisticsActivityIndicator(!IsStatisticsLabelVisible(repository.StarCount)).Row(Row.Statistics).Column(Column.Statistic3),
			};
		}

		class ViewsDataTemplate : BaseRepositoryDataTemplate
		{
			public ViewsDataTemplate(in RepositoryViewModel repositoryViewModel, in Repository repository) : base(CreateViewsDataTemplateViews(repository), repositoryViewModel, repository)
			{

			}

			static IEnumerable<View> CreateViewsDataTemplateViews(in Repository repository) => new View[]
			{
				new StatisticsSvgImage("total_views.svg", nameof(BaseTheme.CardViewsStatsIconColor)).Row(Row.Statistics).Column(Column.Emoji1),

                //Only display the value when the Repository Data finishes loading. This avoid showing '0' while the data is loading.
                new StatisticsLabel(repository.TotalViews.ToAbbreviatedText(), IsStatisticsLabelVisible(repository.TotalViews), nameof(BaseTheme.CardViewsStatsTextColor)).Row(Row.Statistics).Column(Column.Statistic1),

                //Display an activity indicator while the Data is loading
                new StatisticsActivityIndicator(!IsStatisticsLabelVisible(repository.TotalViews)).Row(Row.Statistics).Column(Column.Statistic1),

				new StatisticsSvgImage("unique_views.svg", nameof(BaseTheme.CardUniqueViewsStatsIconColor)).Row(Row.Statistics).Column(Column.Emoji2),

                //Only display the value when the Repository Data finishes loading. This avoid showing '0' while the data is loading.
                new StatisticsLabel(repository.TotalUniqueViews.ToAbbreviatedText(), IsStatisticsLabelVisible(repository.TotalUniqueViews), nameof(BaseTheme.CardUniqueViewsStatsTextColor)).Row(Row.Statistics).Column(Column.Statistic2),

                //Display an activity indicator while the Data is loading
                new StatisticsActivityIndicator(!IsStatisticsLabelVisible(repository.TotalUniqueViews)).Row(Row.Statistics).Column(Column.Statistic2),

				new StatisticsSvgImage("star.svg", nameof(BaseTheme.CardStarsStatsIconColor)).Row(Row.Statistics).Column(Column.Emoji3),

                //Only display the value when the Repository Data finishes loading. This avoid showing '0' while the data is loading.
                new StatisticsLabel(repository.StarCount.ToAbbreviatedText(), IsStatisticsLabelVisible(repository.StarCount), nameof(BaseTheme.CardStarsStatsTextColor)).Row(Row.Statistics).Column(Column.Statistic3),

                //Display an activity indicator while the Data is loading
                new StatisticsActivityIndicator(!IsStatisticsLabelVisible(repository.StarCount)).Row(Row.Statistics).Column(Column.Statistic3),
			};
		}

		class IssuesForksDataTemplate : BaseRepositoryDataTemplate
		{
			public IssuesForksDataTemplate(in RepositoryViewModel repositoryViewModel, in Repository repository) : base(CreateIssuesForksDataTemplateViews(repository), repositoryViewModel, repository)
			{

			}

			static IEnumerable<View> CreateIssuesForksDataTemplateViews(in Repository repository) => new View[]
			{
				new StatisticsSvgImage("star.svg", nameof(BaseTheme.CardStarsStatsIconColor)).Row(Row.Statistics).Column(Column.Emoji1),

                //Only display the value when the Repository Data finishes loading. This avoid showing '0' while the data is loading.
                new StatisticsLabel(repository.StarCount.ToAbbreviatedText(), IsStatisticsLabelVisible(repository.StarCount), nameof(BaseTheme.CardStarsStatsTextColor)).Row(Row.Statistics).Column(Column.Statistic1),

                //Display an activity indicator while the Data is loading
                new StatisticsActivityIndicator(!IsStatisticsLabelVisible(repository.StarCount)).Row(Row.Statistics).Column(Column.Statistic1),

				new StatisticsSvgImage("repo_forked.svg", nameof(BaseTheme.CardForksStatsIconColor)).Row(Row.Statistics).Column(Column.Emoji2),

                //Only display the value when the Repository Data finishes loading. This avoid showing '0' while the data is loading.
                new StatisticsLabel(repository.ForkCount.ToAbbreviatedText(), IsStatisticsLabelVisible(repository.ForkCount), nameof(BaseTheme.CardForksStatsTextColor)).Row(Row.Statistics).Column(Column.Statistic2),

                //Display an activity indicator while the Data is loading
                new StatisticsActivityIndicator(!IsStatisticsLabelVisible(repository.ForkCount)).Row(Row.Statistics).Column(Column.Statistic2),

				new StatisticsSvgImage("issue_opened.svg", nameof(BaseTheme.CardIssuesStatsIconColor)).Row(Row.Statistics).Column(Column.Emoji3),

                //Only display the value when the Repository Data finishes loading. This avoid showing '0' while the data is loading.
                new StatisticsLabel(repository.IssuesCount.ToAbbreviatedText(), IsStatisticsLabelVisible(repository.IssuesCount), nameof(BaseTheme.CardIssuesStatsTextColor)).Row(Row.Statistics).Column(Column.Statistic3),

                //Display an activity indicator while the Data is loading
                new StatisticsActivityIndicator(!IsStatisticsLabelVisible(repository.IssuesCount)).Row(Row.Statistics).Column(Column.Statistic3),
			};
		}

		class StatisticsActivityIndicator : ActivityIndicator
		{
			public StatisticsActivityIndicator(bool isVisible)
			{
				IsVisible = isVisible;
				IsRunning = isVisible;

				Scale = 0.67;

				HorizontalOptions = LayoutOptions.Start;
				VerticalOptions = LayoutOptions.Center;

				SetDynamicResource(ActivityIndicator.ColorProperty, nameof(BaseTheme.PrimaryTextColor));
			}
		}
	}
}