namespace GitTrends.Mobile.Shared
{
    public abstract class PullToRefreshFailedEventArgs : System.EventArgs
    {
        protected PullToRefreshFailedEventArgs(string title, string message, string dismissText = "OK") =>
            (Message, Title, DismissText) = (message, title, dismissText);

        public string Message { get; }
        public string Title { get; }
        public string DismissText { get; }
    }
}
