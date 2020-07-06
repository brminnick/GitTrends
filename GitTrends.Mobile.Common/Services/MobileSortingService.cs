using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using GitTrends.Mobile.Common.Constants;
using GitTrends.Shared;
using Xamarin.Essentials.Interfaces;

namespace GitTrends.Mobile.Common
{
    public class MobileSortingService
    {
        public const SortingOption DefaultSortingOption = SortingOption.Views;

        readonly IPreferences _preferences;

        public MobileSortingService(IPreferences preferences) => _preferences = preferences;

        //Keep as expression-bodied member (e.g. don't use a readonly property) to ensure the correct RESX file is uses when the language changes 
        public static Dictionary<SortingOption, string> SortingOptionsDictionary => new Dictionary<SortingOption, string>
        {
            { SortingOption.Stars,  SortingConstants.Stars },
            { SortingOption.Forks,  SortingConstants.Forks },
            { SortingOption.Issues,  SortingConstants.Issues },
            { SortingOption.Views,  SortingConstants.Views },
            { SortingOption.Clones,  SortingConstants.Clones },
            { SortingOption.UniqueViews,  SortingConstants.UniqueViews },
            { SortingOption.UniqueClones,  SortingConstants.UniqueClones },
        };

        public bool IsReversed
        {
            get => _preferences.Get(nameof(IsReversed), false);
            set => _preferences.Set(nameof(IsReversed), value);
        }

        public SortingOption CurrentOption
        {
            get => GetCurrentOption();
            set
            {
                if (!Enum.IsDefined(typeof(SortingOption), value))
                    throw new InvalidEnumArgumentException();

                _preferences.Set(nameof(CurrentOption), (int)value);
            }
        }

        public static IEnumerable<TReferringSiteModel> SortReferringSites<TReferringSiteModel>(in IEnumerable<TReferringSiteModel> referringSites) where TReferringSiteModel : IReferringSiteModel =>
            referringSites.OrderByDescending(x => x.TotalCount).ThenByDescending(x => x.TotalUniqueCount).ThenBy(x => x.Referrer);

        //SortingCategory.IssuesForks Priority: Trending > Stars > Name > Forks > Issues 
        //SortingCategory.Views Priority: Trending > Views > Name > Unique Views > Stars
        //SortingCategory.Clones Priority: Trending > Clones > Name > Unique Clones > Stars
        public static IEnumerable<Repository> SortRepositories(in IEnumerable<Repository> repositories, in SortingOption sortingOption, in bool isReversed) => sortingOption switch
        {
            SortingOption.Forks when isReversed => repositories.OrderBy(x => x.IsTrending).ThenBy(x => x.ForkCount).ThenByDescending(x => x.Name).ThenBy(x => x.StarCount).ThenBy(x => x.IssuesCount),
            SortingOption.Forks => repositories.OrderByDescending(x => x.IsTrending).ThenByDescending(x => x.ForkCount).ThenBy(x => x.Name).ThenByDescending(x => x.StarCount).ThenByDescending(x => x.IssuesCount),

            SortingOption.Issues when isReversed => repositories.OrderBy(x => x.IsTrending).ThenBy(x => x.IssuesCount).ThenByDescending(x => x.Name).ThenBy(x => x.StarCount).ThenBy(x => x.ForkCount),
            SortingOption.Issues => repositories.OrderByDescending(x => x.IsTrending).ThenByDescending(x => x.IssuesCount).ThenBy(x => x.Name).ThenByDescending(x => x.StarCount).ThenByDescending(x => x.ForkCount),

            SortingOption.Stars when isReversed => repositories.OrderBy(x => x.IsTrending).ThenBy(x => x.StarCount).ThenByDescending(x => x.Name).ThenBy(x => x.TotalViews).ThenBy(x => x.TotalUniqueViews),
            SortingOption.Stars => repositories.OrderByDescending(x => x.IsTrending).ThenByDescending(x => x.StarCount).ThenBy(x => x.Name).ThenByDescending(x => x.TotalViews).ThenByDescending(x => x.TotalUniqueViews),

            SortingOption.Clones when isReversed => repositories.OrderBy(x => x.IsTrending).ThenBy(x => x.TotalClones).ThenByDescending(x => x.Name).ThenBy(x => x.TotalUniqueClones).ThenBy(x => x.StarCount),
            SortingOption.Clones => repositories.OrderByDescending(x => x.IsTrending).ThenByDescending(x => x.TotalClones).ThenBy(x => x.Name).ThenByDescending(x => x.TotalUniqueClones).ThenByDescending(x => x.StarCount),

            SortingOption.UniqueClones when isReversed => repositories.OrderBy(x => x.IsTrending).ThenBy(x => x.TotalUniqueClones).ThenByDescending(x => x.Name).ThenBy(x => x.TotalClones).ThenBy(x => x.StarCount),
            SortingOption.UniqueClones => repositories.OrderByDescending(x => x.IsTrending).ThenByDescending(x => x.TotalUniqueClones).ThenBy(x => x.Name).ThenByDescending(x => x.TotalClones).ThenByDescending(x => x.StarCount),

            SortingOption.Views when isReversed => repositories.OrderBy(x => x.IsTrending).ThenBy(x => x.TotalViews).ThenByDescending(x => x.Name).ThenBy(x => x.TotalUniqueViews).ThenBy(x => x.StarCount),
            SortingOption.Views => repositories.OrderByDescending(x => x.IsTrending).ThenByDescending(x => x.TotalViews).ThenBy(x => x.Name).ThenByDescending(x => x.TotalUniqueViews).ThenByDescending(x => x.StarCount),

            SortingOption.UniqueViews when isReversed => repositories.OrderBy(x => x.IsTrending).ThenBy(x => x.TotalUniqueViews).ThenByDescending(x => x.Name).ThenBy(x => x.TotalViews).ThenBy(x => x.StarCount),
            SortingOption.UniqueViews => repositories.OrderByDescending(x => x.IsTrending).ThenByDescending(x => x.TotalUniqueViews).ThenBy(x => x.Name).ThenByDescending(x => x.TotalViews).ThenByDescending(x => x.StarCount),

            _ => throw new NotSupportedException()
        };

        public static SortingCategory GetSortingCategory(in SortingOption sortingOption) => sortingOption switch
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
                _preferences.Remove(nameof(CurrentOption));
                return getCurrentSortingOption();
            }

            SortingOption getCurrentSortingOption() => (SortingOption)_preferences.Get(nameof(CurrentOption), (int)DefaultSortingOption);
        }
    }

    public enum SortingOption { Stars, Issues, Forks, Views, UniqueViews, Clones, UniqueClones }

    public enum SortingCategory { IssuesForks, Views, Clones }
}
