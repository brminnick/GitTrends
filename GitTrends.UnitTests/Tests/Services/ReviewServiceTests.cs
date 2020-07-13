using System;
using System.Threading.Tasks;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Xamarin.Essentials.Interfaces;

namespace GitTrends.UnitTests
{
    class ReviewServiceTests : BaseTest
    {
        [TestCase(ReviewAction.NoButtonTapped, ReviewAction.NoButtonTapped, ReviewRequest.None)]
        [TestCase(ReviewAction.NoButtonTapped, ReviewAction.YesButtonTapped, ReviewRequest.Email)]
        [TestCase(ReviewAction.YesButtonTapped, ReviewAction.NoButtonTapped, ReviewRequest.None)]
        [TestCase(ReviewAction.YesButtonTapped, ReviewAction.YesButtonTapped, ReviewRequest.AppStore)]
        public async Task UpdateStateTest(ReviewAction firstReviewAction, ReviewAction secondReviewAction, ReviewRequest expectedReviewRequest)
        {
            //Arrange
            ReviewRequest reviewRequest;
            bool didReviewCompletedFire = false;
            var reviewCompletedTCS = new TaskCompletionSource<ReviewRequest>();

            ReviewState reviewState_Initial, reviewState_AfterFirstAction, reviewState_AfterSecondAction;
            string yesButtonText_Initial, yesButtonText_AfterFirstAction, yesButtonText_AfterSecondAction;
            string noButtonText_Initial, noButtonText_AfterFirstAction, noButtonText_AfterSecondAction;
            string titleText_Initial, titleText_AfterFirstAction, titleText_AfterSecondAction;

            ReviewService.ReviewCompleted += HandleReviewCompleted;

            var reviewService = ServiceCollection.ServiceProvider.GetRequiredService<ReviewService>();

            //Act
            reviewState_Initial = reviewService.CurrentState;
            yesButtonText_Initial = reviewService.YesButtonText;
            noButtonText_Initial = reviewService.NoButtonText;
            titleText_Initial = reviewService.StoreRatingRequestViewTitle;

            reviewService.UpdateState(firstReviewAction);

            reviewState_AfterFirstAction = reviewService.CurrentState;
            yesButtonText_AfterFirstAction = reviewService.YesButtonText;
            noButtonText_AfterFirstAction = reviewService.NoButtonText;
            titleText_AfterFirstAction= reviewService.StoreRatingRequestViewTitle;

            reviewService.UpdateState(secondReviewAction);

            reviewState_AfterSecondAction = reviewService.CurrentState;
            yesButtonText_AfterSecondAction = reviewService.YesButtonText;
            noButtonText_AfterSecondAction = reviewService.NoButtonText;
            titleText_AfterSecondAction = reviewService.StoreRatingRequestViewTitle;

            reviewRequest = await reviewCompletedTCS.Task.ConfigureAwait(false);

            //Assert
            Assert.IsTrue(didReviewCompletedFire);

            Assert.AreEqual(ReviewState.Greeting, reviewState_Initial);
            Assert.AreEqual(ReviewServiceConstants.YesButton_Yes, yesButtonText_Initial);
            Assert.AreEqual(ReviewServiceConstants.NoButton_NotReally, noButtonText_Initial);
            Assert.AreEqual(ReviewServiceConstants.TitleLabel_EnjoyingGitTrends, titleText_Initial);

            if (firstReviewAction is ReviewAction.NoButtonTapped)
            {
                Assert.AreEqual(ReviewState.RequestFeedback, reviewState_AfterFirstAction);
                Assert.AreEqual(ReviewServiceConstants.YesButton_OkSure, yesButtonText_AfterFirstAction);
                Assert.AreEqual(ReviewServiceConstants.NoButton_NoThanks, noButtonText_AfterFirstAction);
                Assert.AreEqual(ReviewServiceConstants.TitleLabel_Feedback, titleText_AfterFirstAction );
            }
            else
            {
                Assert.AreEqual(ReviewState.RequestReview, reviewState_AfterFirstAction);
                Assert.AreEqual(ReviewServiceConstants.YesButton_OkSure, yesButtonText_AfterFirstAction);
                Assert.AreEqual(ReviewServiceConstants.NoButton_NoThanks, noButtonText_AfterFirstAction);
                Assert.AreEqual(AppStoreConstants.RatingRequest, titleText_AfterFirstAction);
            }

            Assert.AreEqual(ReviewState.Greeting, reviewState_AfterSecondAction);
            Assert.AreEqual(ReviewServiceConstants.YesButton_Yes, yesButtonText_AfterSecondAction);
            Assert.AreEqual(ReviewServiceConstants.NoButton_NotReally, noButtonText_AfterSecondAction);
            Assert.AreEqual(ReviewServiceConstants.TitleLabel_EnjoyingGitTrends, titleText_AfterSecondAction);

            Assert.AreEqual(expectedReviewRequest, reviewRequest);

            void HandleReviewCompleted(object? sender, ReviewRequest e)
            {
                ReviewService.ReviewCompleted -= HandleReviewCompleted;

                didReviewCompletedFire = true;
                reviewCompletedTCS.SetResult(e);
            }
        }

