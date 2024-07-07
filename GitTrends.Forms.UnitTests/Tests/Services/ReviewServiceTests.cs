using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Xamarin.Essentials.Interfaces;

namespace GitTrends.UnitTests
{
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
				Assert.IsFalse(didReviewRequestedFire_ReviewService);
			}

			await reviewService.TryRequestReviewPrompt().ConfigureAwait(false);
			await reviewRequestedTCS_ReviewService.Task.ConfigureAwait(false);
			var storeReviewResult = await reviewRequestedTCS_StoreReview.Task.ConfigureAwait(false);

			//Assert
			Assert.IsTrue(didReviewRequestedFire_ReviewService);
			Assert.IsTrue(didReviewRequestedFire_StoreReview);
			Assert.IsTrue(storeReviewResult);
			Assert.AreNotEqual(preferences.Get("MostRecentRequestDate", default(DateTime)), default);
			Assert.AreNotEqual(preferences.Get("MostRecentReviewedBuildString", default(string)), default);

			void HandleReviewRequested_ReviewService(object? sender, EventArgs e)
			{
				ReviewService.ReviewRequested -= HandleReviewRequested_ReviewService;

				didReviewRequestedFire_ReviewService = true;
				reviewRequestedTCS_ReviewService.SetResult(null);
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
				Assert.IsFalse(didReviewRequestedFire);
			}

			//Assert
			Assert.IsFalse(didReviewRequestedFire);
			Assert.AreEqual(preferences.Get("MostRecentRequestDate", default(DateTime)), default(DateTime));
			Assert.AreEqual(preferences.Get("MostRecentReviewedBuildString", default(string)), default(string));

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
			Assert.IsFalse(didReviewRequestedFire);
			Assert.AreNotEqual(preferences.Get("MostRecentRequestDate", default(DateTime)), default);
			Assert.AreNotEqual(preferences.Get("MostRecentReviewedBuildString", default(string)), default);

			ReviewService.ReviewRequested -= HandleReviewRequested;

			void HandleReviewRequested(object? sender, EventArgs e) => didReviewRequestedFire = true;
		}
	}
}