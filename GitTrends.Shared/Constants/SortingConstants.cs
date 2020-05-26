using System;
using System.Collections.Generic;

namespace GitTrends.Shared
{
    public class SortingConstants
    {
        public const string CancelText = "Cancel";
        public const SortingOption DefaultSortingOption = SortingOption.Views;

        readonly static Lazy<Dictionary<SortingOption, string>> _sortingOptionsDictionaryHolder = new Lazy<Dictionary<SortingOption, string>>(CreateSortingDictionary);

        public static Dictionary<SortingOption, string> SortingOptionsDictionary => _sortingOptionsDictionaryHolder.Value;

        public static SortingCategory GetSortingCategory(SortingOption sortingOption) => sortingOption switch
        {
            SortingOption.Stars => SortingCategory.Views,
            SortingOption.Forks => SortingCategory.IssuesForks,
            SortingOption.Issues => SortingCategory.IssuesForks,
            SortingOption.Views => SortingCategory.Views,
            SortingOption.Clones => SortingCategory.Clones,
            SortingOption.UniqueViews => SortingCategory.Views,
            SortingOption.UniqueClones => SortingCategory.Clones,
            _ => throw new NotSupportedException()
        };

        static Dictionary<SortingOption, string> CreateSortingDictionary() => new Dictionary<SortingOption, string>
        {
            { SortingOption.Stars,  "Stars" },
            { SortingOption.Forks,  "Forks" },
            { SortingOption.Issues,  "Issues" },
            { SortingOption.Views,  "Views" },
            { SortingOption.Clones,  "Clones" },
            { SortingOption.UniqueViews,  "Unique Views" },
            { SortingOption.UniqueClones,  "Unique Clones" },
        };
    }

    public enum SortingOption { Stars, Issues, Forks, Views, UniqueViews, Clones, UniqueClones }

    public enum SortingCategory { IssuesForks, Views, Clones }
}
