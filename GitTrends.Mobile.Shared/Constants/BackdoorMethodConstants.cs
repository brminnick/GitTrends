#if !AppStore
namespace GitTrends.Mobile.Shared
{
    public class BackdoorMethodConstants
    {
        public const string SetGitHubUser = nameof(SetGitHubUser);
        public const string GetGitHubToken = nameof(GetGitHubToken);
        public const string TriggerPullToRefresh = nameof(TriggerPullToRefresh);
        public const string GetVisibleCollection = nameof(GetVisibleCollection);
        public const string GetCurrentTrendsChartOption = nameof(GetCurrentTrendsChartOption);
        public const string IsTrendsSeriesVisible = nameof(IsTrendsSeriesVisible);
        public const string GetCurrentOnboardingPageNumber = nameof(GetCurrentOnboardingPageNumber);
        public const string PopPage = nameof(PopPage);
        public const string ShouldSendNotifications = nameof(ShouldSendNotifications);
        public const string GetPreferredTheme = nameof(GetPreferredTheme);
        public const string TriggerReviewRequest = nameof(TriggerReviewRequest);
        public const string GetReviewRequestAppStoreTitle = nameof(GetReviewRequestAppStoreTitle);
        public const string AreNotificationsEnabled = nameof(AreNotificationsEnabled);
        public const string GetLoggedInUserAlias = nameof(GetLoggedInUserAlias);
        public const string GetLoggedInUserName = nameof(GetLoggedInUserName);
        public const string GetLoggedInUserAvatarUrl = nameof(GetLoggedInUserAvatarUrl);
    }
}
#endif
