using System;

namespace GitTrends.Mobile.Shared
{
    public static class EmptyDataViewConstants
    {
        public const string UnableToRetrieveData = "Unable to retrieve data";

        const string _pleaseLoginAgain = "Please login again";
        const string _swipeDownToRefresh = "Swipe down to retrieve referring sites";
        const string _loginExpired = "GitHub Login Expired";
        const string _uninitialized = "Data not gathered";

        const string _noFilterMatch = "No Matching Repository Found";
        const string _noRepositoriesFound = "Your repositories list is empty";
        const string _clearSearchBarTryAgain = "Clear search bar and try again";

        const string _noReferralsYet = "No referrals yet";

        public static string GetReferringSitesTitleText(in RefreshState refreshState) => refreshState switch
        {
            RefreshState.Uninitialized => _uninitialized,
            RefreshState.Succeeded => _noReferralsYet,
            RefreshState.LoginExpired => _loginExpired,
            RefreshState.Error => UnableToRetrieveData,
            RefreshState.MaximumApiLimit => UnableToRetrieveData,
            _ => throw new NotSupportedException()
        };

        public static string GetReferringSitesDescriptionText(in RefreshState refreshState) => refreshState switch
        {
            RefreshState.Uninitialized => _swipeDownToRefresh,
            RefreshState.Succeeded => string.Empty,
            RefreshState.LoginExpired => _pleaseLoginAgain,
            RefreshState.Error => _swipeDownToRefresh,
            RefreshState.MaximumApiLimit => _swipeDownToRefresh,
            _ => throw new NotSupportedException()
        };

        public static string GetRepositoryTitleText(in RefreshState refreshState, in bool isRepositoryListEmpty) => refreshState switch
        {
            RefreshState.Uninitialized => _uninitialized,
            RefreshState.Succeeded when !isRepositoryListEmpty => _noFilterMatch,
            RefreshState.Succeeded => _noRepositoriesFound,
            RefreshState.LoginExpired => _loginExpired,
            RefreshState.Error when !isRepositoryListEmpty => _noFilterMatch,
            RefreshState.Error => UnableToRetrieveData,
            RefreshState.MaximumApiLimit => UnableToRetrieveData,
            _ => throw new NotSupportedException()
        };

        public static string GetRepositoryDescriptionText(in RefreshState refreshState, in bool isRepositoryListEmpty) => refreshState switch
        {
            RefreshState.Uninitialized => _swipeDownToRefresh,
            RefreshState.Succeeded when !isRepositoryListEmpty => _clearSearchBarTryAgain,
            RefreshState.Succeeded => string.Empty,
            RefreshState.LoginExpired => _pleaseLoginAgain,
            RefreshState.Error when !isRepositoryListEmpty => _clearSearchBarTryAgain,
            RefreshState.Error => _swipeDownToRefresh,
            RefreshState.MaximumApiLimit => _swipeDownToRefresh,
            _ => throw new NotSupportedException()
        };
    }
}
