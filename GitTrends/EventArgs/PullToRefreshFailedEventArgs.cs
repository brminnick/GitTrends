namespace GitTrends
{
    public class PullToRefreshFailedEventArgs : System.EventArgs
    {
        public PullToRefreshFailedEventArgs(string message, string title) => (ErrorMessage, ErrorTitle) = (message, title);

        public string ErrorMessage { get; }
        public string ErrorTitle { get; }
    }
}
