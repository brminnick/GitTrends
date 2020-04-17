using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Web;
using AsyncAwaitBestPractices;
using GitTrends.Mobile.Shared;
using GitTrends.Shared;
using Newtonsoft.Json;
using Xamarin.Essentials;

namespace GitTrends
{
    public class GitHubAuthenticationService
    {
        const string _oauthTokenKey = "OAuthToken";
        readonly WeakEventManager<AuthorizeSessionCompletedEventArgs> _authorizeSessionCompletedEventManager = new WeakEventManager<AuthorizeSessionCompletedEventArgs>();
        readonly WeakEventManager _authorizeSessionStartedEventManager = new WeakEventManager();
        readonly WeakEventManager _loggedOuteventManager = new WeakEventManager();
        readonly WeakEventManager _demoUserActivatedEventManager = new WeakEventManager();

        readonly AzureFunctionsApiService _azureFunctionsApiService;
        readonly GitHubGraphQLApiService _gitHubGraphQLApiService;
        readonly RepositoryDatabase _repositoryDatabase;
        readonly AnalyticsService _analyticsService;

        public GitHubAuthenticationService(AzureFunctionsApiService azureFunctionsApiService,
                                            GitHubGraphQLApiService gitHubGraphQLApiService,
                                            RepositoryDatabase repositoryDatabase,
                                            AnalyticsService analyticsService)
        {
            _azureFunctionsApiService = azureFunctionsApiService;
            _gitHubGraphQLApiService = gitHubGraphQLApiService;
            _repositoryDatabase = repositoryDatabase;
            _analyticsService = analyticsService;

            ThemeService.PreferenceChanged += HandlePreferenceChanged;
        }

        public event EventHandler AuthorizeSessionStarted
        {
            add => _authorizeSessionStartedEventManager.AddEventHandler(value);
            remove => _authorizeSessionStartedEventManager.RemoveEventHandler(value);
        }

        public event EventHandler<AuthorizeSessionCompletedEventArgs> AuthorizeSessionCompleted
        {
            add => _authorizeSessionCompletedEventManager.AddEventHandler(value);
            remove => _authorizeSessionCompletedEventManager.RemoveEventHandler(value);
        }

        public event EventHandler DemoUserActivated
        {
            add => _demoUserActivatedEventManager.AddEventHandler(value);
            remove => _demoUserActivatedEventManager.RemoveEventHandler(value);
        }

        public event EventHandler LoggedOut
        {
            add => _loggedOuteventManager.AddEventHandler(value);
            remove => _loggedOuteventManager.RemoveEventHandler(value);
        }

        public static bool IsDemoUser => Alias is DemoDataConstants.Alias;

        public bool IsAuthenticated => !string.IsNullOrWhiteSpace(Alias);

        public static string Alias
        {
            get => Preferences.Get(nameof(Alias), string.Empty);
            set => Preferences.Set(nameof(Alias), value);
        }

        public static string Name
        {
            get => Preferences.Get(nameof(Name), string.Empty);
            set => Preferences.Set(nameof(Name), value);
        }

        public static string AvatarUrl
        {
            get => Preferences.Get(nameof(AvatarUrl), string.Empty);
            set => Preferences.Set(nameof(AvatarUrl), value);
        }

        string MostRecentSessionId
        {
            get => Preferences.Get(nameof(MostRecentSessionId), string.Empty);
            set => Preferences.Set(nameof(MostRecentSessionId), value);
        }

        public static async Task<GitHubToken> GetGitHubToken()
        {
            var serializedToken = await SecureStorage.GetAsync(_oauthTokenKey).ConfigureAwait(false);

            try
            {
                var token = JsonConvert.DeserializeObject<GitHubToken?>(serializedToken);

                return token ?? new GitHubToken(string.Empty, string.Empty, string.Empty);
            }
            catch (ArgumentNullException)
            {
                return new GitHubToken(string.Empty, string.Empty, string.Empty);
            }
            catch (JsonReaderException)
            {
                return new GitHubToken(string.Empty, string.Empty, string.Empty);
            }
        }

