using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace GitTrends.UnitTests
{
    class ReferringSitesViewModelTests : BaseTest
    {
        [Test]
        public async Task PullToRefreshTest_UnauthenticatedUser()
        {
            //Arrange
            string emptyDataViewTitle_Initial, emptyDataViewTitle_Final;
            string emptyDataViewDescription_Initial, emptyDataViewDescription_Final;
            bool isEmptyDataViewEnabled_Initial, isEmptyDataViewEnabled_DuringRefresh, isEmptyDataViewEnabled_Final;
            IReadOnlyList<MobileReferringSiteModel> mobileReferringSites_Initial, mobileReferringSites_DuringRefresh, mobileReferringSites_Final;

            bool didPullToRefreshFailedFire = false;
            var pullToRefreshFailedTCS = new TaskCompletionSource<PullToRefreshFailedEventArgs>();

            ReferringSitesViewModel.PullToRefreshFailed += HandlePullToRefreshFailed;

            var referringSitesViewModel = ServiceCollection.ServiceProvider.GetRequiredService<ReferringSitesViewModel>();

            //Act
            emptyDataViewTitle_Initial = referringSitesViewModel.EmptyDataViewTitle;
            mobileReferringSites_Initial = referringSitesViewModel.MobileReferringSitesList;
            isEmptyDataViewEnabled_Initial = referringSitesViewModel.IsEmptyDataViewEnabled;
            emptyDataViewDescription_Initial = referringSitesViewModel.EmptyDataViewDescription;

            var refreshCommandTask = referringSitesViewModel.RefreshCommand.ExecuteAsync((GitTrendsRepoOwner, GitTrendsRepoName, $"https://github.com/{GitTrendsRepoOwner}/{GitTrendsRepoName}", CancellationToken.None));

            isEmptyDataViewEnabled_DuringRefresh = referringSitesViewModel.IsEmptyDataViewEnabled;
            mobileReferringSites_DuringRefresh = referringSitesViewModel.MobileReferringSitesList;

            await refreshCommandTask.ConfigureAwait(false);
            var pullToRefreshFailedEventArgs = await pullToRefreshFailedTCS.Task.ConfigureAwait(false);

            emptyDataViewTitle_Final = referringSitesViewModel.EmptyDataViewTitle;
            mobileReferringSites_Final = referringSitesViewModel.MobileReferringSitesList;
            isEmptyDataViewEnabled_Final = referringSitesViewModel.IsEmptyDataViewEnabled;
            emptyDataViewDescription_Final = referringSitesViewModel.EmptyDataViewDescription;

            //Assert
            Assert.IsTrue(didPullToRefreshFailedFire);
            Assert.IsTrue(pullToRefreshFailedEventArgs is MaximimApiRequestsReachedEventArgs || pullToRefreshFailedEventArgs is ErrorPullToRefreshEventArgs);

            Assert.IsFalse(isEmptyDataViewEnabled_Initial);
            Assert.IsFalse(isEmptyDataViewEnabled_DuringRefresh);
            Assert.IsTrue(isEmptyDataViewEnabled_Final);

            Assert.AreEqual(EmptyDataViewService.GetReferringSitesTitleText(RefreshState.Uninitialized), emptyDataViewTitle_Initial);
            Assert.AreEqual(EmptyDataViewService.GetReferringSitesTitleText(RefreshState.Error), emptyDataViewTitle_Final);

            Assert.AreEqual(EmptyDataViewService.GetReferringSitesDescriptionText(RefreshState.Uninitialized), emptyDataViewDescription_Initial);
            Assert.AreEqual(EmptyDataViewService.GetReferringSitesDescriptionText(RefreshState.Error), emptyDataViewDescription_Final);

            Assert.IsEmpty(mobileReferringSites_Initial);
            Assert.IsEmpty(mobileReferringSites_DuringRefresh);
            Assert.IsEmpty(mobileReferringSites_Final);


            void HandlePullToRefreshFailed(object? sender, PullToRefreshFailedEventArgs e)
            {
                ReferringSitesViewModel.PullToRefreshFailed -= HandlePullToRefreshFailed;

                didPullToRefreshFailedFire = true;
                pullToRefreshFailedTCS.SetResult(e);
            }
        }

        [Test]
        public async Task PullToRefreshTest_AuthenticatedUser()
        {
            //Arrange
            string emptyDataViewTitle_Initial, emptyDataViewTitle_Final;
            string emptyDataViewDescription_Initial, emptyDataViewDescription_Final;
            DateTimeOffset currentTime_Initial, currentTime_Final;
            bool isEmptyDataViewEnabled_Initial, isEmptyDataViewEnabled_DuringRefresh, isEmptyDataViewEnabled_Final;
            IReadOnlyList<MobileReferringSiteModel> mobileReferringSites_Initial, mobileReferringSites_DuringRefresh, mobileReferringSites_Final;

            var referringSitesViewModel = ServiceCollection.ServiceProvider.GetRequiredService<ReferringSitesViewModel>();
            var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
            var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();

            //Act
            await AuthenticateUser(gitHubUserService, gitHubGraphQLApiService).ConfigureAwait(false);

            currentTime_Initial = DateTimeOffset.UtcNow;
            emptyDataViewTitle_Initial = referringSitesViewModel.EmptyDataViewTitle;
            isEmptyDataViewEnabled_Initial = referringSitesViewModel.IsEmptyDataViewEnabled;
            mobileReferringSites_Initial = referringSitesViewModel.MobileReferringSitesList;
            emptyDataViewDescription_Initial = referringSitesViewModel.EmptyDataViewDescription;

            var refreshCommandTask = referringSitesViewModel.RefreshCommand.ExecuteAsync((GitTrendsRepoOwner, GitTrendsRepoName, $"https://github.com/{GitTrendsRepoOwner}/{GitTrendsRepoName}", CancellationToken.None));

            isEmptyDataViewEnabled_DuringRefresh = referringSitesViewModel.IsEmptyDataViewEnabled;
            mobileReferringSites_DuringRefresh = referringSitesViewModel.MobileReferringSitesList;

            await refreshCommandTask.ConfigureAwait(false);

            emptyDataViewTitle_Final = referringSitesViewModel.EmptyDataViewTitle;
            isEmptyDataViewEnabled_Final = referringSitesViewModel.IsEmptyDataViewEnabled;
            mobileReferringSites_Final = referringSitesViewModel.MobileReferringSitesList;
            emptyDataViewDescription_Final = referringSitesViewModel.EmptyDataViewDescription;

            currentTime_Final = DateTimeOffset.UtcNow;

            //Asset
            Assert.IsFalse(isEmptyDataViewEnabled_Initial);
            Assert.IsFalse(isEmptyDataViewEnabled_DuringRefresh);
            Assert.True(isEmptyDataViewEnabled_Final);

            Assert.IsEmpty(mobileReferringSites_Initial);
            Assert.IsEmpty(mobileReferringSites_DuringRefresh);
            Assert.IsNotEmpty(mobileReferringSites_Final);

            Assert.AreEqual(EmptyDataViewService.GetReferringSitesTitleText(RefreshState.Succeeded), emptyDataViewTitle_Final);
            Assert.AreEqual(EmptyDataViewService.GetReferringSitesTitleText(RefreshState.Uninitialized), emptyDataViewTitle_Initial);

            Assert.AreEqual(EmptyDataViewService.GetReferringSitesDescriptionText(RefreshState.Succeeded), emptyDataViewDescription_Final);
            Assert.AreEqual(EmptyDataViewService.GetReferringSitesDescriptionText(RefreshState.Uninitialized), emptyDataViewDescription_Initial);

            foreach (var referringSite in mobileReferringSites_Final)
            {
                Assert.Less(currentTime_Initial, referringSite.DownloadedAt);
                Assert.Greater(currentTime_Final, referringSite.DownloadedAt);
            }
        }

        [Test]
        public void StoreRatingRequestButtonCommandTest_YesYes()
        {
            //Arrange
            bool isStoreRatingRequestVisible_Initial, isStoreRatingRequestVisible_DuringUserInput;
            string reviewRequestView_TitleLabel_Initial, reviewRequestView_TitleLabel_AfterFirstYesButtonCommand, reviewRequestView_TitleLabel_AfterSecondYesButtonCommand;
            string reviewRequestView_NoButtonText_Initial, reviewRequestView_NoButtonText_AfterFirstYesButtonCommand, reviewRequestView_NoButtonText_AfterSecondYesButtonCommand;
            string reviewRequestView_YesButtonText_Initial, reviewRequestView_YesButtonText_AfterFirstYesButtonCommand, reviewRequestView_YesButtonText_AfterSecondYesButtonCommand;
            ReviewState reviewState_Initial, reviewState_AfterFirstYesButtonCommand, reviewState_AfterSecondYesButtonCommand;

            var reviewService = ServiceCollection.ServiceProvider.GetRequiredService<ReviewService>();
            var referringSitesViewModel = ServiceCollection.ServiceProvider.GetRequiredService<ReferringSitesViewModel>();

            //Act
            isStoreRatingRequestVisible_Initial = referringSitesViewModel.IsStoreRatingRequestVisible;
            referringSitesViewModel.IsStoreRatingRequestVisible = true;

            reviewState_Initial = reviewService.CurrentState;
            reviewRequestView_TitleLabel_Initial = referringSitesViewModel.ReviewRequestView_TitleLabel;
            reviewRequestView_NoButtonText_Initial = referringSitesViewModel.ReviewRequestView_NoButtonText;
            reviewRequestView_YesButtonText_Initial = referringSitesViewModel.ReviewRequestView_YesButtonText;
            isStoreRatingRequestVisible_DuringUserInput = referringSitesViewModel.IsStoreRatingRequestVisible;

            referringSitesViewModel.YesButtonCommand.Execute(null);

            reviewState_AfterFirstYesButtonCommand = reviewService.CurrentState;
            reviewRequestView_TitleLabel_AfterFirstYesButtonCommand = referringSitesViewModel.ReviewRequestView_TitleLabel;
            reviewRequestView_NoButtonText_AfterFirstYesButtonCommand = referringSitesViewModel.ReviewRequestView_NoButtonText;
            reviewRequestView_YesButtonText_AfterFirstYesButtonCommand = referringSitesViewModel.ReviewRequestView_YesButtonText;
            isStoreRatingRequestVisible_DuringUserInput &= referringSitesViewModel.IsStoreRatingRequestVisible;

            referringSitesViewModel.YesButtonCommand.Execute(null);

            reviewState_AfterSecondYesButtonCommand = reviewService.CurrentState;
            reviewRequestView_TitleLabel_AfterSecondYesButtonCommand = referringSitesViewModel.ReviewRequestView_TitleLabel;
            reviewRequestView_NoButtonText_AfterSecondYesButtonCommand = referringSitesViewModel.ReviewRequestView_NoButtonText;
            reviewRequestView_YesButtonText_AfterSecondYesButtonCommand = referringSitesViewModel.ReviewRequestView_YesButtonText;

            //Assert
            Assert.IsFalse(isStoreRatingRequestVisible_Initial);
            Assert.IsTrue(isStoreRatingRequestVisible_DuringUserInput);

            Assert.AreEqual(ReviewState.Greeting, reviewState_Initial);
            Assert.AreEqual(ReviewState.RequestReview, reviewState_AfterFirstYesButtonCommand);
            Assert.AreEqual(reviewState_Initial, reviewState_AfterSecondYesButtonCommand);

            Assert.AreEqual(ReviewServiceConstants.TitleLabel_EnjoyingGitTrends, reviewRequestView_TitleLabel_Initial);
            Assert.AreEqual(AppStoreConstants.RatingRequest, reviewRequestView_TitleLabel_AfterFirstYesButtonCommand);
            Assert.AreEqual(reviewRequestView_TitleLabel_Initial, reviewRequestView_TitleLabel_AfterSecondYesButtonCommand);

            Assert.AreEqual(ReviewServiceConstants.NoButton_NotReally, reviewRequestView_NoButtonText_Initial);
            Assert.AreEqual(ReviewServiceConstants.NoButton_NoThanks, reviewRequestView_NoButtonText_AfterFirstYesButtonCommand);
            Assert.AreEqual(reviewRequestView_NoButtonText_Initial, reviewRequestView_NoButtonText_AfterSecondYesButtonCommand);

            Assert.AreEqual(ReviewServiceConstants.YesButton_Yes, reviewRequestView_YesButtonText_Initial);
            Assert.AreEqual(ReviewServiceConstants.YesButton_OkSure, reviewRequestView_YesButtonText_AfterFirstYesButtonCommand);
            Assert.AreEqual(reviewRequestView_YesButtonText_Initial, reviewRequestView_YesButtonText_AfterSecondYesButtonCommand);
        }

        [Test]
        public void StoreRatingRequestButtonCommandTest_YesNo()
        {
            //Arrange
            string reviewRequestView_TitleLabel_Initial, reviewRequestView_TitleLabel_AfterNoButtonCommand, reviewRequestView_TitleLabel_AfterYesButtonCommand;
            string reviewRequestView_NoButtonText_Initial, reviewRequestView_NoButtonText_AfterNoButtonCommand, reviewRequestView_NoButtonText_AfterYesButtonCommand;
            string reviewRequestView_YesButtonText_Initial, reviewRequestView_YesButtonText_AfterNoButtonCommand, reviewRequestView_YesButtonText_AfterYesButtonCommand;
            ReviewState reviewState_Initial, reviewState_AfterNoButtonCommand, reviewState_AfterYesButtonCommand;

            var referringSitesViewModel = ServiceCollection.ServiceProvider.GetRequiredService<ReferringSitesViewModel>();
            var reviewService = ServiceCollection.ServiceProvider.GetRequiredService<ReviewService>();

            //Act
            reviewState_Initial = reviewService.CurrentState;
            reviewRequestView_TitleLabel_Initial = referringSitesViewModel.ReviewRequestView_TitleLabel;
            reviewRequestView_NoButtonText_Initial = referringSitesViewModel.ReviewRequestView_NoButtonText;
            reviewRequestView_YesButtonText_Initial = referringSitesViewModel.ReviewRequestView_YesButtonText;

            referringSitesViewModel.YesButtonCommand.Execute(null);

            reviewState_AfterYesButtonCommand = reviewService.CurrentState;
            reviewRequestView_TitleLabel_AfterYesButtonCommand = referringSitesViewModel.ReviewRequestView_TitleLabel;
            reviewRequestView_NoButtonText_AfterYesButtonCommand = referringSitesViewModel.ReviewRequestView_NoButtonText;
            reviewRequestView_YesButtonText_AfterYesButtonCommand = referringSitesViewModel.ReviewRequestView_YesButtonText;

            referringSitesViewModel.NoButtonCommand.Execute(null);

            reviewState_AfterNoButtonCommand = reviewService.CurrentState;
            reviewRequestView_TitleLabel_AfterNoButtonCommand = referringSitesViewModel.ReviewRequestView_TitleLabel;
            reviewRequestView_NoButtonText_AfterNoButtonCommand = referringSitesViewModel.ReviewRequestView_NoButtonText;
            reviewRequestView_YesButtonText_AfterNoButtonCommand = referringSitesViewModel.ReviewRequestView_YesButtonText;

            //Assert
            Assert.AreEqual(ReviewState.Greeting, reviewState_Initial);
            Assert.AreEqual(ReviewState.RequestReview, reviewState_AfterYesButtonCommand);
            Assert.AreEqual(reviewState_Initial, reviewState_AfterNoButtonCommand);

            Assert.AreEqual(ReviewServiceConstants.TitleLabel_EnjoyingGitTrends, reviewRequestView_TitleLabel_Initial);
            Assert.AreEqual(AppStoreConstants.RatingRequest, reviewRequestView_TitleLabel_AfterYesButtonCommand);
            Assert.AreEqual(reviewRequestView_TitleLabel_Initial, reviewRequestView_TitleLabel_AfterNoButtonCommand);

            Assert.AreEqual(ReviewServiceConstants.NoButton_NotReally, reviewRequestView_NoButtonText_Initial);
            Assert.AreEqual(ReviewServiceConstants.NoButton_NoThanks, reviewRequestView_NoButtonText_AfterYesButtonCommand);
            Assert.AreEqual(reviewRequestView_NoButtonText_Initial, reviewRequestView_NoButtonText_AfterNoButtonCommand);

            Assert.AreEqual(ReviewServiceConstants.YesButton_Yes, reviewRequestView_YesButtonText_Initial);
            Assert.AreEqual(ReviewServiceConstants.YesButton_OkSure, reviewRequestView_YesButtonText_AfterYesButtonCommand);
            Assert.AreEqual(reviewRequestView_YesButtonText_Initial, reviewRequestView_YesButtonText_AfterNoButtonCommand);
        }

        [Test]
        public void StoreRatingRequestButtonCommandTest_NoYes()
        {
            //Arrange
            string reviewRequestView_TitleLabel_Initial, reviewRequestView_TitleLabel_AfterFirstNoButtonCommand, reviewRequestView_TitleLabel_AfterYesButtonCommand;
            string reviewRequestView_NoButtonText_Initial, reviewRequestView_NoButtonText_AfterFirstNoButtonCommand, reviewRequestView_NoButtonText_AfterYesButtonCommand;
            string reviewRequestView_YesButtonText_Initial, reviewRequestView_YesButtonText_AfterFirstNoButtonCommand, reviewRequestView_YesButtonText_AfterYesButtonCommand;
            ReviewState reviewState_Initial, reviewState_AfterFirstNoButtonCommand, reviewState_AfterYesButtonCommand;

            var referringSitesViewModel = ServiceCollection.ServiceProvider.GetRequiredService<ReferringSitesViewModel>();
            var reviewService = ServiceCollection.ServiceProvider.GetRequiredService<ReviewService>();

            //Act
            reviewState_Initial = reviewService.CurrentState;
            reviewRequestView_TitleLabel_Initial = referringSitesViewModel.ReviewRequestView_TitleLabel;
            reviewRequestView_NoButtonText_Initial = referringSitesViewModel.ReviewRequestView_NoButtonText;
            reviewRequestView_YesButtonText_Initial = referringSitesViewModel.ReviewRequestView_YesButtonText;

            referringSitesViewModel.NoButtonCommand.Execute(null);

            reviewState_AfterFirstNoButtonCommand = reviewService.CurrentState;
            reviewRequestView_TitleLabel_AfterFirstNoButtonCommand = referringSitesViewModel.ReviewRequestView_TitleLabel;
            reviewRequestView_NoButtonText_AfterFirstNoButtonCommand = referringSitesViewModel.ReviewRequestView_NoButtonText;
            reviewRequestView_YesButtonText_AfterFirstNoButtonCommand = referringSitesViewModel.ReviewRequestView_YesButtonText;

            referringSitesViewModel.YesButtonCommand.Execute(null);

            reviewState_AfterYesButtonCommand = reviewService.CurrentState;
            reviewRequestView_TitleLabel_AfterYesButtonCommand = referringSitesViewModel.ReviewRequestView_TitleLabel;
            reviewRequestView_NoButtonText_AfterYesButtonCommand = referringSitesViewModel.ReviewRequestView_NoButtonText;
            reviewRequestView_YesButtonText_AfterYesButtonCommand = referringSitesViewModel.ReviewRequestView_YesButtonText;

            //Assert
            Assert.AreEqual(ReviewState.Greeting, reviewState_Initial);
            Assert.AreEqual(ReviewState.RequestFeedback, reviewState_AfterFirstNoButtonCommand);
            Assert.AreEqual(reviewState_Initial, reviewState_AfterYesButtonCommand);

            Assert.AreEqual(ReviewServiceConstants.TitleLabel_EnjoyingGitTrends, reviewRequestView_TitleLabel_Initial);
            Assert.AreEqual(ReviewServiceConstants.TitleLabel_Feedback, reviewRequestView_TitleLabel_AfterFirstNoButtonCommand);
            Assert.AreEqual(reviewRequestView_TitleLabel_Initial, reviewRequestView_TitleLabel_AfterYesButtonCommand);

            Assert.AreEqual(ReviewServiceConstants.NoButton_NotReally, reviewRequestView_NoButtonText_Initial);
            Assert.AreEqual(ReviewServiceConstants.NoButton_NoThanks, reviewRequestView_NoButtonText_AfterFirstNoButtonCommand);
            Assert.AreEqual(reviewRequestView_NoButtonText_Initial, reviewRequestView_NoButtonText_AfterYesButtonCommand);

            Assert.AreEqual(ReviewServiceConstants.YesButton_Yes, reviewRequestView_YesButtonText_Initial);
            Assert.AreEqual(ReviewServiceConstants.YesButton_OkSure, reviewRequestView_YesButtonText_AfterFirstNoButtonCommand);
            Assert.AreEqual(reviewRequestView_YesButtonText_Initial, reviewRequestView_YesButtonText_AfterYesButtonCommand);
        }

        [Test]
        public void StoreRatingRequestButtonCommandTest_NoNo()
        {
            //Arrange
            string reviewRequestView_TitleLabel_Initial, reviewRequestView_TitleLabel_AfterFirstNoButtonCommand, reviewRequestView_TitleLabel_AfterSecondNoButtonCommand;
            string reviewRequestView_NoButtonText_Initial, reviewRequestView_NoButtonText_AfterFirstNoButtonCommand, reviewRequestView_NoButtonText_AfterSecondNoButtonCommand;
            string reviewRequestView_YesButtonText_Initial, reviewRequestView_YesButtonText_AfterFirstNoButtonCommand, reviewRequestView_YesButtonText_AfterSecondNoButtonCommand;
            ReviewState reviewState_Initial, reviewState_AfterFirstNoButtonCommand, reviewState_AfterSecondNoButtonCommand;

            var referringSitesViewModel = ServiceCollection.ServiceProvider.GetRequiredService<ReferringSitesViewModel>();
            var reviewService = ServiceCollection.ServiceProvider.GetRequiredService<ReviewService>();

            //Act
            reviewState_Initial = reviewService.CurrentState;
            reviewRequestView_TitleLabel_Initial = referringSitesViewModel.ReviewRequestView_TitleLabel;
            reviewRequestView_NoButtonText_Initial = referringSitesViewModel.ReviewRequestView_NoButtonText;
            reviewRequestView_YesButtonText_Initial = referringSitesViewModel.ReviewRequestView_YesButtonText;

            referringSitesViewModel.NoButtonCommand.Execute(null);

            reviewState_AfterFirstNoButtonCommand = reviewService.CurrentState;
            reviewRequestView_TitleLabel_AfterFirstNoButtonCommand = referringSitesViewModel.ReviewRequestView_TitleLabel;
            reviewRequestView_NoButtonText_AfterFirstNoButtonCommand = referringSitesViewModel.ReviewRequestView_NoButtonText;
            reviewRequestView_YesButtonText_AfterFirstNoButtonCommand = referringSitesViewModel.ReviewRequestView_YesButtonText;

            referringSitesViewModel.NoButtonCommand.Execute(null);

            reviewState_AfterSecondNoButtonCommand = reviewService.CurrentState;
            reviewRequestView_TitleLabel_AfterSecondNoButtonCommand = referringSitesViewModel.ReviewRequestView_TitleLabel;
            reviewRequestView_NoButtonText_AfterSecondNoButtonCommand = referringSitesViewModel.ReviewRequestView_NoButtonText;
            reviewRequestView_YesButtonText_AfterSecondNoButtonCommand = referringSitesViewModel.ReviewRequestView_YesButtonText;

            //Assert
            Assert.AreEqual(ReviewState.Greeting, reviewState_Initial);
            Assert.AreEqual(ReviewState.RequestFeedback, reviewState_AfterFirstNoButtonCommand);
            Assert.AreEqual(reviewState_Initial, reviewState_AfterSecondNoButtonCommand);

            Assert.AreEqual(ReviewServiceConstants.TitleLabel_EnjoyingGitTrends, reviewRequestView_TitleLabel_Initial);
            Assert.AreEqual(ReviewServiceConstants.TitleLabel_Feedback, reviewRequestView_TitleLabel_AfterFirstNoButtonCommand);
            Assert.AreEqual(reviewRequestView_TitleLabel_Initial, reviewRequestView_TitleLabel_AfterSecondNoButtonCommand);

            Assert.AreEqual(ReviewServiceConstants.NoButton_NotReally, reviewRequestView_NoButtonText_Initial);
            Assert.AreEqual(ReviewServiceConstants.NoButton_NoThanks, reviewRequestView_NoButtonText_AfterFirstNoButtonCommand);
            Assert.AreEqual(reviewRequestView_NoButtonText_Initial, reviewRequestView_NoButtonText_AfterSecondNoButtonCommand);

            Assert.AreEqual(ReviewServiceConstants.YesButton_Yes, reviewRequestView_YesButtonText_Initial);
            Assert.AreEqual(ReviewServiceConstants.YesButton_OkSure, reviewRequestView_YesButtonText_AfterFirstNoButtonCommand);
            Assert.AreEqual(reviewRequestView_YesButtonText_Initial, reviewRequestView_YesButtonText_AfterSecondNoButtonCommand);
        }
    }
}
