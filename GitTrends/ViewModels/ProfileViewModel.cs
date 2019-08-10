using System;
using System.Threading.Tasks;
using System.Windows.Input;
using AsyncAwaitBestPractices;
using AsyncAwaitBestPractices.MVVM;
using Xamarin.Essentials;

namespace GitTrends
{
    public class ProfileViewModel : BaseViewModel
    {
        readonly WeakEventManager<string> _gitHubLoginUrlRetrievedEventManager = new WeakEventManager<string>();
        readonly GitHubAuthenticationService _gitHubAuthenticationService;

        string _gitHubUserImageSource = string.Empty;
        string _gitHubUserNameLabelText = string.Empty;
        string _gitHubButtonText = string.Empty;
        bool _isAuthenticating;

        public ProfileViewModel(GitHubAuthenticationService gitHubAuthenticationService)
        {
            _gitHubAuthenticationService = gitHubAuthenticationService;

            LoginButtonCommand = new AsyncCommand(ExecuteLoginButtonCommand);

            _gitHubAuthenticationService.AuthorizeSessionCompleted += HandleAuthorizeSessionCompleted;
            _gitHubAuthenticationService.AuthorizeSessionStarted += HandleAuthorizeSessionStarted;

            SetGitHubValues();
        }

        public event EventHandler<string> GitHubLoginUrlRetrieved
        {
            add => _gitHubLoginUrlRetrievedEventManager.AddEventHandler(value);
            remove => _gitHubLoginUrlRetrievedEventManager.RemoveEventHandler(value);
        }

        public bool IsNotAuthenticating => !IsAuthenticating;

        public ICommand LoginButtonCommand { get; }

        public string LoginButtonText
        {
            get => _gitHubButtonText;
            set => SetProperty(ref _gitHubButtonText, value);
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
            set => SetProperty(ref _isAuthenticating, value, () => OnPropertyChanged(nameof(IsNotAuthenticating)));
        }

        void HandleAuthorizeSessionCompleted(object sender, AuthorizeSessionCompletedEventArgs e)
        {
            SetGitHubValues();

            IsAuthenticating = false;
        }

        void HandleAuthorizeSessionStarted(object sender, EventArgs e) => IsAuthenticating = true;

        void SetGitHubValues()
        {
            GitHubAliasLabelText = _gitHubAuthenticationService.IsAuthenticated ? _gitHubAuthenticationService.Name : string.Empty;
            LoginButtonText = FontAwesomeButton.GitHubOctocat.ToString() + (_gitHubAuthenticationService.IsAuthenticated ? " Disconnect" : " Connect with GitHub");

            GitHubAvatarImageSource = "DefaultProfileImage";

            if (Connectivity.NetworkAccess is NetworkAccess.Internet && !string.IsNullOrWhiteSpace(_gitHubAuthenticationService.AvatarUrl))
                GitHubAvatarImageSource = _gitHubAuthenticationService.AvatarUrl;
        }

        async Task ExecuteLoginButtonCommand()
        {
            if (_gitHubAuthenticationService.IsAuthenticated)
            {
                await _gitHubAuthenticationService.LogOut().ConfigureAwait(false);

                SetGitHubValues();
            }
            else
            {
                IsAuthenticating = true;

                var loginUrl = await _gitHubAuthenticationService.GetGitHubLoginUrl().ConfigureAwait(false);

                OnGitHubLoginUrlRetrieved(loginUrl);
            }
        }

        void OnGitHubLoginUrlRetrieved(string loginUrl) => _gitHubLoginUrlRetrievedEventManager.HandleEvent(this, loginUrl, nameof(GitHubLoginUrlRetrieved));
    }
}
