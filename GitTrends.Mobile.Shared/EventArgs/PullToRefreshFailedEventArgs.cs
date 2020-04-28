namespace GitTrends.Mobile.Shared
{
    public abstract class PullToRefreshFailedEventArgs : System.EventArgs
    {
        protected PullToRefreshFailedEventArgs(string message, string title, string dissmissText = "OK") =>
            (ErrorMessage, ErrorTitle, DismissText) = (message, title, dissmissText);

        public string ErrorMessage { get; }
        public string ErrorTitle { get; }
        public string DismissText { get; }
    }
}
