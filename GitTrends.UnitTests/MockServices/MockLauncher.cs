namespace GitTrends.UnitTests;

class MockLauncher : ILauncher
{
	static readonly AsyncAwaitBestPractices.WeakEventManager _openAsyncExecutedEventHandler = new();

	public static event EventHandler OpenAsyncExecuted
	{
		add => _openAsyncExecutedEventHandler.AddEventHandler(value);
		remove => _openAsyncExecutedEventHandler.RemoveEventHandler(value);
	}

	public Task<bool> TryOpenAsync(Uri uri) => CanOpenAsync(uri);

	public Task<bool> CanOpenAsync(Uri uri) => Task.FromResult(true);

	public Task<bool> OpenAsync(Uri uri)
	{
		OnOpenAsyncExecuted();
		return Task.FromResult(true);
	}

	public Task<bool> OpenAsync(OpenFileRequest request)
	{
		OnOpenAsyncExecuted();
		return Task.FromResult(true);
	}

	void OnOpenAsyncExecuted() => _openAsyncExecutedEventHandler.RaiseEvent(this, EventArgs.Empty, nameof(OpenAsyncExecuted));
}