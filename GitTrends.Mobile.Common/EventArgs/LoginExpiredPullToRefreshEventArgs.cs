using GitTrends.Mobile.Common.Constants;

namespace GitTrends.Mobile.Common;

public class LoginExpiredPullToRefreshEventArgs() : PullToRefreshFailedEventArgs(EmptyDataViewConstantsInternal.LoginExpired, EmptyDataViewConstantsInternal.PleaseLoginAgain)
{
}