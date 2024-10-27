namespace GitTrends;

public class FirstRunService
{
	readonly IPreferences _preferences;

	public FirstRunService(IPreferences preferences)
	{
		_preferences = preferences;

		GitHubAuthenticationService.AuthorizeSessionCompleted += HandleAuthorizeSessionCompleted;
		GitHubAuthenticationService.DemoUserActivated += HandleDemoUserActivated;
	}

	public bool IsFirstRun
	{
		get => _preferences.Get(nameof(IsFirstRun), true);
		private set => _preferences.Set(nameof(IsFirstRun), value);
	}

	void HandleDemoUserActivated(object? sender, EventArgs e) => IsFirstRun = false;
	void HandleAuthorizeSessionCompleted(object? sender, AuthorizeSessionCompletedEventArgs e) => IsFirstRun = false;

#if !AppStore
	void HandlePagePopped(object? sender, EventArgs e) => IsFirstRun = false;
#endif
}