using System;
using GitTrends.Mobile.Shared;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace GitTrends.UnitTests
{
    class ReferringSitesViewModelTests : BaseTest
    {
        //public event EventHandler<PullToRefreshFailedEventArgs> PullToRefreshFailed

        //public ICommand YesButtonCommand { get; }
        //public ICommand RefreshCommand { get; }

        //public string EmptyDataViewDescription
        //public bool IsEmptyDataViewEnabled
        //public bool IsStoreRatingRequestVisible
        //public bool IsRefreshing
        //public IReadOnlyList<MobileReferringSiteModel> MobileReferringSitesList

        [Test]
        public void NoButtonCommandTest()
        {
            //Arrange
            string reviewRequestView_TitleLabel_Initial, reviewRequestView_TitleLabel_AfterFirstNoButtonCommand, reviewRequestView_TitleLabel_AfterSecondNoButtonCommand;
            string reviewRequestView_NoButtonText_Initial, reviewRequestView_NoButtonText_AfterFirstNoButtonCommand, reviewRequestView_NoButtonText_AfterSecondNoButtonCommand;
            string reviewRequestView_YesButtonText_Initial, reviewRequestView_YesButtonText_AfterFirstNoButtonCommand, reviewRequestView_YesButtonText_AfterSecondNoButtonCommand;
            ReviewState reviewState_Initial, reviewState_AfterFirstNoButtonCommand, reviewState_AfterSecondNoButtonCommand;

            var referringSitesViewModel = ServiceCollection.ServiceProvider.GetService<ReferringSitesViewModel>();
            var reviewService = ServiceCollection.ServiceProvider.GetService<ReviewService>();

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
