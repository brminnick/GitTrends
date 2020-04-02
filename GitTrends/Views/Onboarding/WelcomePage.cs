using GitTrends.Mobile.Shared;
using Xamarin.Forms;

namespace GitTrends
{
    class WelcomePage : BaseOnboardingPage
    {
        public WelcomePage() : base(OnboardingConstants.TealBackgroundColorHex, OnboardingConstants.SkipText)
        {
        }

        protected override View CreateImageView() => new Image
        {
            Source = "GitTrends",
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };

        protected override View CreateDescriptionView() => new BoxView();
    }
}