        [Test]
        public async Task TryRequestReviewPromptTest_Valid()
        {
            //Arrange
            bool didReviewRequestedFire = false;
            var reviewRequestedTCS = new TaskCompletionSource<object?>();

            ReviewService.ReviewRequested += HandleReviewRequested;

            var reviewService = ServiceCollection.ServiceProvider.GetRequiredService<ReviewService>();

            var preferences = ServiceCollection.ServiceProvider.GetRequiredService<IPreferences>();
            preferences.Set("AppInstallDate", DateTime.UtcNow.Subtract(TimeSpan.FromDays(ReviewService.MinimumAppInstallDays)));

            //Act
            for (int i = 0; i < ReviewService.MinimumReviewRequests; i++)
            {
                reviewService.TryRequestReviewPrompt();
                Assert.IsFalse(didReviewRequestedFire);
            }

            reviewService.TryRequestReviewPrompt();
            await reviewRequestedTCS.Task.ConfigureAwait(false);

            //Assert
            Assert.IsTrue(didReviewRequestedFire);
            Assert.AreNotEqual(preferences.Get("MostRecentRequestDate", default(DateTime)), default);
            Assert.AreNotEqual(preferences.Get("MostRecentReviewedBuildString", default(string)), default);

            void HandleReviewRequested(object? sender, EventArgs e)
            {
                ReviewService.ReviewRequested -= HandleReviewRequested;

                didReviewRequestedFire = true;
                reviewRequestedTCS.SetResult(null);
            }
        }

        [Test]
        public void TryRequestReviewPromptTest_InvalidAppInstallDays()
        {
            //Arrange
            bool didReviewRequestedFire = false;
            ReviewService.ReviewRequested += HandleReviewRequested;

            var preferences = ServiceCollection.ServiceProvider.GetRequiredService<IPreferences>();
            var reviewService = ServiceCollection.ServiceProvider.GetRequiredService<ReviewService>();


            //Act
            for (int i = 0; i < ReviewService.MinimumReviewRequests; i++)
            {
                reviewService.TryRequestReviewPrompt();
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
            reviewService.TryRequestReviewPrompt();

            //Assert
            Assert.IsFalse(didReviewRequestedFire);
            Assert.AreNotEqual(preferences.Get("MostRecentRequestDate", default(DateTime)), default);
            Assert.AreNotEqual(preferences.Get("MostRecentReviewedBuildString", default(string)), default);

            ReviewService.ReviewRequested -= HandleReviewRequested;

            void HandleReviewRequested(object? sender, EventArgs e) => didReviewRequestedFire = true;
        }
    }
}