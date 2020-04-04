using System.Threading.Tasks;
using System.Windows.Input;
using AsyncAwaitBestPractices.MVVM;

namespace GitTrends
{
    class OnboardingViewModel : BaseViewModel
    {
        readonly AnalyticsService _analyticsService;
        readonly NotificationService _notificationService;

        public OnboardingViewModel(NotificationService notificationService, AnalyticsService analyticsService) : base(analyticsService)
        {
            _notificationService = notificationService;
            _analyticsService = analyticsService;

            EnableNotificationsButtonTapped = new AsyncCommand(ExecuteEnableNotificationsButtonTapped);
        }

        public ICommand EnableNotificationsButtonTapped { get; }

        async Task ExecuteEnableNotificationsButtonTapped()
        {
            var result = await _notificationService.Register().ConfigureAwait(false);

            _analyticsService.Track("Notification Button Tapped", "Result", result.ToString());
        }
    }
}
