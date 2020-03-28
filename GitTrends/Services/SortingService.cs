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
            get => (SortingOption)Preferences.Get(nameof(CurrentOption), (int)SortingConstants.DefaultSortingOption);
            set => Preferences.Set(nameof(CurrentOption), (int)value);
        }

        //Sorting Priority: Stars > Forks > Issues
        //Sorting Priority: Clones > Unique Clones > Views > Unique Views
        //Sorting Priority: Views > Unique Views > Clones > Unique Clones
        //Sorting Priiroty: Trending > Views > Unique Views > Clones > Unique Clones
        public static IEnumerable<Repository> SortRepositories(in IEnumerable<Repository> repositories, in SortingOption sortingOption, in bool isReversed) => sortingOption switch
        {
            SortingOption.Forks when isReversed => repositories.OrderBy(x => x.ForkCount).ThenBy(x => x.StarCount).ThenBy(x => x.IssuesCount),
            SortingOption.Forks => repositories.OrderByDescending(x => x.ForkCount).ThenByDescending(x => x.StarCount).ThenByDescending(x => x.IssuesCount),

            SortingOption.Issues when isReversed => repositories.OrderBy(x => x.IssuesCount).ThenBy(x => x.StarCount).ThenBy(x => x.ForkCount),
            SortingOption.Issues => repositories.OrderByDescending(x => x.IssuesCount).ThenByDescending(x => x.StarCount).ThenByDescending(x => x.ForkCount),

            SortingOption.Stars when isReversed => repositories.OrderBy(x => x.StarCount).ThenBy(x => x.ForkCount).ThenBy(x => x.IssuesCount),
            SortingOption.Stars => repositories.OrderByDescending(x => x.StarCount).ThenByDescending(x => x.ForkCount).ThenByDescending(x => x.IssuesCount),

            SortingOption.Clones when isReversed => repositories.OrderBy(x => x.TotalClones).ThenBy(x => x.TotalUniqueClones).ThenBy(x => x.TotalViews).ThenBy(x => x.TotalUniqueViews),
            SortingOption.Clones => repositories.OrderByDescending(x => x.TotalClones).ThenByDescending(x => x.TotalUniqueClones).ThenByDescending(x => x.TotalViews).ThenByDescending(x => x.TotalUniqueViews),

            SortingOption.UniqueClones when isReversed => repositories.OrderBy(x => x.TotalUniqueClones).ThenBy(x => x.TotalClones).ThenBy(x => x.TotalViews).ThenBy(x => x.TotalUniqueViews),
            SortingOption.UniqueClones => repositories.OrderByDescending(x => x.TotalUniqueClones).ThenByDescending(x => x.TotalClones).ThenByDescending(x => x.TotalViews).ThenByDescending(x => x.TotalUniqueViews),

            SortingOption.Views when isReversed => repositories.OrderBy(x => x.TotalViews).ThenBy(x => x.TotalUniqueViews).ThenBy(x => x.TotalClones).ThenBy(x => x.TotalUniqueClones),
            SortingOption.Views => repositories.OrderByDescending(x => x.TotalViews).ThenByDescending(x => x.TotalUniqueViews).ThenByDescending(x => x.TotalClones).ThenByDescending(x => x.TotalUniqueClones),

            SortingOption.UniqueViews when isReversed => repositories.OrderBy(x => x.TotalUniqueViews).ThenBy(x => x.TotalViews).ThenBy(x => x.TotalClones).ThenBy(x => x.TotalUniqueClones),
            SortingOption.UniqueViews => repositories.OrderByDescending(x => x.TotalUniqueViews).ThenByDescending(x => x.TotalViews).ThenByDescending(x => x.TotalClones).ThenByDescending(x => x.TotalUniqueClones),

            SortingOption.Trending when isReversed => repositories.OrderBy(x => x.IsTrending).ThenBy(x => x.TotalViews).ThenBy(x => x.TotalUniqueViews).ThenBy(x => x.TotalClones).ThenBy(x => x.TotalUniqueClones),
            SortingOption.Trending => repositories.OrderByDescending(x => x.IsTrending).ThenByDescending(x => x.TotalViews).ThenByDescending(x => x.TotalUniqueViews).ThenByDescending(x => x.TotalClones).ThenByDescending(x => x.TotalUniqueClones),

            _ => throw new NotSupportedException()
        };
    }
}
