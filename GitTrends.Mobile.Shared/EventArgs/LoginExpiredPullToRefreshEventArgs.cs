namespace GitTrends.Mobile.Shared
{
    public class LoginExpiredPullToRefreshEventArgs : PullToRefreshFailedEventArgs
    {
        public LoginExpiredPullToRefreshEventArgs() : base("Login Expired", "Please login again")
        {
        }
    }
}
