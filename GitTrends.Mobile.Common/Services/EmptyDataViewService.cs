using GitTrends.Mobile.Common.Constants;

namespace GitTrends.Mobile.Common;

public static class EmptyDataViewService
{
	static readonly IReadOnlyList<string> _zeroStarsEmptyDataViewDescription =
	[
		EmptyDataViewConstantsInternal.ZeroStarsEmptyDataViewDescription1,
		EmptyDataViewConstantsInternal.ZeroStarsEmptyDataViewDescription2,
		EmptyDataViewConstantsInternal.ZeroStarsEmptyDataViewDescription3
	];

	static readonly Random _random = new((int)DateTime.Now.Ticks);

	public static string GetReferringSitesTitleText(in RefreshState refreshState) => refreshState switch
	{
		RefreshState.Uninitialized => EmptyDataViewConstantsInternal.Uninitialized,
		RefreshState.Succeeded => EmptyDataViewConstantsInternal.NoReferralsYet,
		RefreshState.LoginExpired => EmptyDataViewConstantsInternal.LoginExpired,
		RefreshState.Error or RefreshState.AbuseLimit => EmptyDataViewConstantsInternal.UnableToRetrieveData,
		RefreshState.MaximumApiLimit => EmptyDataViewConstantsInternal.UnableToRetrieveData,
		_ => throw new NotSupportedException()
	};

	public static string GetReferringSitesDescriptionText(in RefreshState refreshState) => refreshState switch
	{
		RefreshState.Uninitialized => EmptyDataViewConstantsInternal.SwipeDownToRefresh_ReferringSites,
		RefreshState.Succeeded => string.Empty,
		RefreshState.LoginExpired => EmptyDataViewConstantsInternal.PleaseLoginAgain,
		RefreshState.Error or RefreshState.AbuseLimit => EmptyDataViewConstantsInternal.SwipeDownToRefresh_ReferringSites,
		RefreshState.MaximumApiLimit => EmptyDataViewConstantsInternal.SwipeDownToRefresh_ReferringSites,
		_ => throw new NotSupportedException()
	};

	public static string GetRepositoryTitleText(in RefreshState refreshState, in bool isRepositoryListEmpty) => refreshState switch
	{
		RefreshState.Uninitialized => EmptyDataViewConstantsInternal.Uninitialized,
		RefreshState.Succeeded or RefreshState.AbuseLimit when !isRepositoryListEmpty => EmptyDataViewConstantsInternal.NoFilterMatch,
		RefreshState.Succeeded or RefreshState.AbuseLimit => EmptyDataViewConstantsInternal.NoRepositoriesFound,
		RefreshState.LoginExpired => EmptyDataViewConstantsInternal.LoginExpired,
		RefreshState.Error when !isRepositoryListEmpty => EmptyDataViewConstantsInternal.NoFilterMatch,
		RefreshState.Error => EmptyDataViewConstantsInternal.UnableToRetrieveData,
		RefreshState.MaximumApiLimit => EmptyDataViewConstantsInternal.UnableToRetrieveData,
		_ => throw new NotSupportedException()
	};

	public static string GetRepositoryDescriptionText(in RefreshState refreshState, in bool isRepositoryListEmpty) => refreshState switch
	{
		RefreshState.Uninitialized => EmptyDataViewConstantsInternal.SwipeDownToRefresh_Repositories,
		RefreshState.Succeeded or RefreshState.AbuseLimit when !isRepositoryListEmpty => EmptyDataViewConstantsInternal.ClearSearchBarTryAgain,
		RefreshState.Succeeded or RefreshState.AbuseLimit => string.Empty,
		RefreshState.LoginExpired => EmptyDataViewConstantsInternal.PleaseLoginAgain,
		RefreshState.Error when !isRepositoryListEmpty => EmptyDataViewConstantsInternal.ClearSearchBarTryAgain,
		RefreshState.Error => EmptyDataViewConstantsInternal.SwipeDownToRefresh_Repositories,
		RefreshState.MaximumApiLimit => EmptyDataViewConstantsInternal.SwipeDownToRefresh_Repositories,
		_ => throw new NotSupportedException()
	};

