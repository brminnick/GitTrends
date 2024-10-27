using System.Web;
using AsyncAwaitBestPractices;
using GitTrends.Common;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;

namespace GitTrends;

public class GitHubAuthenticationService
{
	static readonly AsyncAwaitBestPractices.WeakEventManager _loggedOuteventManager = new();
	static readonly AsyncAwaitBestPractices.WeakEventManager _demoUserActivatedEventManager = new();
	static readonly AsyncAwaitBestPractices.WeakEventManager _authorizeSessionStartedEventManager = new();
	static readonly WeakEventManager<AuthorizeSessionCompletedEventArgs> _authorizeSessionCompletedEventManager = new();

	readonly IPreferences _preferences;
	readonly IAnalyticsService _analyticsService;
	readonly GitHubUserService _gitHubUserService;
	readonly RepositoryDatabase _repositoryDatabase;
	readonly GitHubGraphQLApiService _gitHubGraphQLApiService;
	readonly AzureFunctionsApiService _azureFunctionsApiService;
	readonly GitTrendsStatisticsService _gitTrendsStatisticsService;

	public GitHubAuthenticationService(IPreferences preferences,
										IAnalyticsService analyticsService,
										GitHubUserService gitHubUserService,
										RepositoryDatabase repositoryDatabase,
										GitHubGraphQLApiService gitHubGraphQLApiService,
										AzureFunctionsApiService azureFunctionsApiService,
										GitTrendsStatisticsService gitTrendsStatisticsService)
	{
		_preferences = preferences;
		_analyticsService = analyticsService;
		_gitHubUserService = gitHubUserService;
		_repositoryDatabase = repositoryDatabase;
		_gitHubGraphQLApiService = gitHubGraphQLApiService;
		_azureFunctionsApiService = azureFunctionsApiService;
		_gitTrendsStatisticsService = gitTrendsStatisticsService;

		ThemeService.PreferenceChanged += HandlePreferenceChanged;
		LanguageService.PreferredLanguageChanged += HandlePreferredLanguageChanged;
	}

	public static event EventHandler AuthorizeSessionStarted
	{
		add => _authorizeSessionStartedEventManager.AddEventHandler(value);
		remove => _authorizeSessionStartedEventManager.RemoveEventHandler(value);
	}

	public static event EventHandler<AuthorizeSessionCompletedEventArgs> AuthorizeSessionCompleted
	{
		add => _authorizeSessionCompletedEventManager.AddEventHandler(value);
		remove => _authorizeSessionCompletedEventManager.RemoveEventHandler(value);
	}

	public static event EventHandler DemoUserActivated
	{
		add => _demoUserActivatedEventManager.AddEventHandler(value);
		remove => _demoUserActivatedEventManager.RemoveEventHandler(value);
	}

	public static event EventHandler LoggedOut
	{
		add => _loggedOuteventManager.AddEventHandler(value);
		remove => _loggedOuteventManager.RemoveEventHandler(value);
	}

	string MostRecentSessionId
	{
		get => _preferences.Get(nameof(MostRecentSessionId), string.Empty);
		set => _preferences.Set(nameof(MostRecentSessionId), value);
	}

	public async Task ActivateDemoUser(CancellationToken token)
	{
		await LogOut(token).ConfigureAwait(false);

		SetDemoUserUserValues();

		OnDemoUserActivated();
	}

	public string GetGitHubLoginUrl()
	{
		MostRecentSessionId = Guid.NewGuid().ToString();

		return $"{GitHubConstants.GitHubBaseUrl}/login/oauth/authorize?client_id={_gitTrendsStatisticsService.ClientId}&scope={GitHubConstants.OAuthScope}&state={MostRecentSessionId}";
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

			_gitHubUserService.Name = name;
			_gitHubUserService.Alias = login;
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

	public async Task LogOut(CancellationToken token)
	{
		_gitHubUserService.Alias = string.Empty;
		_gitHubUserService.Name = string.Empty;
		_gitHubUserService.AvatarUrl = string.Empty;

		_gitHubUserService.InvalidateToken();
		await _repositoryDatabase.DeleteAllData(token).ConfigureAwait(false);

		OnLoggedOut();
	}

	void HandlePreferenceChanged(object? sender, PreferredTheme e)
	{
		//Ensure the Demo User Avatar matches the PreferredTheme
		if (_gitHubUserService.IsDemoUser)
			SetDemoUserUserValues();
	}

	void HandlePreferredLanguageChanged(object? sender, string? e)
	{
		//Update Demo User Translations
		if (_gitHubUserService.IsDemoUser)
			SetDemoUserUserValues();
	}

	void SetDemoUserUserValues()
	{
		_gitHubUserService.Name = DemoUserConstants.Name;
		_gitHubUserService.Alias = DemoUserConstants.Alias;
		_gitHubUserService.AvatarUrl = BaseTheme.GetGitTrendsImageSource();
	}

	void OnAuthorizeSessionCompleted(bool isSessionAuthorized) =>
	   _authorizeSessionCompletedEventManager.RaiseEvent(this, new AuthorizeSessionCompletedEventArgs(isSessionAuthorized), nameof(AuthorizeSessionCompleted));

	void OnAuthorizeSessionStarted() =>
	   _authorizeSessionStartedEventManager.RaiseEvent(this, EventArgs.Empty, nameof(AuthorizeSessionStarted));

	void OnLoggedOut() => _loggedOuteventManager.RaiseEvent(this, EventArgs.Empty, nameof(LoggedOut));

	void OnDemoUserActivated() => _demoUserActivatedEventManager.RaiseEvent(this, EventArgs.Empty, nameof(DemoUserActivated));
}