#if !AppStore
namespace GitTrends.Mobile.Shared
{
    class BackdoorMethodConstants
    {
        public const string SetGitHubUser = nameof(SetGitHubUser);
        public const string TriggerPullToRefresh = nameof(TriggerPullToRefresh);
        public const string GetVisibleRepositoryList = nameof(GetVisibleRepositoryList);
        public const string GetVisibleReferringSitesList = nameof(GetVisibleReferringSitesList);
    }
}
#endif
