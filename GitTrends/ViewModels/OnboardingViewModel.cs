namespace GitTrends
{
    public class OnboardingViewModel : BaseViewModel
    {
        int _currentPageIndex = 0;

        public OnboardingViewModel(AnalyticsService analyticsService) : base(analyticsService)
        {
        }

        public int CurrentPageIndex
        {
            get => _currentPageIndex;
            set => SetProperty(ref _currentPageIndex, value);
        }
    }
}
