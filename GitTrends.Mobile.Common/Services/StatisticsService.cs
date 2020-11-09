using System;
using System.Collections.Generic;
using System.Linq;
using GitTrends.Mobile.Common.Constants;
using GitTrends.Shared;

namespace GitTrends.Mobile.Common
{
    public static class StatisticsService
    {
        public static string ToAbbreviatedText(this int number) => ToAbbreviatedText((double)number);

        public static string ToAbbreviatedText(this long number) => ToAbbreviatedText((double)number);

        public static string ToAbbreviatedText(this double number)
        {
            if (number < 10e2)
                return string.Format("{0:0}", number);
            else if (number < 10e5)
                return $"{string.Format("{0:0.0}", number / 10e2)}K";
            else if (number < 10e8)
                return $"{string.Format("{0:0.0}", number / 10e5)}M";
            else if (number < 10e11)
                return $"{string.Format("{0:0.0}", number / 10e8)}B";
            else if (number < 10e14)
                return $"{string.Format("{0:0.0}", number / 10e11)}T";

            return "0";
        }

        public static string GetInformationLabelText<TRepository>(in IReadOnlyList<TRepository> repositories, in MobileSortingService mobileSortingService) where TRepository : IRepository =>
            GetInformationLabelText(repositories, MobileSortingService.GetSortingCategory(mobileSortingService.CurrentOption));

        public static string GetInformationLabelText<TRepository>(in IReadOnlyList<TRepository> repositories, in SortingCategory sortingCategory) where TRepository : IRepository => (repositories.Any(), sortingCategory) switch
        {
            (false, _) => string.Empty,
            (true, SortingCategory.Views) => $"{SortingConstants.Views} {repositories.Sum(x => x.TotalViews).ToAbbreviatedText()}, {SortingConstants.UniqueViews} {repositories.Sum(x => x.TotalUniqueViews).ToAbbreviatedText()}, {SortingConstants.Stars} {repositories.Sum(x => x.StarCount).ToAbbreviatedText()}",
            (true, SortingCategory.Clones) => $"{SortingConstants.Clones} {repositories.Sum(x => x.TotalClones).ToAbbreviatedText()}, {SortingConstants.UniqueClones} {repositories.Sum(x => x.TotalUniqueClones).ToAbbreviatedText()}, {SortingConstants.Stars} {repositories.Sum(x => x.StarCount).ToAbbreviatedText()}",
            (true, SortingCategory.IssuesForks) => $"{SortingConstants.Stars} {repositories.Sum(x => x.StarCount).ToAbbreviatedText()}, {SortingConstants.Forks} {repositories.Sum(x => x.ForkCount).ToAbbreviatedText()}, {SortingConstants.Issues} {repositories.Sum(x => x.IssuesCount).ToAbbreviatedText()}",
            (_, _) => throw new NotSupportedException()
        };

        public static string GetFloatingActionTextButtonText<TRepository>(in MobileSortingService mobileSortingService, in IReadOnlyList<TRepository> repositories, in FloatingActionButtonType floatingActionButtonType) where TRepository : IRepository =>
            GetFloatingActionTextButtonText(MobileSortingService.GetSortingCategory(mobileSortingService.CurrentOption), repositories.ToList(), floatingActionButtonType);

        public static string GetFloatingActionTextButtonText<TRepository>(in SortingCategory sortingCategory, in IReadOnlyList<TRepository> repositories, in FloatingActionButtonType floatingActionButtonType) where TRepository : IRepository
        {
            return (sortingCategory, floatingActionButtonType) switch
            {
                (SortingCategory.Clones, FloatingActionButtonType.Statistic1) => repositories.Sum(x => x.TotalClones).ToAbbreviatedText(),
                (SortingCategory.Clones, FloatingActionButtonType.Statistic2) => repositories.Sum(x => x.TotalUniqueClones).ToAbbreviatedText(),
                (SortingCategory.Clones, FloatingActionButtonType.Statistic3) => repositories.Sum(x => x.StarCount).ToAbbreviatedText(),
                (SortingCategory.Views, FloatingActionButtonType.Statistic1) => repositories.Sum(x => x.TotalViews).ToAbbreviatedText(),
                (SortingCategory.Views, FloatingActionButtonType.Statistic2) => repositories.Sum(x => x.TotalUniqueViews).ToAbbreviatedText(),
                (SortingCategory.Views, FloatingActionButtonType.Statistic3) => repositories.Sum(x => x.StarCount).ToAbbreviatedText(),
                (SortingCategory.IssuesForks, FloatingActionButtonType.Statistic1) => repositories.Sum(x => x.StarCount).ToAbbreviatedText(),
                (SortingCategory.IssuesForks, FloatingActionButtonType.Statistic2) => repositories.Sum(x => x.ForkCount).ToAbbreviatedText(),
                (SortingCategory.IssuesForks, FloatingActionButtonType.Statistic3) => repositories.Sum(x => x.IssuesCount).ToAbbreviatedText(),
                (_, FloatingActionButtonType.Information) => throw new NotSupportedException(),
                (_, _) => throw new NotImplementedException()
            };
        }
    }
}
