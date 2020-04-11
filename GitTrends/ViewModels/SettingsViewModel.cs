using System.Threading.Tasks;
using System.Windows.Input;
using AsyncAwaitBestPractices.MVVM;
using GitTrends.Mobile.Shared;
using Xamarin.Essentials;

namespace GitTrends
{
    public class SettingsViewModel : GitHubAuthenticationViewModel
    {
        readonly GitHubAuthenticationService _gitHubAuthenticationService;
        readonly TrendsChartSettingsService _trendsChartSettingsService;
        readonly DeepLinkingService _deepLinkingService;
        readonly NotificationService _notificationService;

        string _gitHubUserImageSource = string.Empty;
        string _gitHubUserNameLabelText = string.Empty;
        string _gitHubNameLabelText = string.Empty;
        string _gitHubButtonText = string.Empty;
        bool _isRegisteringForNotifications;

        public SettingsViewModel(GitHubAuthenticationService gitHubAuthenticationService,
                                    TrendsChartSettingsService trendsChartSettingsService,
                                    AnalyticsService analyticsService,
                                    DeepLinkingService deepLinkingService,
                                    NotificationService notificationService)
                : base(gitHubAuthenticationService, deepLinkingService, analyticsService)
        {
            _gitHubAuthenticationService = gitHubAuthenticationService;
            _trendsChartSettingsService = trendsChartSettingsService;
            _deepLinkingService = deepLinkingService;
            _notificationService = notificationService;

            RegisterForPushNotificationsButtonCommand = new AsyncCommand(ExecuteRegisterForPushNotificationsButtonCommand, _ => !IsRegisteringForNotifications);
            CreatedByLabelTappedCommand = new AsyncCommand(ExecuteCreatedByLabelTapped);

            _gitHubAuthenticationService.AuthorizeSessionCompleted += HandleAuthorizeSessionCompleted;

            SetGitHubValues();
        }

        public ICommand CreatedByLabelTappedCommand { get; }
        public IAsyncCommand RegisterForPushNotificationsButtonCommand { get; }

        public override bool IsDemoButtonVisible => base.IsDemoButtonVisible && LoginButtonText is GitHubLoginButtonConstants.ConnectWithGitHub;

        public bool ShouldShowClonesByDefaultSwitchValue
        {
            get => _trendsChartSettingsService.ShouldShowClonesByDefault;
            set
            {
                _trendsChartSettingsService.ShouldShowClonesByDefault = value;
                OnPropertyChanged();
            }
        }

        public bool ShouldShowUniqueClonesByDefaultSwitchValue
        {
            get => _trendsChartSettingsService.ShouldShowUniqueClonesByDefault;
            set
            {
                _trendsChartSettingsService.ShouldShowUniqueClonesByDefault = value;
                OnPropertyChanged();
            }
        }

        public bool ShouldShowViewsByDefaultSwitchValue
        {
            get => _trendsChartSettingsService.ShouldShowViewsByDefault;
            set
            {
                _trendsChartSettingsService.ShouldShowViewsByDefault = value;
                OnPropertyChanged();
            }
        }

        public bool ShouldShowUniqueViewsByDefaultSwitchValue
        {
            get => _trendsChartSettingsService.ShouldShowUniqueViewsByDefault;
            set
            {
                _trendsChartSettingsService.ShouldShowUniqueViewsByDefault = value;
                OnPropertyChanged();
            }
        }

        public string LoginButtonText
        {
            get => _gitHubButtonText;
            set => SetProperty(ref _gitHubButtonText, value, () => OnPropertyChanged(nameof(IsDemoButtonVisible)));
        }

        public string GitHubAvatarImageSource
        {
            get => _gitHubUserImageSource;
            set => SetProperty(ref _gitHubUserImageSource, value);
        }

        public string GitHubAliasLabelText
        {
            get => _gitHubUserNameLabelText;
            set => SetProperty(ref _gitHubUserNameLabelText, value);
        }

        public string GitHubNameLabelText
        {
            get => _gitHubNameLabelText;
            set => SetProperty(ref _gitHubNameLabelText, value);
        }

        bool IsRegisteringForNotifications
        {
            get => _isRegisteringForNotifications;
            set
            {
                if (_isRegisteringForNotifications != value)
                {
                    _isRegisteringForNotifications = value;
                    MainThread.BeginInvokeOnMainThread(RegisterForPushNotificationsButtonCommand.RaiseCanExecuteChanged);
                }
            }
        }

        void HandleAuthorizeSessionCompleted(object sender, AuthorizeSessionCompletedEventArgs e) => SetGitHubValues();

        void SetGitHubValues()
        {
            GitHubAliasLabelText = _gitHubAuthenticationService.IsAuthenticated ? $"@{GitHubAuthenticationService.Alias}" : string.Empty;
            GitHubNameLabelText = _gitHubAuthenticationService.IsAuthenticated ? GitHubAuthenticationService.Name : "Not Logged In";
            LoginButtonText = _gitHubAuthenticationService.IsAuthenticated ? $"{GitHubLoginButtonConstants.Disconnect}" : $"{GitHubLoginButtonConstants.ConnectWithGitHub}";

            GitHubAvatarImageSource = "DefaultProfileImage";

            if (Connectivity.NetworkAccess is NetworkAccess.Internet && !string.IsNullOrWhiteSpace(GitHubAuthenticationService.AvatarUrl))
                GitHubAvatarImageSource = GitHubAuthenticationService.AvatarUrl;
        }

        protected override async Task ExecuteConnectToGitHubButtonCommand(GitHubAuthenticationService gitHubAuthenticationService, DeepLinkingService deepLinkingService)
        {
            AnalyticsService.Track("GitHub Button Tapped", nameof(GitHubAuthenticationService.IsAuthenticated), gitHubAuthenticationService.IsAuthenticated.ToString());

            if (gitHubAuthenticationService.IsAuthenticated)
            {
                await gitHubAuthenticationService.LogOut().ConfigureAwait(false);

                SetGitHubValues();
            }
            else
            {
                await base.ExecuteConnectToGitHubButtonCommand(gitHubAuthenticationService, deepLinkingService).ConfigureAwait(false);
            }
        }

        protected override async Task ExecuteDemoButtonCommand(string buttonText)
        {
            try
            {
                await base.ExecuteDemoButtonCommand(buttonText).ConfigureAwait(false);

                AnalyticsService.Track("Settings Demo Button Tapped");

                await _gitHubAuthenticationService.ActivateDemoUser().ConfigureAwait(false);

                SetGitHubValues();
            }
            finally
            {
                IsAuthenticating = false;
            }
        }

        async Task ExecuteRegisterForPushNotificationsButtonCommand()
        {
            IsRegisteringForNotifications = true;

            try
            {
                var result = await _notificationService.Register(true).ConfigureAwait(false);
                AnalyticsService.Track("Settings Notification Button Tapped", "Result", result.ToString());
            }
            finally
            {
                IsRegisteringForNotifications = false;
            }
        }

        Task ExecuteCreatedByLabelTapped()
        {
            AnalyticsService.Track("CreatedBy Label Tapped");
            return _deepLinkingService.OpenApp("twitter://user?id=3418408341", "https://twitter.com/intent/user?user_id=3418408341");
        }
    }
}
