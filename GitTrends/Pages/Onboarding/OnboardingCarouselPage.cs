using System;
using System.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace GitTrends
{
    public class OnboardingCarouselPage : CarouselPage
    {
        readonly AnalyticsService _analyticsService;
        readonly OnboardingViewModel _onboardingViewModel;

        public OnboardingCarouselPage(GitTrendsOnboardingPage welcomeOnboardingPage,
                                        ChartOnboardingPage chartOnboardingPage,
                                        NotificationsOnboardingPage notificationsOnboardingPage,
                                        ConnectToGitHubOnboardingPage connectToGitHubOnboardingPage,
                                        OnboardingViewModel onboardingViewModel,
                                        AnalyticsService analyticsService)
        {
            On<iOS>().SetUseSafeArea(true);
            ChildAdded += HandleChildAdded;

            BindingContext = _onboardingViewModel = onboardingViewModel;
            _analyticsService = analyticsService;

            BaseOnboardingContentPage.SkipButtonTapped += HandleSkipButtonTapped;

            Children.Add(welcomeOnboardingPage);
            Children.Add(chartOnboardingPage);
            Children.Add(notificationsOnboardingPage);
            Children.Add(connectToGitHubOnboardingPage);
        }

        public int PageCount => Children.Count;

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            _onboardingViewModel.IsAuthenticating = false;
        }

        void HandleSkipButtonTapped(object sender, EventArgs e)
        {
            _analyticsService.Track("Skip Button Tapped");

            MainThread.BeginInvokeOnMainThread(() => CurrentPage = Children.Last());
        }

        void HandleChildAdded(object sender, ElementEventArgs e) => OnPropertyChanged(nameof(PageCount));
    }
}
