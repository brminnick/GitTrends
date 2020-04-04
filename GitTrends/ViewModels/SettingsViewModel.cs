using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using AsyncAwaitBestPractices;
using AsyncAwaitBestPractices.MVVM;
using GitTrends.Mobile.Shared;
using Shiny;
using Shiny.Notifications;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace GitTrends
{
    public class SettingsViewModel : BaseViewModel
    {
        readonly WeakEventManager<(bool isSuccessful, string errorMessage)> _registerForNotificationCompletedEventHandler = new WeakEventManager<(bool isSuccessful, string errorMessage)>();
        readonly WeakEventManager<string?> _gitHubLoginUrlRetrievedEventManager = new WeakEventManager<string?>();

        readonly GitHubAuthenticationService _gitHubAuthenticationService;
        readonly TrendsChartSettingsService _trendsChartSettingsService;
        readonly DeepLinkingService _deepLinkingService;
        readonly NotificationService _notificationService;

        string _gitHubUserImageSource = string.Empty;
        string _gitHubUserNameLabelText = string.Empty;
        string _gitHubButtonText = string.Empty;
        bool _isAuthenticating, _isRegisteringForNotifications;

        public SettingsViewModel(GitHubAuthenticationService gitHubAuthenticationService,
                                    TrendsChartSettingsService trendsChartSettingsService,
                                    AnalyticsService analyticsService,
                                    DeepLinkingService deepLinkingService,
                                    NotificationService notificationService) : base(analyticsService)
        {
            _gitHubAuthenticationService = gitHubAuthenticationService;
            _trendsChartSettingsService = trendsChartSettingsService;
            _deepLinkingService = deepLinkingService;
            _notificationService = notificationService;

            RegisterForPushNotificationsButtonCommand = new AsyncCommand(ExecuteRegisterForPushNotificationsButtonCommand, _ => !IsRegisteringForNotifications);
            CreatedByLabelTappedCommand = new AsyncCommand(ExecuteCreatedByLabelTapped);
            LoginButtonCommand = new AsyncCommand(ExecuteLoginButtonCommand, _ => !IsAuthenticating);
            DemoButtonCommand = new Command(ExecuteDemoButtonCommand);

            _gitHubAuthenticationService.AuthorizeSessionCompleted += HandleAuthorizeSessionCompleted;
            _gitHubAuthenticationService.AuthorizeSessionStarted += HandleAuthorizeSessionStarted;

            SetGitHubValues();
        }

        public event EventHandler<(bool isSuccessful, string errorMessage)> RegisterForNotificationsCompleted
        {
            add => _registerForNotificationCompletedEventHandler.AddEventHandler(value);
            remove => _registerForNotificationCompletedEventHandler.RemoveEventHandler(value);
        }

        public event EventHandler<string?> GitHubLoginUrlRetrieved
        {
            add => _gitHubLoginUrlRetrievedEventManager.AddEventHandler(value);
            remove => _gitHubLoginUrlRetrievedEventManager.RemoveEventHandler(value);
        }

        public ICommand DemoButtonCommand { get; }
        public ICommand CreatedByLabelTappedCommand { get; }
        public IAsyncCommand LoginButtonCommand { get; }
        public IAsyncCommand RegisterForPushNotificationsButtonCommand { get; }

        public bool IsDemoButtonVisible => !IsAuthenticating
                                            && LoginButtonText is GitHubLoginButtonConstants.ConnectWithGitHub
                                            && GitHubAuthenticationService.Alias != DemoDataConstants.Alias;

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

        public bool IsAuthenticating
        {
            get => _isAuthenticating;
            set => SetProperty(ref _isAuthenticating, value, () =>
            {
                OnPropertyChanged(nameof(IsDemoButtonVisible));
                MainThread.InvokeOnMainThreadAsync(LoginButtonCommand.RaiseCanExecuteChanged).SafeFireAndForget(ex => Debug.WriteLine(ex));
            });
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

        void HandleAuthorizeSessionCompleted(object sender, AuthorizeSessionCompletedEventArgs e)
        {
            SetGitHubValues();

            IsAuthenticating = false;
        }

        void HandleAuthorizeSessionStarted(object sender, EventArgs e) => IsAuthenticating = true;

        void SetGitHubValues()
        {
            GitHubAliasLabelText = _gitHubAuthenticationService.IsAuthenticated ? GitHubAuthenticationService.Name : string.Empty;
            LoginButtonText = _gitHubAuthenticationService.IsAuthenticated ? $"{GitHubLoginButtonConstants.Disconnect}" : $"{GitHubLoginButtonConstants.ConnectWithGitHub}";

            GitHubAvatarImageSource = "DefaultProfileImage";

            if (Connectivity.NetworkAccess is NetworkAccess.Internet && !string.IsNullOrWhiteSpace(GitHubAuthenticationService.AvatarUrl))
                GitHubAvatarImageSource = GitHubAuthenticationService.AvatarUrl;
        }

        async Task ExecuteLoginButtonCommand()
        {
            AnalyticsService.Track("GitHub Button Tapped", nameof(GitHubAuthenticationService.IsAuthenticated), _gitHubAuthenticationService.IsAuthenticated.ToString());

            if (_gitHubAuthenticationService.IsAuthenticated)
            {
                await _gitHubAuthenticationService.LogOut().ConfigureAwait(false);

                SetGitHubValues();
            }
            else
            {
                IsAuthenticating = true;

                try
                {
                    var loginUrl = await _gitHubAuthenticationService.GetGitHubLoginUrl().ConfigureAwait(false);
                    OnGitHubLoginUrlRetrieved(loginUrl);
                }
                catch (Exception e)
                {
                    AnalyticsService.Report(e);

                    OnGitHubLoginUrlRetrieved(null);
                    IsAuthenticating = false;
                }
            }
        }

        void ExecuteDemoButtonCommand()
        {
            AnalyticsService.Track("Demo Button Tapped");

            _gitHubAuthenticationService.ActivateDemoUser();

            SetGitHubValues();
        }

        async Task ExecuteRegisterForPushNotificationsButtonCommand()
        {
            AccessState? finalNotificationRequestResult = null;

            IsRegisteringForNotifications = true;

            var initialNotificationRequestResult = await _notificationService.Register().ConfigureAwait(false);

            try
            {
                switch (initialNotificationRequestResult)
                {
                    case AccessState.Denied:
                    case AccessState.Disabled:
                        await _deepLinkingService.ShowSettingsUI().ConfigureAwait(false);
                        break;

                    case AccessState.Available:
                    case AccessState.Restricted:
                        OnRegisterForNotificationsCompleted(true, string.Empty);
                        break;

                    case AccessState.NotSetup:
                        await _notificationService.Register().ConfigureAwait(false);
                        break;

                    case AccessState.NotSupported:
                        OnRegisterForNotificationsCompleted(false, "Notifications Are Not Supported");
                        break;

                    default:
                        throw new NotImplementedException();
                }

                finalNotificationRequestResult = await _notificationService.Register().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                AnalyticsService.Report(e);
            }
            finally
            {
                IsRegisteringForNotifications = false;

                AnalyticsService.Track("Register For Notifications Button Tapped", new Dictionary<string, string>
                {
                    { nameof(initialNotificationRequestResult), initialNotificationRequestResult.ToString() },
                    { nameof(finalNotificationRequestResult), finalNotificationRequestResult?.ToString() ?? "null" },
                });
            }
        }

        Task ExecuteCreatedByLabelTapped()
        {
            AnalyticsService.Track("CreatedBy Label Tapped");
            return _deepLinkingService.OpenApp("twitter://user?id=3418408341", "https://twitter.com/intent/user?user_id=3418408341");
        }

        void OnGitHubLoginUrlRetrieved(string? loginUrl) => _gitHubLoginUrlRetrievedEventManager.HandleEvent(this, loginUrl, nameof(GitHubLoginUrlRetrieved));

        void OnRegisterForNotificationsCompleted(bool isSuccessful, string errorMessage) =>
            _registerForNotificationCompletedEventHandler.HandleEvent(this, (isSuccessful, errorMessage), nameof(RegisterForNotificationsCompleted));
    }
}
