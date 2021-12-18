#if !AppStore
using System.Threading;
using Android.Runtime;
using Autofac;
using GitTrends.Mobile.Common;
using Java.Interop;

namespace GitTrends.Droid;

public partial class MainApplication
{
	UITestsBackdoorService? _uiTestBackdoorService;
	UITestsBackdoorService UITestBackdoorService => _uiTestBackdoorService ??= ContainerService.Container.Resolve<UITestsBackdoorService>();

	[Preserve, Export(BackdoorMethodConstants.SetGitHubUser)]
	public async void SetGitHubUser(string accessToken) =>
		await UITestBackdoorService.SetGitHubUser(accessToken.ToString(), CancellationToken.None).ConfigureAwait(false);

	[Preserve, Export(BackdoorMethodConstants.TriggerPullToRefresh)]
	public async void TriggerRepositoriesPullToRefresh() =>
		await UITestBackdoorService.TriggerPullToRefresh().ConfigureAwait(false);

	[Preserve, Export(BackdoorMethodConstants.GetVisibleCollection)]
	public string GetVisibleCollection() =>
		SerializeObject(UITestBackdoorService.GetVisibleCollection());

	[Preserve, Export(BackdoorMethodConstants.GetCurrentTrendsChartOption)]
	public string GetCurrentTrendsChartOption() =>
		SerializeObject(UITestBackdoorService.GetCurrentTrendsChartOption());

	[Preserve, Export(BackdoorMethodConstants.IsViewsClonesChartSeriesVisible)]
	public bool IsViewsClonesChartSeriesVisible(string seriesLabel) =>
		UITestBackdoorService.IsViewsClonesChartSeriesVisible(seriesLabel);

	[Preserve, Export(BackdoorMethodConstants.GetCurrentOnboardingPageNumber)]
	public string GetCurrentOnboardingPageNumber() =>
		SerializeObject(UITestBackdoorService.GetCurrentOnboardingPageNumber());

	[Preserve, Export(BackdoorMethodConstants.GetCurrentTrendsPageNumber)]
	public string GetCurrentTrendsPageNumber() =>
		SerializeObject(UITestBackdoorService.GetCurrentTrendsPageNumber());

	[Preserve, Export(BackdoorMethodConstants.PopPage)]
	public async void PopPage() => await UITestBackdoorService.PopPage().ConfigureAwait(false);

	[Preserve, Export(BackdoorMethodConstants.ShouldSendNotifications)]
	public bool ShouldSendNotifications() => UITestBackdoorService.ShouldSendNotifications();

	[Preserve, Export(BackdoorMethodConstants.GetPreferredTheme)]
	public string GetPreferredTheme() => SerializeObject(UITestBackdoorService.GetPreferredTheme());

	[Preserve, Export(BackdoorMethodConstants.TriggerReviewRequest)]
	public void TriggerReviewRequest() => UITestBackdoorService.TriggerReviewRequest();

	[Preserve, Export(BackdoorMethodConstants.GetReviewRequestAppStoreTitle)]
	public string GetReviewRequestAppStoreTitle() => SerializeObject(UITestBackdoorService.GetReviewRequestAppStoreTitle());

	[Preserve, Export(BackdoorMethodConstants.AreNotificationsEnabled)]
	public bool AreNotificationsEnabled() => UITestBackdoorService.AreNotificationsEnabled().GetAwaiter().GetResult();

	[Preserve, Export(BackdoorMethodConstants.GetGitHubToken)]
	public string GetGitHubToken() => SerializeObject(UITestBackdoorService.GetGitHubToken().GetAwaiter().GetResult());

	[Preserve, Export(BackdoorMethodConstants.GetLoggedInUserAlias)]
	public string GetLoggedInUserAlias() => SerializeObject(UITestBackdoorService.GetLoggedInUserAlias());

	[Preserve, Export(BackdoorMethodConstants.GetLoggedInUserName)]
	public string GetLoggedInUserName() => SerializeObject(UITestBackdoorService.GetLoggedInUserName());

	[Preserve, Export(BackdoorMethodConstants.GetLoggedInUserAvatarUrl)]
	public string GetLoggedInUserAvatarUrl() => SerializeObject(UITestBackdoorService.GetLoggedInUserAvatarUrl());

	[Preserve, Export(BackdoorMethodConstants.GetPreferredLanguage)]
	public string GetPreferredLanguage() => SerializeObject(UITestBackdoorService.GetPreferredLanguage());

	[Preserve, Export(BackdoorMethodConstants.GetContributorsCollection)]
	public string GetContributorsCollection() => SerializeObject(UITestBackdoorService.GetVisibleContributors());

	[Preserve, Export(BackdoorMethodConstants.GetLibrariesCollection)]
	public string GetLibrariesCollection() => SerializeObject(UITestBackdoorService.GetVisibleLibraries());

	static string SerializeObject<T>(T value) => Newtonsoft.Json.JsonConvert.SerializeObject(value);
}
#endif
