namespace GitTrends.Mobile.Shared
{
    public abstract class PullToRefreshFailedEventArgs : System.EventArgs
    {
        protected PullToRefreshFailedEventArgs(string message, string title) => (ErrorMessage, ErrorTitle) = (message, title);

        public string ErrorMessage { get; }
        public string ErrorTitle { get; }
        public string DismissText { get; } = "OK";
    }
}
