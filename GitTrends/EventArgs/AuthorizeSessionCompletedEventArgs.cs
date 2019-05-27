namespace GitTrends
{
    public class AuthorizeSessionCompletedEventArgs : System.EventArgs
    {
        public AuthorizeSessionCompletedEventArgs(bool isSessionAuthorized) => IsSessionAuthorized = isSessionAuthorized;

        public bool IsSessionAuthorized { get; }
    }
}
