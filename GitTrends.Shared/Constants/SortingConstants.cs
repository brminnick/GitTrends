using System;
using System.Collections.Generic;

namespace GitTrends.Shared
{
    class SortingConstants
    {
        readonly static Lazy<Dictionary<SortingOption, string>> _sortingOptionsDictionaryHolder = new Lazy<Dictionary<SortingOption, string>>(CreateSortingDictionary);

        public static Dictionary<SortingOption, string> SortingOptionsDictionary => _sortingOptionsDictionaryHolder.Value;

        static Dictionary<SortingOption, string> CreateSortingDictionary() => new Dictionary<SortingOption, string>
        {
            { SortingOption.Stars,  "Stars" },
            { SortingOption.Issues,  "Issues" },
            { SortingOption.Forks,  "Forks" },
            { SortingOption.Views,  "Views" },
            { SortingOption.UniqueViews,  "Unique Views" },
            { SortingOption.Clones,  "Clones" },
            { SortingOption.UniqueClones,  "Unique Clones" },
        };
    }

    public enum SortingOption  { Stars, Issues, Forks, Views, UniqueViews, Clones, UniqueClones }
}
