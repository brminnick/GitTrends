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
        #region Constant Fields
        readonly WeakEventManager<string> _gitHubLoginUrlRetrievedEventManager = new WeakEventManager<string>();
        #endregion

        #region Fields
        string _gitHubUserImageSource, _gitHubUserNameLabelText, _gitHubButtonText;
        bool _isAuthenticating;
        #endregion

        #region Constructors
        public ProfileViewModel()
        {
            LoginButtonCommand = new AsyncCommand(ExecuteLoginButtonCommand);

            GitHubAuthenticationService.AuthorizeSessionCompleted += HandleAuthorizeSessionCompleted;
            GitHubAuthenticationService.AuthorizeSessionStarted += HandleAuthorizeSessionStarted;

            SetGitHubValues();
        }
        #endregion

        #region Events
        public event EventHandler<string> GitHubLoginUrlRetrieved
        {
            add => _gitHubLoginUrlRetrievedEventManager.AddEventHandler(value);
            remove => _gitHubLoginUrlRetrievedEventManager.RemoveEventHandler(value);
        }
        #endregion

        #region Properties
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
        #endregion

        #region Methods
        void HandleAuthorizeSessionCompleted(object sender, AuthorizeSessionCompletedEventArgs e)
        {
            SetGitHubValues();

            IsAuthenticating = false;
        }

        void HandleAuthorizeSessionStarted(object sender, EventArgs e) => IsAuthenticating = true;

        void SetGitHubValues()
        {
            GitHubAliasLabelText = GitHubAuthenticationService.IsAuthenticated ? GitHubAuthenticationService.Name : string.Empty;
            LoginButtonText = GitHubAuthenticationService.IsAuthenticated ? FontAwesomeButton.GitHubOctocat.ToString() + " Logout" : FontAwesomeButton.GitHubOctocat.ToString() + " Connect with GitHub";

            GitHubAvatarImageSource = "DefaultProfileImage";

            if (Connectivity.NetworkAccess is NetworkAccess.Internet && !string.IsNullOrWhiteSpace(GitHubAuthenticationService.AvatarUrl))
                GitHubAvatarImageSource = GitHubAuthenticationService.AvatarUrl;
        }

        async Task ExecuteLoginButtonCommand()
        {
            if (GitHubAuthenticationService.IsAuthenticated)
            {
                await GitHubAuthenticationService.LogOut().ConfigureAwait(false);

                SetGitHubValues();
            }
            else
            {
                IsAuthenticating = true;

                var loginUrl = await GitHubAuthenticationService.GetGitHubLoginUrl().ConfigureAwait(false);

                OnGitHubLoginUrlRetrieved(loginUrl);
            }
        }

        void OnGitHubLoginUrlRetrieved(string loginUrl) => _gitHubLoginUrlRetrievedEventManager.HandleEvent(this, loginUrl, nameof(GitHubLoginUrlRetrieved));
        #endregion
    }
}
