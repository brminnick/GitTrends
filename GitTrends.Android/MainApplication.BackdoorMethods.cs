#if !AppStore
using Android.Runtime;
using Autofac;
using GitTrends.Mobile.Shared;
using Java.Interop;

namespace GitTrends.Droid
{
    public partial class MainApplication
    {
        UITestBackdoorService? _uiTestBackdoorService;
        UITestBackdoorService UITestBackdoorService => _uiTestBackdoorService ??= ContainerService.Container.BeginLifetimeScope().Resolve<UITestBackdoorService>();

        [Preserve, Export(BackdoorMethodConstants.SetGitHubUser)]
        public async void SetGitHubUser(string accessToken) =>
            await UITestBackdoorService.SetGitHubUser(accessToken.ToString()).ConfigureAwait(false);

        [Preserve, Export(BackdoorMethodConstants.TriggerPullToRefresh)]
        public async void TriggerRepositoriesPullToRefresh() =>
            await UITestBackdoorService.TriggerPullToRefresh().ConfigureAwait(false);

        [Preserve, Export(BackdoorMethodConstants.GetVisibleCollection)]
        public string GetVisibleCollection() =>
            SerializeObject(UITestBackdoorService.GetVisibleCollection());

        [Preserve, Export(BackdoorMethodConstants.GetCurrentTrendsChartOption)]
        public string GetCurrentTrendsChartOption() =>
            SerializeObject(UITestBackdoorService.GetCurrentTrendsChartOption());

        [Preserve, Export(BackdoorMethodConstants.IsTrendsSeriesVisible)]
        public bool IsTrendsSeriesVisible(string seriesLabel) =>
            UITestBackdoorService.IsTrendsSeriesVisible(seriesLabel);

        [Preserve, Export(BackdoorMethodConstants.GetCurrentOnboardingPageNumber)]
        public string GetCurrentOnboardingPageNumber() =>
            SerializeObject(UITestBackdoorService.GetCurrentOnboardingPageNumber());

        [Preserve, Export(BackdoorMethodConstants.PopPage)]
        public async void PopPage() =>
            await UITestBackdoorService.PopPage().ConfigureAwait(false);

        [Preserve, Export(BackdoorMethodConstants.ShouldSendNotifications)]
        public bool ShouldSendNotifications() => UITestBackdoorService.ShouldSendNotifications();

        [Preserve, Export(BackdoorMethodConstants.GetPreferredTheme)]
        public string GetPreferredTheme() =>
            SerializeObject(UITestBackdoorService.GetPreferredTheme());

        static string SerializeObject<T>(T value) => Newtonsoft.Json.JsonConvert.SerializeObject(value);
    }
}
#endif
