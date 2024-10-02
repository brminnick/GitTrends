namespace GitTrends;

public class InitializationCompleteEventArgs(bool isInitializationSuccessful) : EventArgs
{
	public bool IsInitializationSuccessful { get; } = isInitializationSuccessful;
}