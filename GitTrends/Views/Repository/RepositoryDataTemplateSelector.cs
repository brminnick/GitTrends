using System;
using System.Collections.Generic;
using System.Linq;
using GitTrends.Mobile.Common;
using GitTrends.Shared;
using Xamarin.Forms;
using Xamarin.Forms.Markup;

namespace GitTrends
{
    class RepositoryDataTemplateSelector : DataTemplateSelector
    {
        readonly MobileSortingService _sortingService;

        public RepositoryDataTemplateSelector(MobileSortingService sortingService) => _sortingService = sortingService;

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            var repository = (Repository)item;

            var sortingCategory = MobileSortingService.GetSortingCategory(_sortingService.CurrentOption);

            return sortingCategory switch
            {
                SortingCategory.Clones => new ClonesDataTemplate(repository),
                SortingCategory.Views => new ViewsDataTemplate(repository),
                SortingCategory.IssuesForks => new IssuesForksDataTemplate(repository),
                _ => throw new NotSupportedException()
            };
        }

        static bool IsStatisticsLabelVisible(IEnumerable<BaseDailyModel> dailyModels) => dailyModels.Any();

        class ClonesDataTemplate : BaseRepositoryDataTemplate
        {
            public ClonesDataTemplate(in Repository repository) : base(CreateClonesDataTemplateViews(repository), repository)
            {

            }

            static IEnumerable<View> CreateClonesDataTemplateViews(in Repository repository) => new View[]
            {
                new StatisticsSvgImage("total_clones.svg", nameof(BaseTheme.CardClonesStatsIconColor)).Row(Row.Statistics).Column(Column.Emoji1),

                //Only display the value when the Repository Data finishes loading. This avoid showing '0' while the data is loading.
                new StatisticsLabel(repository.TotalClones.ToAbbreviatedText(), IsStatisticsLabelVisible(repository.DailyClonesList), nameof(BaseTheme.CardClonesStatsTextColor)).Row(Row.Statistics).Column(Column.Statistic1),

                new StatisticsSvgImage("unique_clones.svg", nameof(BaseTheme.CardUniqueClonesStatsIconColor)).Row(Row.Statistics).Column(Column.Emoji2),

                //Only display the value when the Repository Data finishes loading. This avoid showing '0' while the data is loading.
                new StatisticsLabel(repository.TotalUniqueClones.ToAbbreviatedText(), IsStatisticsLabelVisible(repository.DailyClonesList), nameof(BaseTheme.CardUniqueClonesStatsTextColor)).Row(Row.Statistics).Column(Column.Statistic2),

                new StatisticsSvgImage("star.svg", nameof(BaseTheme.CardStarsStatsIconColor)).Row(Row.Statistics).Column(Column.Emoji3),

                //Only display the value when the Repository Data finishes loading. This avoid showing '0' while the data is loading.
                new StatisticsLabel(repository.StarCount.ToAbbreviatedText(), IsStatisticsLabelVisible(repository.DailyClonesList), nameof(BaseTheme.CardStarsStatsTextColor)).Row(Row.Statistics).Column(Column.Statistic3),
            };
        }

        class ViewsDataTemplate : BaseRepositoryDataTemplate
        {
            public ViewsDataTemplate(in Repository repository) : base(CreateViewsDataTemplateViews(repository), repository)
            {

            }

            static IEnumerable<View> CreateViewsDataTemplateViews(in Repository repository) => new View[]
            {
                new StatisticsSvgImage("total_views.svg", nameof(BaseTheme.CardViewsStatsIconColor)).Row(Row.Statistics).Column(Column.Emoji1),

                //Only display the value when the Repository Data finishes loading. This avoid showing '0' while the data is loading.
                new StatisticsLabel(repository.TotalViews.ToAbbreviatedText(), IsStatisticsLabelVisible(repository.DailyViewsList), nameof(BaseTheme.CardViewsStatsTextColor)).Row(Row.Statistics).Column(Column.Statistic1),

                new StatisticsSvgImage("unique_views.svg", nameof(BaseTheme.CardUniqueViewsStatsIconColor)).Row(Row.Statistics).Column(Column.Emoji2),

                //Only display the value when the Repository Data finishes loading. This avoid showing '0' while the data is loading.
                new StatisticsLabel(repository.TotalUniqueViews.ToAbbreviatedText(), IsStatisticsLabelVisible(repository.DailyViewsList), nameof(BaseTheme.CardUniqueViewsStatsTextColor)).Row(Row.Statistics).Column(Column.Statistic2),

                new StatisticsSvgImage("star.svg", nameof(BaseTheme.CardStarsStatsIconColor)).Row(Row.Statistics).Column(Column.Emoji3),

                //Only display the value when the Repository Data finishes loading. This avoid showing '0' while the data is loading.
                new StatisticsLabel(repository.StarCount.ToAbbreviatedText(), IsStatisticsLabelVisible(repository.DailyViewsList), nameof(BaseTheme.CardStarsStatsTextColor)).Row(Row.Statistics).Column(Column.Statistic3),
            };
        }

        class IssuesForksDataTemplate : BaseRepositoryDataTemplate
        {
            public IssuesForksDataTemplate(in Repository repository) : base(CreateIssuesForksDataTemplateViews(repository), repository)
            {

            }

            static IEnumerable<View> CreateIssuesForksDataTemplateViews(in Repository repository) => new View[]
            {
                new StatisticsSvgImage("star.svg", nameof(BaseTheme.CardStarsStatsIconColor)).Row(Row.Statistics).Column(Column.Emoji1),

                //Only display the value when the Repository Data finishes loading. This avoid showing '0' while the data is loading.
                new StatisticsLabel(repository.StarCount.ToAbbreviatedText(), IsStatisticsLabelVisible(repository.DailyViewsList), nameof(BaseTheme.CardStarsStatsTextColor)).Row(Row.Statistics).Column(Column.Statistic1),

                new StatisticsSvgImage("repo_forked.svg", nameof(BaseTheme.CardForksStatsIconColor)).Row(Row.Statistics).Column(Column.Emoji2),

                //Only display the value when the Repository Data finishes loading. This avoid showing '0' while the data is loading.
                new StatisticsLabel(repository.ForkCount.ToAbbreviatedText(), IsStatisticsLabelVisible(repository.DailyViewsList), nameof(BaseTheme.CardForksStatsTextColor)).Row(Row.Statistics).Column(Column.Statistic2),

                new StatisticsSvgImage("issue_opened.svg", nameof(BaseTheme.CardIssuesStatsIconColor)).Row(Row.Statistics).Column(Column.Emoji3),

                //Only display the value when the Repository Data finishes loading. This avoid showing '0' while the data is loading.
                new StatisticsLabel(repository.IssuesCount.ToAbbreviatedText(), IsStatisticsLabelVisible(repository.DailyViewsList), nameof(BaseTheme.CardIssuesStatsTextColor)).Row(Row.Statistics).Column(Column.Statistic3),
            };
        }
    }
}