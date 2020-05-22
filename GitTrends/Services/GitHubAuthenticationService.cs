using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using AsyncAwaitBestPractices;
using GitTrends.Mobile.Shared;
using GitTrends.Shared;
using Xamarin.Essentials.Interfaces;

namespace GitTrends
{
    public class GitHubAuthenticationService
    {
        readonly WeakEventManager<AuthorizeSessionCompletedEventArgs> _authorizeSessionCompletedEventManager = new WeakEventManager<AuthorizeSessionCompletedEventArgs>();
        readonly WeakEventManager _authorizeSessionStartedEventManager = new WeakEventManager();
        readonly WeakEventManager _loggedOuteventManager = new WeakEventManager();
        readonly WeakEventManager _demoUserActivatedEventManager = new WeakEventManager();

        readonly AzureFunctionsApiService _azureFunctionsApiService;
        readonly GitHubGraphQLApiService _gitHubGraphQLApiService;
        readonly RepositoryDatabase _repositoryDatabase;
        readonly IAnalyticsService _analyticsService;
        readonly GitHubUserService _gitHubUserService;
        readonly IPreferences _preferences;

        public GitHubAuthenticationService(AzureFunctionsApiService azureFunctionsApiService,
                                            GitHubGraphQLApiService gitHubGraphQLApiService,
                                            RepositoryDatabase repositoryDatabase,
                                            IAnalyticsService analyticsService,
                                            IPreferences preferences,
                                            GitHubUserService gitHubUserService)
        {
            _azureFunctionsApiService = azureFunctionsApiService;
            _gitHubGraphQLApiService = gitHubGraphQLApiService;
            _repositoryDatabase = repositoryDatabase;
            _gitHubUserService = gitHubUserService;
            _analyticsService = analyticsService;
            _preferences = preferences;

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

        string MostRecentSessionId
        {
            get => _preferences.Get(nameof(MostRecentSessionId), string.Empty);
            set => _preferences.Set(nameof(MostRecentSessionId), value);
        }

        public async Task ActivateDemoUser()
        {
            await LogOut().ConfigureAwait(false);

            _gitHubUserService.Name = DemoDataConstants.Name;
            _gitHubUserService.Alias = DemoDataConstants.Alias;
            _gitHubUserService.AvatarUrl = BaseTheme.GetGitTrendsImageSource();

            OnDemoUserActivated();
        }

        public async Task<string> GetGitHubLoginUrl(CancellationToken cancellationToken)
        {
            MostRecentSessionId = Guid.NewGuid().ToString();

            var clientIdDTO = await _azureFunctionsApiService.GetGitHubClientId(cancellationToken).ConfigureAwait(false);

            return $"{GitHubConstants.GitHubBaseUrl}/login/oauth/authorize?client_id={clientIdDTO.ClientId}&scope={GitHubConstants.OAuthScope}&state={MostRecentSessionId}";
        }

        public async Task AuthorizeSession(Uri callbackUri, CancellationToken cancellationToken)
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
                var token = await _azureFunctionsApiService.GenerateGitTrendsOAuthToken(generateTokenDTO, cancellationToken).ConfigureAwait(false);

                await _gitHubUserService.SaveGitHubToken(token).ConfigureAwait(false);

                var (login, name, avatarUri) = await _gitHubGraphQLApiService.GetCurrentUserInfo(cancellationToken).ConfigureAwait(false);

                _gitHubUserService.Alias = login;
                _gitHubUserService.Name = name;
                _gitHubUserService.AvatarUrl = avatarUri.ToString();

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
            _gitHubUserService.Alias = string.Empty;
            _gitHubUserService.Name = string.Empty;
            _gitHubUserService.AvatarUrl = string.Empty;

            _gitHubUserService.InvalidateToken();
            await _repositoryDatabase.DeleteAllData().ConfigureAwait(false);

            OnLoggedOut();
        }

        async void HandlePreferenceChanged(object sender, PreferredTheme e)
        {
            //Ensure the Demo User Alias matches the PreferredTheme
            if (_gitHubUserService.Alias is DemoDataConstants.Alias)
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
