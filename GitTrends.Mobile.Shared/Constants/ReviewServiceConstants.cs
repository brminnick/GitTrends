using System;
using Xamarin.Essentials;

namespace GitTrends.Mobile.Shared
{
    static class ReviewServiceConstants
    {
        public const string TitleLabel_EnjoyingGitTrends = "Enjoying GitTrends?";
        public const string TitleLabel_EnjoyingNewVersionOfGitTrends = "Enjoing the New Version of GitTrends?";
        public const string TitleLabel_Feedback = "Would you mind giving us some feedback?";

        public const string NoButton_NoThanks = "No Thanks";
        public const string NoButton_NotReally = "Not Really";

        public const string YesButton_OkSure = "Ok, Sure!";
        public const string YesButton_Yes = "Yes!";
    }

    enum ReviewState { Greeting, RequestFeedback, RequestReview }
    enum ReviewAction { NoButtonTapped, YesButtonTapped }
    enum ReviewRequest { None, AppStore, Email }
}
