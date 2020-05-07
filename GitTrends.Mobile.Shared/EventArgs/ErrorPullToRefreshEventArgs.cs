namespace GitTrends.Mobile.Shared
{
    public class ErrorPullToRefreshEventArgs : PullToRefreshFailedEventArgs
    {
        public ErrorPullToRefreshEventArgs(string message) : base("Unable To Connect To GitHub", message)
        {
        }
    }
}
