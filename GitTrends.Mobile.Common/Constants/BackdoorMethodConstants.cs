#if !AppStore
namespace GitTrends.Mobile.Common;

public class BackdoorMethodConstants
{
	public const string AreNotificationsEnabled = nameof(AreNotificationsEnabled);
	public const string GetContributorsCollection = nameof(GetContributorsCollection);
	public const string GetCurrentOnboardingPageNumber = nameof(GetCurrentOnboardingPageNumber);
	public const string GetCurrentTrendsChartOption = nameof(GetCurrentTrendsChartOption);
	public const string GetCurrentTrendsPageNumber = nameof(GetCurrentTrendsPageNumber);
	public const string GetGitHubToken = nameof(GetGitHubToken);
	public const string GetLibrariesCollection = nameof(GetLibrariesCollection);
	public const string GetLoggedInUserAlias = nameof(GetLoggedInUserAlias);
	public const string GetLoggedInUserAvatarUrl = nameof(GetLoggedInUserAvatarUrl);
	public const string GetLoggedInUserName = nameof(GetLoggedInUserName);
	public const string GetPreferredLanguage = nameof(GetPreferredLanguage);
	public const string GetReviewRequestAppStoreTitle = nameof(GetReviewRequestAppStoreTitle);
	public const string GetPreferredTheme = nameof(GetPreferredTheme);
	public const string GetVisibleCollection = nameof(GetVisibleCollection);
	public const string IsViewsClonesChartSeriesVisible = nameof(IsViewsClonesChartSeriesVisible);
	public const string PopPage = nameof(PopPage);
	public const string SetGitHubUser = nameof(SetGitHubUser);
	public const string ShouldSendNotifications = nameof(ShouldSendNotifications);
	public const string TriggerPullToRefresh = nameof(TriggerPullToRefresh);
	public const string TriggerReviewRequest = nameof(TriggerReviewRequest);
}
#endif