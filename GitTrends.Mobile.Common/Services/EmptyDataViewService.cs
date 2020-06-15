using System;
using GitTrends.Mobile.Common.Constants;

namespace GitTrends.Mobile.Common
{
    public static class EmptyDataViewService
    {
        public static string GetReferringSitesTitleText(in RefreshState refreshState) => refreshState switch
        {
            RefreshState.Uninitialized => EmptyDataViewConstantsInternal.Uninitialized,
            RefreshState.Succeeded => EmptyDataViewConstantsInternal.NoReferralsYet,
            RefreshState.LoginExpired => EmptyDataViewConstantsInternal.LoginExpired,
            RefreshState.Error => EmptyDataViewConstants.UnableToRetrieveData,
            RefreshState.MaximumApiLimit => EmptyDataViewConstants.UnableToRetrieveData,
            _ => throw new NotSupportedException()
        };

        public static string GetReferringSitesDescriptionText(in RefreshState refreshState) => refreshState switch
        {
            RefreshState.Uninitialized => EmptyDataViewConstantsInternal.SwipeDownToRefresh_ReferringSites,
            RefreshState.Succeeded => string.Empty,
            RefreshState.LoginExpired => EmptyDataViewConstantsInternal.PleaseLoginAgain,
            RefreshState.Error => EmptyDataViewConstantsInternal.SwipeDownToRefresh_ReferringSites,
            RefreshState.MaximumApiLimit => EmptyDataViewConstantsInternal.SwipeDownToRefresh_ReferringSites,
            _ => throw new NotSupportedException()
        };

        public static string GetRepositoryTitleText(in RefreshState refreshState, in bool isRepositoryListEmpty) => refreshState switch
        {
            RefreshState.Uninitialized => EmptyDataViewConstantsInternal.Uninitialized,
            RefreshState.Succeeded when !isRepositoryListEmpty => EmptyDataViewConstantsInternal.NoFilterMatch,
            RefreshState.Succeeded => EmptyDataViewConstantsInternal.NoRepositoriesFound,
            RefreshState.LoginExpired => EmptyDataViewConstantsInternal.LoginExpired,
            RefreshState.Error when !isRepositoryListEmpty => EmptyDataViewConstantsInternal.NoFilterMatch,
            RefreshState.Error => EmptyDataViewConstants.UnableToRetrieveData,
            RefreshState.MaximumApiLimit => EmptyDataViewConstants.UnableToRetrieveData,
            _ => throw new NotSupportedException()
        };

        public static string GetRepositoryDescriptionText(in RefreshState refreshState, in bool isRepositoryListEmpty) => refreshState switch
        {
            RefreshState.Uninitialized => EmptyDataViewConstantsInternal.SwipeDownToRefresh_Repositories,
            RefreshState.Succeeded when !isRepositoryListEmpty => EmptyDataViewConstantsInternal.ClearSearchBarTryAgain,
            RefreshState.Succeeded => string.Empty,
            RefreshState.LoginExpired => EmptyDataViewConstantsInternal.PleaseLoginAgain,
            RefreshState.Error when !isRepositoryListEmpty => EmptyDataViewConstantsInternal.ClearSearchBarTryAgain,
            RefreshState.Error => EmptyDataViewConstantsInternal.SwipeDownToRefresh_Repositories,
            RefreshState.MaximumApiLimit => EmptyDataViewConstantsInternal.SwipeDownToRefresh_Repositories,
            _ => throw new NotSupportedException()
        };
    }
}
