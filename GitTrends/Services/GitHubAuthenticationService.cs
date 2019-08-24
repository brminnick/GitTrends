using System;
using System.Threading.Tasks;
using System.Web;
using AsyncAwaitBestPractices;
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

        readonly AzureFunctionsApiService _azureFunctionsApiService;
        readonly GitHubGraphQLApiService _gitHubGraphQLApiService;
        readonly RepositoryDatabase _repositoryDatabase;

        string _sessionId = string.Empty;

        public GitHubAuthenticationService(AzureFunctionsApiService azureFunctionsApiService,
                                            GitHubGraphQLApiService gitHubGraphQLApiService,
                                            RepositoryDatabase repositoryDatabase)
        {
            _azureFunctionsApiService = azureFunctionsApiService;
            _gitHubGraphQLApiService = gitHubGraphQLApiService;
            _repositoryDatabase = repositoryDatabase;
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

        public bool IsAuthenticated => !string.IsNullOrWhiteSpace(Name);

        public string Alias
        {
            get => Preferences.Get(nameof(Alias), string.Empty);
            set => Preferences.Set(nameof(Alias), value);
        }

        public string Name
        {
            get => Preferences.Get(nameof(Name), string.Empty);
            set => Preferences.Set(nameof(Name), value);
        }

        public string AvatarUrl
        {
            get => Preferences.Get(nameof(AvatarUrl), string.Empty);
            set => Preferences.Set(nameof(AvatarUrl), value);
        }

        public async Task<string> GetGitHubLoginUrl()
        {
            _sessionId = Guid.NewGuid().ToString();

            var clientIdDTO = await _azureFunctionsApiService.GetGitHubClientId().ConfigureAwait(false);

            return $"{GitHubConstants.GitHubAuthBaseUrl}/login/oauth/authorize?client_id={clientIdDTO.ClientId}&scope=repo%20read:user&state={_sessionId}";
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

                if (state != _sessionId)
                    throw new InvalidOperationException("Invalid SessionId");

                _sessionId = string.Empty;

                var generateTokenDTO = new GenerateTokenDTO(code, state);
                var token = await _azureFunctionsApiService.GenerateGitTrendsOAuthToken(generateTokenDTO).ConfigureAwait(false);

                await SaveGitHubToken(token).ConfigureAwait(false);

                var (login, name, avatarUri) = await _gitHubGraphQLApiService.GetCurrentUserInfo().ConfigureAwait(false);

                Alias = login;
                Name = name;
                AvatarUrl = avatarUri.ToString();

                OnAuthorizeSessionCompleted(true);
            }
            catch
            {
                OnAuthorizeSessionCompleted(false);
                throw;
            }
        }

        public static async Task<GitHubToken> GetGitHubToken()
        {
            var serializedToken = await SecureStorage.GetAsync(_oauthTokenKey).ConfigureAwait(false);

            try
            {
                var token = await Task.Run(() => JsonConvert.DeserializeObject<GitHubToken>(serializedToken)).ConfigureAwait(false);

                if (token is null)
                    return new GitHubToken(string.Empty, string.Empty, string.Empty);

                return token;
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

        public Task LogOut()
        {
            Alias = string.Empty;
            Name = string.Empty;
            AvatarUrl = string.Empty;

            return Task.WhenAll(InvalidateToken(), _repositoryDatabase.DeleteAllData());
        }

        Task InvalidateToken() => SecureStorage.SetAsync(_oauthTokenKey, string.Empty);

        async Task SaveGitHubToken(GitHubToken token)
        {
            if (token is null)
                throw new ArgumentNullException(nameof(token));

            if (token.AccessToken is null)
                throw new ArgumentNullException(nameof(token.AccessToken));

            var serializedToken = await Task.Run(() => JsonConvert.SerializeObject(token)).ConfigureAwait(false);
            await SecureStorage.SetAsync(_oauthTokenKey, serializedToken).ConfigureAwait(false);
        }

        void OnAuthorizeSessionCompleted(bool isSessionAuthorized) =>
           _authorizeSessionCompletedEventManager.HandleEvent(null, new AuthorizeSessionCompletedEventArgs(isSessionAuthorized), nameof(AuthorizeSessionCompleted));

        void OnAuthorizeSessionStarted() =>
           _authorizeSessionStartedEventManager.HandleEvent(null, EventArgs.Empty, nameof(AuthorizeSessionStarted));
    }
}
