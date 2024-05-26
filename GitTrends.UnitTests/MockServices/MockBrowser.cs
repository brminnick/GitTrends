using AsyncAwaitBestPractices;

namespace GitTrends.UnitTests;

public class MockBrowser : IBrowser
{
	static readonly WeakEventManager<Uri> _openAsyncExecutedEventHandler = new();

	public static event EventHandler<Uri> OpenAsyncExecuted
	{
		add => _openAsyncExecutedEventHandler.AddEventHandler(value);
		remove => _openAsyncExecutedEventHandler.RemoveEventHandler(value);
	}

	public async Task<bool> OpenAsync(Uri uri, BrowserLaunchOptions options)
	{
		await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);

		OnOpenAsyncExecuted(uri);

		return true;
	}

	void OnOpenAsyncExecuted(in Uri uri) => _openAsyncExecutedEventHandler.RaiseEvent(this, uri, nameof(OpenAsyncExecuted));
}