namespace GitTrends.UnitTests;

class ReviewServiceTests : BaseTest
{
	[Test]
	public async Task TryRequestReviewPromptTest_Valid()
	{
		//Arrange
		bool didReviewRequestedFire_ReviewService = false;
		bool didReviewRequestedFire_StoreReview = false;

		var reviewRequestedTCS_ReviewService = new TaskCompletionSource();
		var reviewRequestedTCS_StoreReview = new TaskCompletionSource<bool>();

		MockStoreReview.ReviewRequested += HandleReviewRequested_StoreReview;
		ReviewService.ReviewRequested += HandleReviewRequested_ReviewService;

		var reviewService = ServiceCollection.ServiceProvider.GetRequiredService<ReviewService>();

		var preferences = ServiceCollection.ServiceProvider.GetRequiredService<IPreferences>();
		preferences.Set("AppInstallDate", DateTime.UtcNow.Subtract(TimeSpan.FromDays(ReviewService.MinimumAppInstallDays)));

		//Act
		for (int i = 0; i < ReviewService.MinimumReviewRequests; i++)
		{
			await reviewService.TryRequestReviewPrompt().ConfigureAwait(false);
			Assert.That(didReviewRequestedFire_ReviewService, Is.False);
		}

		await reviewService.TryRequestReviewPrompt().ConfigureAwait(false);
		await reviewRequestedTCS_ReviewService.Task.ConfigureAwait(false);
		var storeReviewResult = await reviewRequestedTCS_StoreReview.Task.ConfigureAwait(false);

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(didReviewRequestedFire_ReviewService);
			Assert.That(didReviewRequestedFire_StoreReview);
			Assert.That(storeReviewResult);
			Assert.That(preferences.Get("MostRecentRequestDate", default(DateTime)), Is.Not.EqualTo(default(DateTime)));
			Assert.That(preferences.Get("MostRecentReviewedBuildString", default(string)), Is.Not.EqualTo(null));
		});

		void HandleReviewRequested_ReviewService(object? sender, EventArgs e)
		{
			ReviewService.ReviewRequested -= HandleReviewRequested_ReviewService;

			didReviewRequestedFire_ReviewService = true;
			reviewRequestedTCS_ReviewService.SetResult();
		}

		void HandleReviewRequested_StoreReview(object? sender, bool e)
		{
			MockStoreReview.ReviewRequested -= HandleReviewRequested_StoreReview;

			didReviewRequestedFire_StoreReview = true;
			reviewRequestedTCS_StoreReview.SetResult(e);
		}
	}

	[Test]
	public async Task TryRequestReviewPromptTest_InvalidAppInstallDays()
	{
		//Arrange
		bool didReviewRequestedFire = false;
		ReviewService.ReviewRequested += HandleReviewRequested;

		var preferences = ServiceCollection.ServiceProvider.GetRequiredService<IPreferences>();
		var reviewService = ServiceCollection.ServiceProvider.GetRequiredService<ReviewService>();


		//Act
		for (int i = 0; i < ReviewService.MinimumReviewRequests; i++)
		{
			await reviewService.TryRequestReviewPrompt().ConfigureAwait(false);
			Assert.That(didReviewRequestedFire, Is.False);
		}

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(didReviewRequestedFire, Is.False);
			Assert.That(preferences.Get("MostRecentRequestDate", default(DateTime)), Is.EqualTo(default(DateTime)));
			Assert.That(preferences.Get("MostRecentReviewedBuildString", default(string)), Is.EqualTo(default(string)));
		});

		ReviewService.ReviewRequested -= HandleReviewRequested;

		void HandleReviewRequested(object? sender, EventArgs e) => didReviewRequestedFire = true;
	}

	[Test]
	public async Task TryRequestReviewPromptTest_InvalidMostRecentRequestDate()
	{
		//Arrange
		await TryRequestReviewPromptTest_Valid().ConfigureAwait(false);

		bool didReviewRequestedFire = false;
		ReviewService.ReviewRequested += HandleReviewRequested;

		var preferences = ServiceCollection.ServiceProvider.GetRequiredService<IPreferences>();
		var reviewService = ServiceCollection.ServiceProvider.GetRequiredService<ReviewService>();

		//Act
		await reviewService.TryRequestReviewPrompt().ConfigureAwait(false);

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(didReviewRequestedFire, Is.False);
			Assert.That(preferences.Get("MostRecentRequestDate", default(DateTime)), Is.Not.EqualTo(default(DateTime)));
			Assert.That(preferences.Get("MostRecentReviewedBuildString", default(string)), Is.Not.EqualTo(default(string)));
		});

		ReviewService.ReviewRequested -= HandleReviewRequested;

		void HandleReviewRequested(object? sender, EventArgs e) => didReviewRequestedFire = true;
	}
}