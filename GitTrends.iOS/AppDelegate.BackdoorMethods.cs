#if !AppStore
using System.Threading;
using Autofac;
using Foundation;
using GitTrends.Mobile.Common;

namespace GitTrends.iOS
{
    public partial class AppDelegate
    {
        public AppDelegate() => Xamarin.Calabash.Start();

        UITestsBackdoorService? _uiTestBackdoorService;
        UITestsBackdoorService UITestBackdoorService => _uiTestBackdoorService ??= ContainerService.Container.Resolve<UITestsBackdoorService>();

        [Preserve, Export(BackdoorMethodConstants.SetGitHubUser + ":")]
        public async void SetGitHubUser(NSString accessToken) =>
            await UITestBackdoorService.SetGitHubUser(accessToken.ToString(), CancellationToken.None).ConfigureAwait(false);

        [Preserve, Export(BackdoorMethodConstants.TriggerPullToRefresh + ":")]
        public async void TriggerRepositoriesPullToRefresh(NSString noValue) =>
            await UITestBackdoorService.TriggerPullToRefresh().ConfigureAwait(false);

        [Preserve, Export(BackdoorMethodConstants.GetVisibleCollection + ":")]
        public NSString GetVisibleRepositoryList(NSString noValue) =>
            SerializeObject(UITestBackdoorService.GetVisibleCollection());

        [Preserve, Export(BackdoorMethodConstants.GetCurrentTrendsChartOption + ":")]
        public NSString GetCurrentTrendsChartOption(NSString noValue) =>
            SerializeObject(UITestBackdoorService.GetCurrentTrendsChartOption());

        [Preserve, Export(BackdoorMethodConstants.IsTrendsSeriesVisible + ":")]
        public NSString IsTrendsSeriesVisible(NSString seriesLabel) =>
            SerializeObject(UITestBackdoorService.IsTrendsSeriesVisible(seriesLabel.ToString()));

        [Preserve, Export(BackdoorMethodConstants.GetCurrentOnboardingPageNumber + ":")]
        public NSString GetCurrentOnboardingPageNumber(NSString noValue) =>
            SerializeObject(UITestBackdoorService.GetCurrentOnboardingPageNumber());

        [Preserve, Export(BackdoorMethodConstants.PopPage + ":")]
        public async void PopPage(NSString noValue) =>
            await UITestBackdoorService.PopPage().ConfigureAwait(false);

        [Preserve, Export(BackdoorMethodConstants.ShouldSendNotifications + ":")]
        public NSString ShouldSendNotifications(NSString noValue) =>
            SerializeObject(UITestBackdoorService.ShouldSendNotifications());

        [Preserve, Export(BackdoorMethodConstants.GetPreferredTheme + ":")]
        public NSString GetPreferredTheme(NSString noValue) =>
            SerializeObject(UITestBackdoorService.GetPreferredTheme());

        [Preserve, Export(BackdoorMethodConstants.TriggerReviewRequest + ":")]
        public void TriggerReviewRequest(NSString noValue) => UITestBackdoorService.TriggerReviewRequest();

        [Preserve, Export(BackdoorMethodConstants.GetReviewRequestAppStoreTitle + ":")]
        public NSString GetReviewRequestAppStoreTitle(NSString noValue) =>
           SerializeObject(UITestBackdoorService.GetReviewRequestAppStoreTitle());

        [Preserve, Export(BackdoorMethodConstants.AreNotificationsEnabled + ":")]
        public NSString AreNotificationsEnabled(NSString noValue) =>
           SerializeObject(UITestBackdoorService.AreNotificationsEnabled().GetAwaiter().GetResult());

        [Preserve, Export(BackdoorMethodConstants.GetGitHubToken + ":")]
        public NSString GetGitHubToken(NSString noValue) =>
            SerializeObject(UITestBackdoorService.GetGitHubToken().GetAwaiter().GetResult());

        [Preserve, Export(BackdoorMethodConstants.GetLoggedInUserAlias + ":")]
        public NSString GetLoggedInUserAlias(NSString noValue) =>
            SerializeObject(UITestBackdoorService.GetLoggedInUserAlias());

        [Preserve, Export(BackdoorMethodConstants.GetLoggedInUserName + ":")]
        public NSString GetLoggedInUserName(NSString noValue) =>
            SerializeObject(UITestBackdoorService.GetLoggedInUserName());

        [Preserve, Export(BackdoorMethodConstants.GetLoggedInUserAvatarUrl + ":")]
        public NSString GetLoggedInUserAvatarUrl(NSString noValue) =>
            SerializeObject(UITestBackdoorService.GetLoggedInUserAvatarUrl());

        [Preserve, Export(BackdoorMethodConstants.GetPreferredLanguage + ":")]
        public NSString GetPreferredLanguage(NSString noValue) =>
            SerializeObject(UITestBackdoorService.GetPreferredLanguage());

        static NSString SerializeObject<T>(T value) => new NSString(Newtonsoft.Json.JsonConvert.SerializeObject(value));
    }
}
#endif
