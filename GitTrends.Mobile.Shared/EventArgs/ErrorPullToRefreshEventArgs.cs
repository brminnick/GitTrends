namespace GitTrends.Mobile.Shared
{
    public class ErrorPullToRefreshEventArgs : PullToRefreshFailedEventArgs
    {
        public ErrorPullToRefreshEventArgs(string message) : base("Error", message)
        {
        }
    }
}
