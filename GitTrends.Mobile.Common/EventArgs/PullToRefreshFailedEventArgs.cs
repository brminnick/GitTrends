namespace GitTrends.Mobile.Common;

public abstract class PullToRefreshFailedEventArgs(string title, string message, string cancel = "OK", string? accept = null) : EventArgs
{
	public string Message { get; } = message;
	public string Title { get; } = title;
	public string Cancel { get; } = cancel;
	public string? Accept { get; } = accept;
}