#if !AppStore
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using GitTrends.Mobile.Common;
using GitTrends.Shared;
using Syncfusion.SfChart.XForms;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;

namespace GitTrends
{
    public class UITestsBackdoorService
    {
        readonly static WeakEventManager _popPageStartedEventManager = new WeakEventManager();
        readonly static WeakEventManager<Page> _popPageCompletedEventManager = new WeakEventManager<Page>();

        readonly IMainThread _mainThread;
        readonly ThemeService _themeService;
        readonly LanguageService _languageService;
        readonly GitHubUserService _gitHubUserService;
        readonly NotificationService _notificationService;
        readonly GitHubGraphQLApiService _gitHubGraphQLApiService;
        readonly TrendsChartSettingsService _trendsChartSettingsService;
        readonly GitHubAuthenticationService _gitHubAuthenticationService;

        public UITestsBackdoorService(IMainThread mainThread,
                                        ThemeService themeService,
                                        LanguageService languageService,
                                        GitHubUserService gitHubUserService,
                                        NotificationService notificationService,
                                        GitHubGraphQLApiService gitHubGraphQLApiService,
                                        TrendsChartSettingsService trendsChartSettingsService,
                                        GitHubAuthenticationService gitHubAuthenticationService)
        {
            _mainThread = mainThread;
            _themeService = themeService;
            _languageService = languageService;
            _gitHubUserService = gitHubUserService;
            _notificationService = notificationService;
            _gitHubGraphQLApiService = gitHubGraphQLApiService;
            _trendsChartSettingsService = trendsChartSettingsService;
            _gitHubAuthenticationService = gitHubAuthenticationService;
        }

        public static event EventHandler PopPageStarted
        {
            add => _popPageStartedEventManager.AddEventHandler(value);
            remove => _popPageStartedEventManager.RemoveEventHandler(value);
        }

        public static event EventHandler<Page> PopPageCompleted
        {
            add => _popPageCompletedEventManager.AddEventHandler(value);
            remove => _popPageCompletedEventManager.RemoveEventHandler(value);
        }

        public string? GetPreferredLanguage() => _languageService.PreferredLanguage;

        public string GetLoggedInUserAlias() => _gitHubUserService.Alias;
        public string GetLoggedInUserName() => _gitHubUserService.Name;
        public string GetLoggedInUserAvatarUrl() => _gitHubUserService.AvatarUrl;

        public async Task SetGitHubUser(string token, CancellationToken cancellationToken)
        {
            await _gitHubUserService.SaveGitHubToken(new GitHubToken(token, string.Empty, "Bearer")).ConfigureAwait(false);

            var (alias, name, avatarUri) = await _gitHubGraphQLApiService.GetCurrentUserInfo(cancellationToken).ConfigureAwait(false);

            _gitHubUserService.Alias = alias;
            _gitHubUserService.Name = name;
            _gitHubUserService.AvatarUrl = avatarUri.ToString();
        }

        public Task<GitHubToken> GetGitHubToken() => _gitHubUserService.GetGitHubToken();

        public void TriggerReviewRequest()
        {
            var referringSitesPage = (ReferringSitesPage)GetVisibleContentPage();
            var referringSitesViewModel = (ReferringSitesViewModel)referringSitesPage.BindingContext;

            referringSitesViewModel.IsStoreRatingRequestVisible = true;
        }

        public string GetReviewRequestAppStoreTitle() => AppStoreConstants.RatingRequest;

        public PreferredTheme GetPreferredTheme() => _themeService.Preference;

        public bool ShouldSendNotifications() => _notificationService.ShouldSendNotifications;

        public Task TriggerPullToRefresh() => _mainThread.InvokeOnMainThreadAsync(() => GetVisibleRefreshView().IsRefreshing = true);

        public IReadOnlyList<T> GetVisibleCollection<T>() => GetVisibleCollection().Cast<T>().ToList();

        public async Task PopPage()
        {
            OnPopPageStarted();

            Page pagePopped;

            if (GetVisiblePageFromModalStack() is Page page)
                pagePopped = await page.Navigation.PopModalAsync();
            else
                pagePopped = await GetVisiblePageFromNavigationStack().Navigation.PopAsync();

            OnPopPageCompleted(pagePopped);
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

            var trendsFrame = trendsPageLayout.Children.OfType<TrendsChart>().First();
            var trendsChart = (SfChart)trendsFrame.Content;

            return trendsChart.Series.First(x => x.Label.Equals(seriesTitle)).IsVisible;
        }

        public int GetCurrentOnboardingPageNumber()
        {
            var onboardingCarouselPage = (OnboardingCarouselPage)Application.Current.MainPage.Navigation.ModalStack.Last();
            var currentPage = onboardingCarouselPage.CurrentPage;

            return onboardingCarouselPage.Children.IndexOf(currentPage);
        }

        public Task<bool> AreNotificationsEnabled() => _notificationService.AreNotificationsEnabled();

        RefreshView GetVisibleRefreshView()
        {
            var visibleContentPage = GetVisibleContentPage();

            if (visibleContentPage.Content is RefreshView refreshView)
                return refreshView;
            else if (visibleContentPage.Content is Layout<View> layout && layout.Children.OfType<RefreshView>().FirstOrDefault() is RefreshView layoutRefreshView)
                return layoutRefreshView;
            else
                throw new Exception($"{visibleContentPage.GetType()} Does Not Contain a RefreshView");
        }

        ContentPage GetVisibleContentPage() => (ContentPage)(GetVisiblePageFromModalStack() ?? GetVisiblePageFromNavigationStack());

        Page? GetVisiblePageFromModalStack() => Application.Current.MainPage.Navigation.ModalStack.LastOrDefault();

        Page GetVisiblePageFromNavigationStack() => Application.Current.MainPage.Navigation.NavigationStack.Last();

        void OnPopPageStarted() => _popPageStartedEventManager.RaiseEvent(this, EventArgs.Empty, nameof(PopPageStarted));
        void OnPopPageCompleted(Page page) => _popPageCompletedEventManager.RaiseEvent(this, page, nameof(PopPageCompleted));
    }
}
#endif
