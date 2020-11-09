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
            RefreshState.Error => EmptyDataViewConstantsInternal.UnableToRetrieveData,
            RefreshState.MaximumApiLimit => EmptyDataViewConstantsInternal.UnableToRetrieveData,
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
            RefreshState.Error => EmptyDataViewConstantsInternal.UnableToRetrieveData,
            RefreshState.MaximumApiLimit => EmptyDataViewConstantsInternal.UnableToRetrieveData,
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

        public static string GetViewsClonesTitleText(in RefreshState refreshState) => refreshState switch
        {
            RefreshState.Uninitialized => EmptyDataViewConstantsInternal.Uninitialized,
            RefreshState.Succeeded => EmptyDataViewConstantsInternal.NoTrafficYet,
            RefreshState.LoginExpired => EmptyDataViewConstantsInternal.LoginExpired,
            RefreshState.Error => EmptyDataViewConstantsInternal.UnableToRetrieveData,
            RefreshState.MaximumApiLimit => EmptyDataViewConstantsInternal.UnableToRetrieveData,
            _ => throw new NotSupportedException()
        };

#if AppStore
#error Stars Chart Refresh State not complete
#endif

        public static string GetStarsTitleText(in RefreshState refreshState, in double totalStars) => refreshState switch
        {
            RefreshState.Uninitialized => "Uninitialized",
            RefreshState.Succeeded when totalStars is 1 => "1 Star Text",
            RefreshState.Succeeded => "0 Stars Text",
            RefreshState.LoginExpired => EmptyDataViewConstantsInternal.PleaseLoginAgain,
            RefreshState.Error => EmptyDataViewConstantsInternal.UnableToRetrieveData,
            RefreshState.MaximumApiLimit => EmptyDataViewConstantsInternal.UnableToRetrieveData,
            _ => throw new NotSupportedException()
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

        public static string GetStarsImage(in RefreshState refreshState, in double totalStars) => refreshState switch
        {
            RefreshState.Uninitialized => "EmptyStarChart",
            RefreshState.Succeeded when totalStars is 1 => "1StarEmptyChartImage",
            RefreshState.Succeeded => "EmptyStarChart",
            RefreshState.LoginExpired => "EmptyStarChart",
            RefreshState.Error => "EmptyStarChart",
            RefreshState.MaximumApiLimit => "EmptyStarChart",
            _ => throw new NotSupportedException()
        };
    }
}
