using AsyncAwaitBestPractices;
using Plugin.StoreReview;
using Plugin.StoreReview.Abstractions;

namespace GitTrends.UnitTests;

public class MockStoreReview : IStoreReview
{
	static readonly WeakEventManager<bool> _reviewRequestedEventManager = new();
	static readonly WeakEventManager<string> _storeListingOpenedEventManager = new();
	static readonly WeakEventManager<string> _storeReviewPageOpenedEventManager = new();

	public static event EventHandler<string> StoreListingOpened
	{
		add => _storeListingOpenedEventManager.AddEventHandler(value);
		remove => _storeListingOpenedEventManager.RemoveEventHandler(value);
	}

	public static event EventHandler<string> StoreReviewPageOpened
	{
		add => _storeReviewPageOpenedEventManager.AddEventHandler(value);
		remove => _storeReviewPageOpenedEventManager.RemoveEventHandler(value);
	}

	public static event EventHandler<bool> ReviewRequested
	{
		add => _reviewRequestedEventManager.AddEventHandler(value);
		remove => _reviewRequestedEventManager.RemoveEventHandler(value);
	}

	public void OpenStoreListing(string appId) => OnStoreListingOpened(appId);

	public void OpenStoreReviewPage(string appId) => OnStoreReviewPageOpened(appId);

	public Task<ReviewStatus> RequestReview(bool testMode)
	{
		OnReviewRequested(testMode);
		return Task.FromResult(ReviewStatus.Succeeded);
	}

	void OnStoreListingOpened(in string appId) => _storeListingOpenedEventManager.RaiseEvent(this, appId, nameof(StoreListingOpened));
	void OnStoreReviewPageOpened(in string appId) => _storeReviewPageOpenedEventManager.RaiseEvent(this, appId, nameof(StoreReviewPageOpened));
	void OnReviewRequested(in bool testMode) => _reviewRequestedEventManager.RaiseEvent(this, testMode, nameof(ReviewRequested));
}