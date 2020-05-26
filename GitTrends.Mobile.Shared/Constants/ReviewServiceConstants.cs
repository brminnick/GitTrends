namespace GitTrends.Mobile.Shared
{
    public static class ReviewServiceConstants
    {
        public const string TitleLabel_EnjoyingGitTrends = "Enjoying GitTrends?";
        public const string TitleLabel_EnjoyingNewVersionOfGitTrends = "Enjoing the New Version of GitTrends?";
        public const string TitleLabel_Feedback = "Would you mind giving us some feedback?";

        public const string NoButton_NoThanks = "No Thanks";
        public const string NoButton_NotReally = "Not Really";

        public const string YesButton_OkSure = "Ok, Sure!";
        public const string YesButton_Yes = "Yes!";
    }

    public enum ReviewState { Greeting, RequestFeedback, RequestReview }
    public enum ReviewAction { NoButtonTapped, YesButtonTapped }
    public enum ReviewRequest { None, AppStore, Email }
}