	public static string GetViewsClonesTitleText(in RefreshState refreshState) => refreshState switch
	{
		RefreshState.Uninitialized => EmptyDataViewConstantsInternal.Uninitialized,
		RefreshState.Succeeded => EmptyDataViewConstantsInternal.NoTrafficYet,
		RefreshState.LoginExpired => EmptyDataViewConstantsInternal.LoginExpired,
		RefreshState.Error => EmptyDataViewConstantsInternal.UnableToRetrieveData,
		RefreshState.MaximumApiLimit => EmptyDataViewConstantsInternal.UnableToRetrieveData,
		_ => throw new NotSupportedException()
	};

	public static string GetStarsEmptyDataViewDescriptionText(in RefreshState refreshState, in double totalStars) => refreshState switch
	{
		RefreshState.Uninitialized => string.Empty,
		RefreshState.Succeeded when totalStars is 1 => EmptyDataViewConstantsInternal.FirstStar,
		RefreshState.Succeeded => GetZeroStarsDescription(),
		RefreshState.LoginExpired => string.Empty,
		RefreshState.Error => string.Empty,
		RefreshState.MaximumApiLimit => string.Empty,
		_ => throw new NotSupportedException()
	};

	public static string GetStarsEmptyDataViewTitleText(in RefreshState refreshState, in double totalStars) => refreshState switch
	{
		RefreshState.Uninitialized => EmptyDataViewConstantsInternal.Uninitialized,
		RefreshState.Succeeded when totalStars is 1 => EmptyDataViewConstantsInternal.Congratulations,
		RefreshState.Succeeded => EmptyDataViewConstantsInternal.NoStarsYet,
		RefreshState.LoginExpired => EmptyDataViewConstantsInternal.PleaseLoginAgain,
		RefreshState.Error => EmptyDataViewConstantsInternal.UnableToRetrieveData,
		RefreshState.MaximumApiLimit => EmptyDataViewConstantsInternal.UnableToRetrieveData,
		_ => throw new NotSupportedException()
	};

	public static string GetStarsHeaderTitleText() => EmptyDataViewConstantsInternal.TOTAL;

	public static string GetStarsHeaderMessageText(in RefreshState refreshState, double totalStars) => (refreshState, totalStars) switch
	{
		(RefreshState.Uninitialized, _) => EmptyDataViewConstantsInternal.LOADING,
		(_, 0 or 1) => TrendsChartTitleConstants.YouGotThis,
		(_, > 1) => TrendsChartTitleConstants.KeepItUp,
		(_, _) => throw new ArgumentOutOfRangeException(paramName: nameof(totalStars), message: $"{nameof(totalStars)} cannot be negative")
	};

	public static string GetViewsClonesImage(in RefreshState refreshState) => refreshState switch
	{
		RefreshState.Uninitialized => "EmptyTrafficChart",
		RefreshState.Succeeded => "EmptyTrafficChart",
		RefreshState.LoginExpired => "EmptyTrafficChart",
		RefreshState.Error => "EmptyTrafficChart",
		RefreshState.MaximumApiLimit => "EmptyTrafficChart",
		_ => throw new NotSupportedException()
	};

	public static string GetStarsEmptyDataViewImage(in RefreshState refreshState, in double totalStars) => refreshState switch
	{
		RefreshState.Uninitialized => "EmptyStarChart",
		RefreshState.Succeeded when totalStars is 1 => "EmptyOneStarChart",
		RefreshState.Succeeded => "EmptyStarChart",
		RefreshState.LoginExpired => "EmptyStarChart",
		RefreshState.Error => "EmptyStarChart",
		RefreshState.MaximumApiLimit => "EmptyStarChart",
		_ => throw new NotSupportedException()
	};

	static string GetZeroStarsDescription() => _zeroStarsEmptyDataViewDescription[_random.Next(0, _zeroStarsEmptyDataViewDescription.Count)];
}