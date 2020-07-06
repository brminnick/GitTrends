using GitTrends.Mobile.Common.Constants;

namespace GitTrends.Mobile.Common
{
    public class ErrorPullToRefreshEventArgs : PullToRefreshFailedEventArgs
    {
        public ErrorPullToRefreshEventArgs(string message) : base(PullToRefreshFailedConstants.UnableToConnectToGitHub, message)
        {
        }
    }
}
