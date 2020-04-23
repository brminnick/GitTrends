#if !AppStore
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GitTrends.Mobile.Shared;
using GitTrends.Shared;
using Syncfusion.SfChart.XForms;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace GitTrends
{
    public class UITestBackdoorService
    {
        readonly GitHubAuthenticationService _gitHubAuthenticationService;
        readonly GitHubGraphQLApiService _gitHubGraphQLApiService;
        readonly TrendsChartSettingsService _trendsChartSettingsService;
        readonly NotificationService _notificationService;
        readonly ThemeService _themeService;

        public UITestBackdoorService(GitHubAuthenticationService gitHubAuthenticationService,
                                        NotificationService notificationService,
                                        GitHubGraphQLApiService gitHubGraphQLApiService,
                                        TrendsChartSettingsService trendsChartSettingsService,
                                        ThemeService themeService)
        {
            _gitHubAuthenticationService = gitHubAuthenticationService;
            _gitHubGraphQLApiService = gitHubGraphQLApiService;
            _trendsChartSettingsService = trendsChartSettingsService;
            _notificationService = notificationService;
            _themeService = themeService;
        }

        public async Task SetGitHubUser(string token, CancellationToken cancellationToken)
        {
            await _gitHubAuthenticationService.SaveGitHubToken(new GitHubToken(token, string.Empty, "Bearer")).ConfigureAwait(false);

            var (alias, name, avatarUri) = await _gitHubGraphQLApiService.GetCurrentUserInfo(cancellationToken).ConfigureAwait(false);

            GitHubAuthenticationService.Alias = alias;
            GitHubAuthenticationService.AvatarUrl = avatarUri.ToString();
            GitHubAuthenticationService.Name = name;
        }

        public void TriggerReviewRequest()
        {
            var referringSitesPage = (ReferringSitesPage)GetVisibleContentPage();
            var referringSitesViewModel = (ReferringSitesViewModel)referringSitesPage.BindingContext;

            referringSitesViewModel.IsStoreRatingRequestVisible = true;
        }

        public string GetReviewRequestAppStoreTitle() => AppStoreConstants.RatingRequest;

        public PreferredTheme GetPreferredTheme() => _themeService.Preference;

        public bool ShouldSendNotifications() => _notificationService.ShouldSendNotifications;

        public Task TriggerPullToRefresh() => MainThread.InvokeOnMainThreadAsync(() => GetVisibleRefreshView().IsRefreshing = true);

        public IReadOnlyList<T> GetVisibleCollection<T>() => GetVisibleCollection().Cast<T>().ToList();

        public Task PopPage()
        {
            FirstRunService.IsFirstRun = false;

            if (GetVisiblePageFromModalStack() is Page page)
                return page.Navigation.PopModalAsync();

            return GetVisiblePageFromNavigationStack().Navigation.PopAsync();
        }

        public IEnumerable GetVisibleCollection()
        {
            var collectionView = (CollectionView)GetVisibleRefreshView().Content;
            return collectionView.ItemsSource;
        }

        public TrendsChartOption GetCurrentTrendsChartOption() => _trendsChartSettingsService.CurrentTrendsChartOption;

        public bool IsTrendsSeriesVisible(string seriesTitle)
        {
            var trendsPageLayout = (Layout<View>)GetVisibleContentPage().Content;

            var trendsChart = trendsPageLayout.Children.OfType<SfChart>().First();

            return trendsChart.Series.First(x => x.Label.Equals(seriesTitle)).IsVisible;
        }

        public int GetCurrentOnboardingPageNumber()
        {
            var onboardingCarouselPage = (OnboardingCarouselPage)Application.Current.MainPage.Navigation.ModalStack.Last();
            var currentPage = onboardingCarouselPage.CurrentPage;

            return onboardingCarouselPage.Children.IndexOf(currentPage);
        }

        RefreshView GetVisibleRefreshView()
        {
            var visibleContentPage = GetVisibleContentPage();

            if (visibleContentPage.Content is RefreshView refreshView)
                return refreshView;
            else if (visibleContentPage.Content is Layout<View> layout && layout.Children.OfType<RefreshView>().FirstOrDefault() is RefreshView layoutRefreshView)
                return layoutRefreshView;
            else
                throw new NotSupportedException($"{visibleContentPage.GetType()} Does Not Contain a RefreshView");
        }

        ContentPage GetVisibleContentPage() => (ContentPage)(GetVisiblePageFromModalStack() ?? GetVisiblePageFromNavigationStack());

        Page? GetVisiblePageFromModalStack() => Application.Current.MainPage.Navigation.ModalStack.LastOrDefault();

        Page GetVisiblePageFromNavigationStack() => Application.Current.MainPage.Navigation.NavigationStack.Last();
    }
}
#endif
