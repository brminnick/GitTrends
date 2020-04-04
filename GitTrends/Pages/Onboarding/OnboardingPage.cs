using System;
using System.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace GitTrends
{
    class OnboardingPage : CarouselPage
    {
        readonly AnalyticsService _analyticsService;

        public OnboardingPage(WelcomeOnboardingPage welcomeOnboardingPage,
                                ChartOnboardingPage chartOnboardingPage,
                                OnboardingViewModel onboardingViewModel,
                                AnalyticsService analyticsService)
        {
            On<iOS>().SetUseSafeArea(true);

            BindingContext = onboardingViewModel;
            _analyticsService = analyticsService;

            BaseOnboardingPage.SkipButtonTapped += HandleSkipButtonTapped;

            Children.Add(welcomeOnboardingPage);
            Children.Add(chartOnboardingPage);
        }

        void HandleSkipButtonTapped(object sender, EventArgs e)
        {
            _analyticsService.Track("Skip Button Tapped");

            MainThread.BeginInvokeOnMainThread(() => CurrentPage = Children.Last());
        }
    }
}
