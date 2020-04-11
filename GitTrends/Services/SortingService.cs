using System;
using System.Collections.Generic;
using System.Linq;
using GitTrends.Shared;
using Xamarin.Essentials;

namespace GitTrends
{
    public class SortingService
    {
        public bool IsReversed
        {
            get => Preferences.Get(nameof(IsReversed), false);
            set => Preferences.Set(nameof(IsReversed), value);
        }

        public SortingOption CurrentOption
        {
            get => GetCurrentOption();
            set => Preferences.Set(nameof(CurrentOption), (int)value);
        }

        //SortingCategory.IssuesForks Priority: Trending > Stars > Forks > Issues
        //SortingCategory.Views Priority: Trending > Views > Unique Views > Stars
        //SortingCategory.Clones Priority: Trending > Clones > Unique Clones > Stars
        public static IEnumerable<Repository> SortRepositories(in IEnumerable<Repository> repositories, in SortingOption sortingOption, in bool isReversed) => sortingOption switch
        {
            SortingOption.Forks when isReversed => repositories.OrderBy(x => x.IsTrending).ThenBy(x => x.ForkCount).ThenBy(x => x.StarCount).ThenBy(x => x.IssuesCount),
            SortingOption.Forks => repositories.OrderByDescending(x => x.IsTrending).ThenByDescending(x => x.ForkCount).ThenByDescending(x => x.StarCount).ThenByDescending(x => x.IssuesCount),

            SortingOption.Issues when isReversed => repositories.OrderBy(x => x.IsTrending).ThenBy(x => x.IssuesCount).ThenBy(x => x.StarCount).ThenBy(x => x.ForkCount),
            SortingOption.Issues => repositories.OrderByDescending(x => x.IsTrending).ThenByDescending(x => x.IssuesCount).ThenByDescending(x => x.StarCount).ThenByDescending(x => x.ForkCount),

            SortingOption.Stars when isReversed => repositories.OrderBy(x => x.IsTrending).ThenBy(x => x.StarCount).ThenBy(x => x.TotalViews).ThenBy(x => x.TotalUniqueViews),
            SortingOption.Stars => repositories.OrderByDescending(x => x.IsTrending).ThenByDescending(x => x.StarCount).ThenByDescending(x => x.TotalViews).ThenByDescending(x => x.TotalUniqueViews),

            SortingOption.Clones when isReversed => repositories.OrderBy(x => x.IsTrending).ThenBy(x => x.TotalClones).ThenBy(x => x.TotalUniqueClones).ThenBy(x => x.StarCount),
            SortingOption.Clones => repositories.OrderByDescending(x => x.IsTrending).ThenByDescending(x => x.TotalClones).ThenByDescending(x => x.TotalUniqueClones).ThenByDescending(x => x.StarCount),

            SortingOption.UniqueClones when isReversed => repositories.OrderBy(x => x.IsTrending).ThenBy(x => x.TotalUniqueClones).ThenBy(x => x.TotalClones).ThenBy(x => x.StarCount),
            SortingOption.UniqueClones => repositories.OrderByDescending(x => x.IsTrending).ThenByDescending(x => x.TotalUniqueClones).ThenByDescending(x => x.TotalClones).ThenByDescending(x => x.StarCount),

            SortingOption.Views when isReversed => repositories.OrderBy(x => x.IsTrending).ThenBy(x => x.TotalViews).ThenBy(x => x.TotalUniqueViews).ThenBy(x => x.StarCount),
            SortingOption.Views => repositories.OrderByDescending(x => x.IsTrending).ThenByDescending(x => x.TotalViews).ThenByDescending(x => x.TotalUniqueViews).ThenByDescending(x => x.StarCount),

            SortingOption.UniqueViews when isReversed => repositories.OrderBy(x => x.IsTrending).ThenBy(x => x.TotalUniqueViews).ThenBy(x => x.TotalViews).ThenBy(x => x.StarCount),
            SortingOption.UniqueViews => repositories.OrderByDescending(x => x.IsTrending).ThenByDescending(x => x.TotalUniqueViews).ThenByDescending(x => x.TotalViews).ThenByDescending(x => x.StarCount),

            _ => throw new NotSupportedException()
        };

        //Bug Fix caused by removing SortingConstants.Trending between v0.12.0 and v1.0
        SortingOption GetCurrentOption()
        {
            var currentOption = getCurrentSortingOption();

            if (Enum.IsDefined(typeof(SortingOption), currentOption))
            {
                return currentOption;                
            }
            else
            {
                Preferences.Remove(nameof(CurrentOption));
                return getCurrentSortingOption();
            }

            SortingOption getCurrentSortingOption() => (SortingOption)Preferences.Get(nameof(CurrentOption), (int)SortingConstants.DefaultSortingOption);
        }
    }
}
