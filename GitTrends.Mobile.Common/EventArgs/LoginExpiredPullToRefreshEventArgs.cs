namespace GitTrends.Mobile.Common
{
    public class LoginExpiredPullToRefreshEventArgs : PullToRefreshFailedEventArgs
    {
        public LoginExpiredPullToRefreshEventArgs() : base("Login Expired", "Please login again")
        {
        }
    }
}
