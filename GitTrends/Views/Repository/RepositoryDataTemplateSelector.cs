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
            var sortingCategory = MobileSortingService.GetSortingCategory(_sortingService.CurrentOption);

            return sortingCategory switch
            {
                SortingCategory.Clones => new ClonesDataTemplate(),
                SortingCategory.Views => new ViewsDataTemplate(),
                SortingCategory.IssuesForks => new IssuesForksDataTemplate(),
                _ => throw new NotSupportedException()
            };
        }

        static bool IsStatisticsLabelVisibleConverter(IEnumerable<BaseDailyModel> dailyModels) => dailyModels.Any();
        static string StatisticsLabelTextConverter(long number) => number.ConvertToAbbreviatedText();

        class ClonesDataTemplate : BaseRepositoryDataTemplate
        {
            public ClonesDataTemplate() : base(CreateClonesDataTemplateViews())
            {

            }

            static IEnumerable<View> CreateClonesDataTemplateViews() => new View[]
            {
                new StatisticsSvgImage("total_clones.svg", nameof(BaseTheme.CardClonesStatsIconColor)).Row(Row.Statistics).Column(Column.Emoji1),

                //Only display the value when the Repository Data finishes loading. This avoid showing '0' while the data is loading.
                new StatisticsLabel(nameof(BaseTheme.CardClonesStatsTextColor)).Row(Row.Statistics).Column(Column.Statistic1)
                    .Bind<StatisticsLabel, long, string>(Label.TextProperty, nameof(Repository.TotalClones), convert: StatisticsLabelTextConverter)
                    .Bind<StatisticsLabel, IReadOnlyList<DailyClonesModel>, bool>(VisualElement.IsVisibleProperty, nameof(Repository.DailyClonesList), convert: IsStatisticsLabelVisibleConverter),

                new StatisticsSvgImage("unique_clones.svg", nameof(BaseTheme.CardUniqueClonesStatsIconColor)).Row(Row.Statistics).Column(Column.Emoji2),

                //Only display the value when the Repository Data finishes loading. This avoid showing '0' while the data is loading.
                new StatisticsLabel(nameof(BaseTheme.CardUniqueClonesStatsTextColor)).Row(Row.Statistics).Column(Column.Statistic2)
                    .Bind<StatisticsLabel, long, string>(Label.TextProperty, nameof(Repository.TotalUniqueClones), convert: StatisticsLabelTextConverter)
                    .Bind<StatisticsLabel, IReadOnlyList<DailyClonesModel>, bool>(VisualElement.IsVisibleProperty, nameof(Repository.DailyClonesList), convert: IsStatisticsLabelVisibleConverter),

                new StatisticsSvgImage("star.svg", nameof(BaseTheme.CardStarsStatsIconColor)).Row(Row.Statistics).Column(Column.Emoji3),

                //Only display the value when the Repository Data finishes loading. This avoid showing '0' while the data is loading.
                new StatisticsLabel(nameof(BaseTheme.CardStarsStatsTextColor)).Row(Row.Statistics).Column(Column.Statistic3)
                    .Bind<StatisticsLabel, long, string>(Label.TextProperty, nameof(Repository.StarCount), convert: StatisticsLabelTextConverter)
                    .Bind<StatisticsLabel, IReadOnlyList<DailyClonesModel>, bool>(VisualElement.IsVisibleProperty, nameof(Repository.DailyClonesList), convert: IsStatisticsLabelVisibleConverter),
            };
        }

        class ViewsDataTemplate : BaseRepositoryDataTemplate
        {
            public ViewsDataTemplate() : base(CreateViewsDataTemplateViews())
            {

            }

            static IEnumerable<View> CreateViewsDataTemplateViews() => new View[]
            {
                new StatisticsSvgImage("total_views.svg", nameof(BaseTheme.CardViewsStatsIconColor)).Row(Row.Statistics).Column(Column.Emoji1),

                //Only display the value when the Repository Data finishes loading. This avoid showing '0' while the data is loading.
                new StatisticsLabel(nameof(BaseTheme.CardViewsStatsTextColor)).Row(Row.Statistics).Column(Column.Statistic1)
                    .Bind<StatisticsLabel, long, string>(Label.TextProperty, nameof(Repository.TotalViews), convert: StatisticsLabelTextConverter)
                    .Bind<StatisticsLabel, IReadOnlyList<DailyViewsModel>, bool>(VisualElement.IsVisibleProperty, nameof(Repository.DailyViewsList), convert: IsStatisticsLabelVisibleConverter),

                new StatisticsSvgImage("unique_views.svg", nameof(BaseTheme.CardUniqueViewsStatsIconColor)).Row(Row.Statistics).Column(Column.Emoji2),

                //Only display the value when the Repository Data finishes loading. This avoid showing '0' while the data is loading.
                new StatisticsLabel(nameof(BaseTheme.CardUniqueViewsStatsTextColor)).Row(Row.Statistics).Column(Column.Statistic2)
                    .Bind<StatisticsLabel, long, string>(Label.TextProperty, nameof(Repository.TotalUniqueViews), convert: StatisticsLabelTextConverter)
                    .Bind<StatisticsLabel, IReadOnlyList<DailyViewsModel>, bool>(VisualElement.IsVisibleProperty, nameof(Repository.DailyViewsList), convert: IsStatisticsLabelVisibleConverter),

                new StatisticsSvgImage("star.svg", nameof(BaseTheme.CardStarsStatsIconColor)).Row(Row.Statistics).Column(Column.Emoji3),

                //Only display the value when the Repository Data finishes loading. This avoid showing '0' while the data is loading.
                new StatisticsLabel(nameof(BaseTheme.CardStarsStatsTextColor)).Row(Row.Statistics).Column(Column.Statistic3)
                    .Bind<StatisticsLabel, long, string>(Label.TextProperty, nameof(Repository.StarCount), convert: StatisticsLabelTextConverter)
                    .Bind<StatisticsLabel, IReadOnlyList<DailyViewsModel>, bool>(VisualElement.IsVisibleProperty, nameof(Repository.DailyViewsList), convert: IsStatisticsLabelVisibleConverter),
            };
        }

        class IssuesForksDataTemplate : BaseRepositoryDataTemplate
        {
            public IssuesForksDataTemplate() : base(CreateIssuesForksDataTemplateViews())
            {

            }

            static IEnumerable<View> CreateIssuesForksDataTemplateViews() => new View[]
            {
                new StatisticsSvgImage("star.svg", nameof(BaseTheme.CardStarsStatsIconColor)).Row(Row.Statistics).Column(Column.Emoji1),

                //Only display the value when the Repository Data finishes loading. This avoid showing '0' while the data is loading.
                new StatisticsLabel(nameof(BaseTheme.CardStarsStatsTextColor)).Row(Row.Statistics).Column(Column.Statistic1)
                    .Bind<StatisticsLabel, long, string>(Label.TextProperty, nameof(Repository.StarCount), convert: StatisticsLabelTextConverter)
                    .Bind<StatisticsLabel, IReadOnlyList<DailyViewsModel>, bool>(VisualElement.IsVisibleProperty, nameof(Repository.DailyViewsList), convert: IsStatisticsLabelVisibleConverter),

                new StatisticsSvgImage("repo_forked.svg", nameof(BaseTheme.CardForksStatsIconColor)).Row(Row.Statistics).Column(Column.Emoji2),

                //Only display the value when the Repository Data finishes loading. This avoid showing '0' while the data is loading.
                new StatisticsLabel(nameof(BaseTheme.CardForksStatsTextColor)).Row(Row.Statistics).Column(Column.Statistic2)
                    .Bind<StatisticsLabel, long, string>(Label.TextProperty, nameof(Repository.ForkCount), convert: StatisticsLabelTextConverter)
                    .Bind<StatisticsLabel, IReadOnlyList<DailyViewsModel>, bool>(VisualElement.IsVisibleProperty, nameof(Repository.DailyViewsList), convert: IsStatisticsLabelVisibleConverter),

                new StatisticsSvgImage("issue_opened.svg", nameof(BaseTheme.CardIssuesStatsIconColor)).Row(Row.Statistics).Column(Column.Emoji3),

                //Only display the value when the Repository Data finishes loading. This avoid showing '0' while the data is loading.
                new StatisticsLabel(nameof(BaseTheme.CardIssuesStatsTextColor)).Row(Row.Statistics).Column(Column.Statistic3)
                    .Bind<StatisticsLabel, long, string>(Label.TextProperty, nameof(Repository.IssuesCount), convert: StatisticsLabelTextConverter)
                    .Bind<StatisticsLabel, IReadOnlyList<DailyViewsModel>, bool>(VisualElement.IsVisibleProperty, nameof(Repository.DailyViewsList), convert: IsStatisticsLabelVisibleConverter),
            };
        }        
    }
}