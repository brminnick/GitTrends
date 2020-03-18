using System;
using System.Collections.Generic;

namespace GitTrends.Shared
{
    class SortingConstants
    {
        public const SortingOption DefaultSortingOption = SortingOption.Trending;

        readonly static Lazy<Dictionary<SortingOption, string>> _sortingOptionsDictionaryHolder = new Lazy<Dictionary<SortingOption, string>>(CreateSortingDictionary);

        public static Dictionary<SortingOption, string> SortingOptionsDictionary => _sortingOptionsDictionaryHolder.Value;

        static Dictionary<SortingOption, string> CreateSortingDictionary() => new Dictionary<SortingOption, string>
        {
            { SortingOption.Stars,  "Stars" },
            { SortingOption.Forks,  "Forks" },
            { SortingOption.Issues,  "Issues" },
            { SortingOption.Views,  "Views" },
            { SortingOption.Clones,  "Clones" },
            { SortingOption.UniqueViews,  "Unique Views" },
            { SortingOption.UniqueClones,  "Unique Clones" },
            { SortingOption.Trending, "Trending" }
        };
    }

    public enum SortingOption  { Stars, Issues, Forks, Views, UniqueViews, Clones, UniqueClones, Trending }
}
