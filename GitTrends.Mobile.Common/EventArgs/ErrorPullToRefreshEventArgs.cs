using GitTrends.Mobile.Common.Constants;

namespace GitTrends.Mobile.Common;

public class ErrorPullToRefreshEventArgs(string message) : PullToRefreshFailedEventArgs(PullToRefreshFailedConstants.UnableToConnectToGitHub, message)
{
}