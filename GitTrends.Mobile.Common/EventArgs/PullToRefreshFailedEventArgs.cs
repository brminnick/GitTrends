namespace GitTrends.Mobile.Common
{
    public abstract class PullToRefreshFailedEventArgs : System.EventArgs
    {
        protected PullToRefreshFailedEventArgs(string title, string message, string cancel = "OK", string? accept = null) =>
            (Message, Title, Cancel, Accept) = (message, title, cancel, accept);

        public string Message { get; }
        public string Title { get; }
        public string Cancel { get; }
        public string? Accept { get; }
    }
}
