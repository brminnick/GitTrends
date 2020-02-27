using System;
using System.Threading.Tasks;
using System.Windows.Input;
using AsyncAwaitBestPractices;
using AsyncAwaitBestPractices.MVVM;
using GitTrends.Mobile.Shared;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace GitTrends
{
    public class SettingsViewModel : BaseViewModel
    {
        readonly WeakEventManager<string?> _gitHubLoginUrlRetrievedEventManager = new WeakEventManager<string?>();
        readonly GitHubAuthenticationService _gitHubAuthenticationService;
        readonly TrendsChartSettingsService _trendsChartSettingsService;

        string _gitHubUserImageSource = string.Empty;
        string _gitHubUserNameLabelText = string.Empty;
        string _gitHubButtonText = string.Empty;
        bool _isAuthenticating;

        public SettingsViewModel(GitHubAuthenticationService gitHubAuthenticationService,
                                    TrendsChartSettingsService trendsChartSettingsService,
                                    AnalyticsService analyticsService) : base(analyticsService)
        {
            _gitHubAuthenticationService = gitHubAuthenticationService;
            _trendsChartSettingsService = trendsChartSettingsService;

            LoginButtonCommand = new AsyncCommand(ExecuteLoginButtonCommand, _ => !IsAuthenticating);
            DemoButtonCommand = new Command(ExecuteDemoButtonCommand);

            _gitHubAuthenticationService.AuthorizeSessionCompleted += HandleAuthorizeSessionCompleted;
            _gitHubAuthenticationService.AuthorizeSessionStarted += HandleAuthorizeSessionStarted;

            SetGitHubValues();
        }

        public event EventHandler<string?> GitHubLoginUrlRetrieved
        {
            add => _gitHubLoginUrlRetrievedEventManager.AddEventHandler(value);
            remove => _gitHubLoginUrlRetrievedEventManager.RemoveEventHandler(value);
        }

        public ICommand DemoButtonCommand { get; }
        public IAsyncCommand LoginButtonCommand { get; }

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
                return MainThread.InvokeOnMainThreadAsync(LoginButtonCommand.RaiseCanExecuteChanged);
            });
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
            GitHubAliasLabelText = GitHubAuthenticationService.Name = DemoDataConstants.Name;
            GitHubAvatarImageSource = GitHubAuthenticationService.AvatarUrl = DemoDataConstants.AvatarUrl;
            GitHubAuthenticationService.Alias = DemoDataConstants.Alias;

            SetGitHubValues();
        }

        void OnGitHubLoginUrlRetrieved(string? loginUrl) => _gitHubLoginUrlRetrievedEventManager.HandleEvent(this, loginUrl, nameof(GitHubLoginUrlRetrieved));
    }
}
