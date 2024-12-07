using System.ComponentModel;
using GitTrends.Common;
using GitTrends.Mobile.Common.Constants;
using Microsoft.Maui.Storage;

namespace GitTrends.Mobile.Common;

public class MobileSortingService(IPreferences preferences)
{
	public const SortingOption DefaultSortingOption = SortingOption.Views;

	readonly IPreferences _preferences = preferences;

	//Keep as expression-bodied member (e.g. don't use a readonly property) to ensure the correct RESX file is uses when the language changes 
	public static IReadOnlyDictionary<SortingOption, string> SortingOptionsDictionary => new Dictionary<SortingOption, string>
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
			if (!Enum.IsDefined(value))
				throw new InvalidEnumArgumentException();

			_preferences.Set(nameof(CurrentOption), (int)value);
		}
	}

	public static IEnumerable<TReferringSiteModel> SortReferringSites<TReferringSiteModel>(in IEnumerable<TReferringSiteModel> referringSites) where TReferringSiteModel : IReferringSiteModel =>
		referringSites.OrderByDescending(static x => x.TotalCount).ThenByDescending(static x => x.TotalUniqueCount).ThenBy(static x => x.Referrer);

	//SortingCategory.IssuesForks Priority: Favorite > Trending > Stars > Name > Forks > Issues 
	//SortingCategory.Views Priority: Favorite > Trending > Views > Name > Unique Views > Stars
	//SortingCategory.Clones Priority: Favorite > Trending > Clones > Name > Unique Clones > Stars
	public static IEnumerable<Repository> SortRepositories(in IEnumerable<Repository> repositories, in SortingOption sortingOption, in bool isReversed) => sortingOption switch
	{
		SortingOption.Forks when isReversed => repositories.OrderBy(static x => x.IsFavorite is true).ThenBy(static x => x.IsTrending).ThenBy(static x => x.ForkCount).ThenByDescending(static x => x.Name).ThenBy(static x => x.StarCount).ThenBy(static x => x.IssuesCount),
		SortingOption.Forks => repositories.OrderByDescending(static x => x.IsFavorite is true).ThenByDescending(static x => x.IsTrending).ThenByDescending(static x => x.ForkCount).ThenBy(static x => x.Name).ThenByDescending(static x => x.StarCount).ThenByDescending(static x => x.IssuesCount),

		SortingOption.Issues when isReversed => repositories.OrderBy(static x => x.IsFavorite is true).ThenBy(static x => x.IsTrending).ThenBy(static x => x.IssuesCount).ThenByDescending(static x => x.Name).ThenBy(static x => x.StarCount).ThenBy(static x => x.ForkCount),
		SortingOption.Issues => repositories.OrderByDescending(static x => x.IsFavorite is true).ThenByDescending(static x => x.IsTrending).ThenByDescending(static x => x.IssuesCount).ThenBy(static x => x.Name).ThenByDescending(static x => x.StarCount).ThenByDescending(static x => x.ForkCount),

		SortingOption.Stars when isReversed => repositories.OrderBy(static x => x.IsFavorite is true).ThenBy(static x => x.IsTrending).ThenBy(static x => x.StarCount).ThenByDescending(static x => x.Name).ThenBy(static x => x.TotalViews).ThenBy(static x => x.TotalUniqueViews),
		SortingOption.Stars => repositories.OrderByDescending(static x => x.IsFavorite is true).ThenByDescending(static x => x.IsTrending).ThenByDescending(static x => x.StarCount).ThenBy(static x => x.Name).ThenByDescending(static x => x.TotalViews).ThenByDescending(static x => x.TotalUniqueViews),

		SortingOption.Clones when isReversed => repositories.OrderBy(static x => x.IsFavorite is true).ThenBy(static x => x.IsTrending).ThenBy(static x => x.TotalClones).ThenByDescending(static x => x.Name).ThenBy(static x => x.TotalUniqueClones).ThenBy(static x => x.StarCount),
		SortingOption.Clones => repositories.OrderByDescending(static x => x.IsFavorite is true).ThenByDescending(static x => x.IsTrending).ThenByDescending(static x => x.TotalClones).ThenBy(static x => x.Name).ThenByDescending(static x => x.TotalUniqueClones).ThenByDescending(static x => x.StarCount),

		SortingOption.UniqueClones when isReversed => repositories.OrderBy(static x => x.IsFavorite is true).ThenBy(static x => x.IsTrending).ThenBy(static x => x.TotalUniqueClones).ThenByDescending(static x => x.Name).ThenBy(static x => x.TotalClones).ThenBy(static x => x.StarCount),
		SortingOption.UniqueClones => repositories.OrderByDescending(static x => x.IsFavorite is true).ThenByDescending(static x => x.IsTrending).ThenByDescending(static x => x.TotalUniqueClones).ThenBy(static x => x.Name).ThenByDescending(static x => x.TotalClones).ThenByDescending(static x => x.StarCount),

		SortingOption.Views when isReversed => repositories.OrderBy(static x => x.IsFavorite is true).ThenBy(static x => x.IsTrending).ThenBy(static x => x.TotalViews).ThenByDescending(static x => x.Name).ThenBy(static x => x.TotalUniqueViews).ThenBy(static x => x.StarCount),
		SortingOption.Views => repositories.OrderByDescending(static x => x.IsFavorite is true).ThenByDescending(static x => x.IsTrending).ThenByDescending(static x => x.TotalViews).ThenBy(static x => x.Name).ThenByDescending(static x => x.TotalUniqueViews).ThenByDescending(static x => x.StarCount),

		SortingOption.UniqueViews when isReversed => repositories.OrderBy(static x => x.IsFavorite is true).ThenBy(static x => x.IsTrending).ThenBy(static x => x.TotalUniqueViews).ThenByDescending(static x => x.Name).ThenBy(static x => x.TotalViews).ThenBy(static x => x.StarCount),
		SortingOption.UniqueViews => repositories.OrderByDescending(static x => x.IsFavorite is true).ThenByDescending(static x => x.IsTrending).ThenByDescending(static x => x.TotalUniqueViews).ThenBy(static x => x.Name).ThenByDescending(static x => x.TotalViews).ThenByDescending(static x => x.StarCount),

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

		if (Enum.IsDefined(currentOption))
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