namespace GitTrends
{
    public class ProfileViewModel : BaseViewModel
    {
        #region Fields
        string _gitHubUserImageSource, _gitHubUserNameLabelText;
        #endregion

        #region Constructors
        public ProfileViewModel()
        {
            GitHubAuthenticationService.AuthorizeSessionCompleted += HandleAuthorizeSessionCompleted;
            SetGitHubValues();
        }
        #endregion

        #region Properties
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
        #endregion

        #region Methods
        void HandleAuthorizeSessionCompleted(object sender, AuthorizeSessionCompletedEventArgs e) => SetGitHubValues();

        void SetGitHubValues()
        {
            GitHubAliasLabelText = string.IsNullOrWhiteSpace(GitHubAuthenticationService.Name) ? "" : GitHubAuthenticationService.Name;

            if (Xamarin.Essentials.Connectivity.NetworkAccess is Xamarin.Essentials.NetworkAccess.Internet && !string.IsNullOrWhiteSpace(GitHubAuthenticationService.AvatarUrl))
                GitHubAvatarImageSource = GitHubAuthenticationService.AvatarUrl;
            else
                GitHubAvatarImageSource = "DefaultProfileImage";
        }
        #endregion
    }
}
