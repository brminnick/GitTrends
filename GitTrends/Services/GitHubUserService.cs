using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using GitHubApiStatus;
using GitTrends.Shared;
using Newtonsoft.Json;
using Xamarin.Essentials.Interfaces;

namespace GitTrends
{
    public class GitHubUserService
    {
        const string _oauthTokenKey = "OAuthToken";

        readonly static WeakEventManager<string> _nameChangedEventManager = new();
        readonly static WeakEventManager<string> _aliasChangedEventManager = new();
        readonly static WeakEventManager<string> _avatarUrlChangedEventManager = new();
        readonly static WeakEventManager<bool> _shouldIncludeOrganizationsChangedEventManager = new();

        readonly IPreferences _preferences;
        readonly ISecureStorage _secureStorage;
        readonly GitHubApiStatusService _gitHubApiStatusService;

        public GitHubUserService(IPreferences preferences,
                                    ISecureStorage secureStorage,
                                    GitHubApiStatusService gitHubApiStatusService)
        {
            _preferences = preferences;
            _secureStorage = secureStorage;
            _gitHubApiStatusService = gitHubApiStatusService;

            GitHubAuthenticationService.LoggedOut += HandleLoggedOut;
            GitHubAuthenticationService.DemoUserActivated += HandleDemoUserActivated;
            GitHubAuthenticationService.AuthorizeSessionCompleted += HandleAuthorizeSessionCompleted;
        }

        public static event EventHandler<string> NameChanged
        {
            add => _nameChangedEventManager.AddEventHandler(value);
            remove => _nameChangedEventManager.RemoveEventHandler(value);
        }

        public static event EventHandler<string> AliasChanged
        {
            add => _aliasChangedEventManager.AddEventHandler(value);
            remove => _aliasChangedEventManager.RemoveEventHandler(value);
        }

        public static event EventHandler<string> AvatarUrlChanged
        {
            add => _avatarUrlChangedEventManager.AddEventHandler(value);
            remove => _avatarUrlChangedEventManager.RemoveEventHandler(value);
        }

        public static event EventHandler<bool> ShouldIncludeOrganizationsChanged
        {
            add => _shouldIncludeOrganizationsChangedEventManager.AddEventHandler(value);
            remove => _shouldIncludeOrganizationsChangedEventManager.RemoveEventHandler(value);
        }

        public bool IsDemoUser
        {
            get => _preferences.Get(nameof(IsDemoUser), false);
            private set => _preferences.Set(nameof(IsDemoUser), value);
        }

        public bool IsAuthenticated
        {
            get => _preferences.Get(nameof(IsAuthenticated), false);
            private set => _preferences.Set(nameof(IsAuthenticated), value);
        }

        public string Alias
        {
            get => _preferences.Get(nameof(Alias), string.Empty);
            set
            {
                if (Alias != value)
                {
                    _preferences.Set(nameof(Alias), value);
                    OnAliasChanged(value);
                }
            }
        }

        public string Name
        {
            get => _preferences.Get(nameof(Name), string.Empty);
            set
            {
                if (Name != value)
                {
                    _preferences.Set(nameof(Name), value);
                    OnNameChanged(value);
                }
            }
        }

        public string AvatarUrl
        {
            get => _preferences.Get(nameof(AvatarUrl), string.Empty);
            set
            {
                if (AvatarUrl != value)
                {
                    _preferences.Set(nameof(AvatarUrl), value);
                    OnAvatarUrlChanged(value);
                }
            }
        }

        public bool ShouldIncludeOrganizations
        {
            get => _preferences.Get(nameof(ShouldIncludeOrganizations), true);
            set
            {
                if (ShouldIncludeOrganizations != value)
                {
                    _preferences.Set(nameof(ShouldIncludeOrganizations), value);
                    OnShouldIncludeOrganizationsChanged(value);
                }
            }
        }

        public async Task<GitHubToken> GetGitHubToken()
        {
            var serializedToken = await _secureStorage.GetAsync(_oauthTokenKey).ConfigureAwait(false);

            try
            {
                var token = JsonConvert.DeserializeObject<GitHubToken?>(serializedToken);

                if (token is null)
                    return GitHubToken.Empty;

                if (!_gitHubApiStatusService.IsProductHeaderValueValid)
                    _gitHubApiStatusService.AddProductHeaderValue(getProductHeaderValue());

                if (!_gitHubApiStatusService.IsAuthenticationHeaderValueSet)
                    _gitHubApiStatusService.SetAuthenticationHeaderValue(getAuthenticationHeaderValue(token));

                IsAuthenticated = true;

                return token;
            }
            catch (ArgumentNullException)
            {
                IsAuthenticated = false; 
                return GitHubToken.Empty;
            }
            catch (JsonReaderException)
            {
                IsAuthenticated = false;
                return GitHubToken.Empty;
            }

            static AuthenticationHeaderValue getAuthenticationHeaderValue(in GitHubToken token) => new(token.TokenType, token.AccessToken);
            static ProductHeaderValue getProductHeaderValue() => new($"{nameof(GitTrends)}");
        }

        public async Task SaveGitHubToken(GitHubToken token)
        {
            if (token is null)
                throw new ArgumentNullException(nameof(token));

            if (token.AccessToken is null)
                throw new ArgumentNullException(nameof(token.AccessToken));

            var serializedToken = JsonConvert.SerializeObject(token);
            await _secureStorage.SetAsync(_oauthTokenKey, serializedToken).ConfigureAwait(false);

            IsAuthenticated = true;
        }

        public void InvalidateToken()
        {
            _secureStorage.Remove(_oauthTokenKey);
            IsAuthenticated = false;
        }

        void HandleLoggedOut(object sender, EventArgs e)
        {
            IsAuthenticated = false;
            IsDemoUser = false;
        }

        void HandleDemoUserActivated(object sender, EventArgs e) => IsDemoUser = true;
        void HandleAuthorizeSessionCompleted(object sender, AuthorizeSessionCompletedEventArgs e) => IsAuthenticated = e.IsSessionAuthorized;

        void OnNameChanged(in string name) => _nameChangedEventManager.RaiseEvent(this, name, nameof(NameChanged));
        void OnAliasChanged(in string alias) => _aliasChangedEventManager.RaiseEvent(this, alias, nameof(AliasChanged));
        void OnAvatarUrlChanged(in string avatarUrl) => _avatarUrlChangedEventManager.RaiseEvent(this, avatarUrl, nameof(AvatarUrlChanged));
        void OnShouldIncludeOrganizationsChanged(in bool shouldIncludeOrganizations) => _shouldIncludeOrganizationsChangedEventManager.RaiseEvent(this, shouldIncludeOrganizations, nameof(ShouldIncludeOrganizationsChanged));
    }
}
