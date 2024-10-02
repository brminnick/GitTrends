namespace GitTrends;

public class AuthorizeSessionCompletedEventArgs(bool isSessionAuthorized) : EventArgs
{
	public bool IsSessionAuthorized { get; } = isSessionAuthorized;
}