        public async Task ActivateDemoUser()
        {
            await LogOut().ConfigureAwait(false);

            Name = DemoDataConstants.Name;
            Alias = DemoDataConstants.Alias;
            AvatarUrl = BaseTheme.GetGitTrendsImageSource();

            OnDemoUserActivated();
        }

        public async Task<string> GetGitHubLoginUrl()
        {
            MostRecentSessionId = Guid.NewGuid().ToString();

            var clientIdDTO = await _azureFunctionsApiService.GetGitHubClientId().ConfigureAwait(false);

            return $"{GitHubConstants.GitHubBaseUrl}/login/oauth/authorize?client_id={clientIdDTO.ClientId}&scope=public_repo%20read:user&state={MostRecentSessionId}";
        }

        public async Task AuthorizeSession(Uri callbackUri)
        {
            OnAuthorizeSessionStarted();

            var code = HttpUtility.ParseQueryString(callbackUri.Query).Get("code");
            var state = HttpUtility.ParseQueryString(callbackUri.Query).Get("state");

            try
            {
                if (string.IsNullOrEmpty(code))
                    throw new Exception("Invalid Authorization Code");

                if (state != MostRecentSessionId)
                    throw new InvalidOperationException("Invalid SessionId");
                else
                    MostRecentSessionId = string.Empty;

                var generateTokenDTO = new GenerateTokenDTO(code, state);
                var token = await _azureFunctionsApiService.GenerateGitTrendsOAuthToken(generateTokenDTO).ConfigureAwait(false);

                await SaveGitHubToken(token).ConfigureAwait(false);

                var (login, name, avatarUri) = await _gitHubGraphQLApiService.GetCurrentUserInfo().ConfigureAwait(false);

                Alias = login;
                Name = name;
                AvatarUrl = avatarUri.ToString();

                OnAuthorizeSessionCompleted(true);
            }
            catch (Exception e)
            {
                _analyticsService.Report(e);

                OnAuthorizeSessionCompleted(false);
                throw;
            }
        }

        public async Task LogOut()
        {
            Alias = string.Empty;
            Name = string.Empty;
            AvatarUrl = string.Empty;

            await Task.WhenAll(InvalidateToken(), _repositoryDatabase.DeleteAllData()).ConfigureAwait(false);

            OnLoggedOut();
        }

        internal Task SaveGitHubToken(GitHubToken token)
        {
            if (token is null)
                throw new ArgumentNullException(nameof(token));

            if (token.AccessToken is null)
                throw new ArgumentNullException(nameof(token.AccessToken));

            var serializedToken = JsonConvert.SerializeObject(token);
            return SecureStorage.SetAsync(_oauthTokenKey, serializedToken);
        }

        Task InvalidateToken() => SecureStorage.SetAsync(_oauthTokenKey, string.Empty);

        async void HandlePreferenceChanged(object sender, PreferredTheme e)
        {
            //Ensure the Demo User Alias matches the PreferredTheme
            if (Alias is DemoDataConstants.Alias)
            {
                await ActivateDemoUser();
            }
        }

        void OnAuthorizeSessionCompleted(bool isSessionAuthorized) =>
           _authorizeSessionCompletedEventManager.HandleEvent(this, new AuthorizeSessionCompletedEventArgs(isSessionAuthorized), nameof(AuthorizeSessionCompleted));

        void OnAuthorizeSessionStarted() =>
           _authorizeSessionStartedEventManager.HandleEvent(this, EventArgs.Empty, nameof(AuthorizeSessionStarted));

        void OnLoggedOut() => _loggedOuteventManager.HandleEvent(this, EventArgs.Empty, nameof(LoggedOut));

        void OnDemoUserActivated() => _demoUserActivatedEventManager.HandleEvent(this, EventArgs.Empty, nameof(DemoUserActivated));
    }
}
