using GitTrends.Mobile.Common.Constants;

namespace GitTrends.Mobile.Common
{
    public class LoginExpiredPullToRefreshEventArgs : PullToRefreshFailedEventArgs
    {
        public LoginExpiredPullToRefreshEventArgs() : base(EmptyDataViewConstantsInternal.LoginExpired, EmptyDataViewConstantsInternal.PleaseLoginAgain)
        {
        }
    }
}
