#if !AppStore
namespace GitTrends.Mobile.Shared
{
    class BackdoorMethodConstants
    {
        public const string SetGitHubUser = nameof(SetGitHubUser);
        public const string TriggerPullToRefresh = nameof(TriggerPullToRefresh);
        public const string GetVisibleCollection = nameof(GetVisibleCollection);
        public const string GetCurrentTrendsChartOption = nameof(GetCurrentTrendsChartOption);
    }
}
#endif
