using System;
using System.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace GitTrends
{
    class OnboardingCarouselPage : CarouselPage
    {
        readonly AnalyticsService _analyticsService;

        public OnboardingCarouselPage(GitTrendsOnboardingPage welcomeOnboardingPage,
                                ChartOnboardingPage chartOnboardingPage,
                                NotificationsOnboardingPage notificationsOnboardingPage,
                                OnboardingViewModel onboardingViewModel,
                                AnalyticsService analyticsService)
        {
            On<iOS>().SetUseSafeArea(true);

            BindingContext = onboardingViewModel;
            _analyticsService = analyticsService;

            BaseOnboardingContentPage.SkipButtonTapped += HandleSkipButtonTapped;

            Children.Add(welcomeOnboardingPage);
            Children.Add(chartOnboardingPage);
            Children.Add(notificationsOnboardingPage);            
        }

        void HandleSkipButtonTapped(object sender, EventArgs e)
        {
            _analyticsService.Track("Skip Button Tapped");

            MainThread.BeginInvokeOnMainThread(() => CurrentPage = Children.Last());
        }
    }
}
