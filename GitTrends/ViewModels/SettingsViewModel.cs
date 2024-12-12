using AsyncAwaitBestPractices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GitTrends.Common;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using Shiny;

namespace GitTrends;

public partial class SettingsViewModel : GitHubAuthenticationViewModel
{
	public const string LoggedOutGitHubAliasLabelText = "";

	static readonly WeakEventManager<bool> _organizationsCarouselViewVisibilityChangedEventManager = new();
	static readonly WeakEventManager<AccessState?> _setNotificationsPreferenceCompletedEventManager = new();

	readonly ThemeService _themeService;
	readonly LanguageService _languageService;
	readonly IVersionTracking _versionTracking;
	readonly DeepLinkingService _deepLinkingService;
	readonly NotificationService _notificationService;
	readonly AzureFunctionsApiService _azureFunctionsApiService;
	readonly GitTrendsStatisticsService _gitTrendsStatisticsService;
	readonly TrendsChartSettingsService _trendsChartSettingsService;

    public SettingsViewModel(IDispatcher dispatcher,
		ThemeService themeService,
		LanguageService languageService,
		IVersionTracking versionTracking,
		IAnalyticsService analyticsService,
		GitHubUserService gitHubUserService,
		DeepLinkingService deepLinkingService,
		NotificationService notificationService,
		AzureFunctionsApiService azureFunctionsApiService,
		GitTrendsStatisticsService gitTrendsStatisticsService,
		TrendsChartSettingsService trendsChartSettingsService,
		GitHubAuthenticationService gitHubAuthenticationService)
		: base(dispatcher, analyticsService, gitHubUserService, deepLinkingService, gitHubAuthenticationService)
	{
		_themeService = themeService;
		_versionTracking = versionTracking;
		_languageService = languageService;
		_deepLinkingService = deepLinkingService;
		_notificationService = notificationService;
		_azureFunctionsApiService = azureFunctionsApiService;
		_gitTrendsStatisticsService = gitTrendsStatisticsService;
		_trendsChartSettingsService = trendsChartSettingsService;

		App.Resumed += HandleResumed;

		GitHubUserService.NameChanged += HandleNameChanged;
		GitHubUserService.AliasChanged += HandleAliasChanged;
		GitHubUserService.AvatarUrlChanged += HandleAvatarUrlChanged;

		ThemeService.PreferenceChanged += HandlePreferenceChanged;
		LanguageService.PreferredLanguageChanged += HandlePreferredLanguageChanged;
		GitHubAuthenticationService.AuthorizeSessionCompleted += HandleAuthorizeSessionCompleted;

		InitializeText(themeService, trendsChartSettingsService);

		LanguagePickerSelectedIndex = CultureConstants.CulturePickerOptions.Keys.ToList().IndexOf(languageService.PreferredLanguage ?? string.Empty);

		initializeIsRegisterForNotificationsSwitch().SafeFireAndForget();

		async Task initializeIsRegisterForNotificationsSwitch() => IsRegisterForNotificationsSwitchToggled = notificationService.ShouldSendNotifications && await notificationService.AreNotificationsEnabled(CancellationToken.None).ConfigureAwait(false);
	}

	public static event EventHandler<AccessState?> SetNotificationsPreferenceCompleted
	{
		add => _setNotificationsPreferenceCompletedEventManager.AddEventHandler(value);
		remove => _setNotificationsPreferenceCompletedEventManager.RemoveEventHandler(value);
	}

	public static event EventHandler<bool> OrganizationsCarouselViewVisibilityChanged
	{
		add => _organizationsCarouselViewVisibilityChangedEventManager.AddEventHandler(value);
		remove => _organizationsCarouselViewVisibilityChangedEventManager.RemoveEventHandler(value);
	}

	public override bool IsDemoButtonVisible => base.IsDemoButtonVisible && LoginLabelText == GitHubLoginButtonConstants.ConnectToGitHub;

	public bool IsAliasLabelVisible => !IsAuthenticating && LoginLabelText == GitHubLoginButtonConstants.Disconnect;

	public IReadOnlyList<string> LanguagePickerItemsSource { get; } = [.. CultureConstants.CulturePickerOptions.Values];
	
	[ObservableProperty]
	public partial bool IsRegisterForNotificationsSwitchToggled { get; internal set; }
	
	[ObservableProperty]
	public partial int PreferredChartsSelectedIndex { get; internal set; }
	
	[ObservableProperty]
	public partial int ThemePickerSelectedIndex { get; internal set; }
	
	[ObservableProperty]
	public partial int LanguagePickerSelectedIndex { get; internal set; }
	
	[ObservableProperty]
	public partial IReadOnlyList<string> ThemePickerItemsSource { get; private set; } = [];

	[ObservableProperty]
	public partial IReadOnlyList<string> PreferredChartsItemsSource { get; private set; } = [];
	
	[ObservableProperty]
    public partial string TitleText { get; private set; } = string.Empty;
    
    [ObservableProperty]
    public partial string AboutLabelText { get; private set; } = string.Empty;
    
    [ObservableProperty]
    public partial string ThemeLabelText { get; private set; } = string.Empty;
    
    [ObservableProperty]
    public partial string LanguageLabelText { get; private set; } = string.Empty;
    
    [ObservableProperty]
    public partial string TryDemoButtonText { get; private set; } = string.Empty;
    
    [ObservableProperty]
    public partial string CopyrightLabelText { get; private set; } = string.Empty;
    
    [ObservableProperty]
    public partial string GitHubAvatarImageSource { get; private set; } = string.Empty;
    
    [ObservableProperty]
    public partial string GitHubAliasLabelText { get; private set; } = string.Empty;
    
    [ObservableProperty]
    public partial string PreferredChartsLabelText { get; private set; } = string.Empty;
    
    [ObservableProperty]
    public partial string RegisterForNotificationsLabelText { get; private set; } = string.Empty;
    
    [ObservableProperty]
    public partial string ShouldIncludeOrganizationsLabelText { get; private set; } = string.Empty;
    
    [ObservableProperty]
    public partial string GitHubNameLabelText { get; private set; } = string.Empty;
    
    [ObservableProperty, NotifyPropertyChangedFor(nameof(IsDemoButtonVisible)), NotifyPropertyChangedFor(nameof(IsAliasLabelVisible))]
    public partial string LoginLabelText { get; private set; } = string.Empty;
    
    [ObservableProperty]
    public partial bool IsRegisterForNotificationsSwitchEnabled { get; private set; } = true;
    
    [ObservableProperty, NotifyPropertyChangedFor(nameof(IsShouldIncludeOrganizationsSwitchToggled))]
    public partial bool IsShouldIncludeOrganizationsSwitchEnabled { get; private set; }

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

	public bool IsShouldIncludeOrganizationsSwitchToggled
	{
		get => GitHubUserService.ShouldIncludeOrganizations && IsShouldIncludeOrganizationsSwitchEnabled;
		set
		{
			if (IsShouldIncludeOrganizationsSwitchToggled != value)
			{
				AnalyticsService.Track("Should Include Organizations Switch Toggled", nameof(IsShouldIncludeOrganizationsSwitchToggled), value.ToString());

				GitHubUserService.ShouldIncludeOrganizations = value;
				OnPropertyChanged();

				OnOrganizationsCarouselViewVisibilityChanged(value);
			}
		}
	}

	protected override async void NotifyIsAuthenticatingPropertyChanged()
	{
		base.NotifyIsAuthenticatingPropertyChanged();

		OnPropertyChanged(nameof(IsAliasLabelVisible));
		await Dispatcher.DispatchAsync(GitHubUserViewTappedCommand.NotifyCanExecuteChanged).ConfigureAwait(false);
	}

	protected override async Task HandleConnectToGitHubButton((CancellationToken cancellationToken, BrowserLaunchOptions? browserLaunchOptions) parameter)
	{
		AnalyticsService.Track($"{nameof(SettingsViewModel)}.{nameof(HandleConnectToGitHubButton)}", nameof(GitHubUserService.IsAuthenticated), GitHubUserService.IsAuthenticated.ToString());

		if (GitHubUserService.IsAuthenticated || GitHubUserService.IsDemoUser)
		{
			await GitHubAuthenticationService.LogOut(parameter.cancellationToken).ConfigureAwait(false);

			SetGitHubValues();
		}
		else
		{
			await base.HandleConnectToGitHubButton(parameter).ConfigureAwait(false);
		}
	}

	protected override async Task HandleDemoButtonTapped(string? buttonText, CancellationToken token)
	{
		var demoUserActivatedTCS = new TaskCompletionSource();
		GitHubAuthenticationService.DemoUserActivated += HandleDemoUserActivated;

		try
		{
			await base.HandleDemoButtonTapped(buttonText, token).ConfigureAwait(false);

			AnalyticsService.Track("Settings Try Demo Button Tapped");
			await GitHubAuthenticationService.ActivateDemoUser(token).ConfigureAwait(false);
			await demoUserActivatedTCS.Task.WaitAsync(token).ConfigureAwait(false);
		}
		finally
		{
			IsAuthenticating = false;
		}

		void HandleDemoUserActivated(object? sender, EventArgs e)
		{
			GitHubAuthenticationService.DemoUserActivated -= HandleDemoUserActivated;

			SetGitHubValues();

			demoUserActivatedTCS.SetResult();
		}
	}

	bool CanOpenGitTrendsOrganizationBrowserCommandExecute() => _gitTrendsStatisticsService.EnableOrganizationsUri is not null;

	[RelayCommand(CanExecute = nameof(CanOpenGitTrendsOrganizationBrowserCommandExecute))]
	Task OpenGitTrendsOrganizationBrowser(CancellationToken token)
	{
		if (_gitTrendsStatisticsService.EnableOrganizationsUri is null)
			throw new InvalidOperationException($"{nameof(GitTrendsStatisticsService)}.{nameof(GitTrendsStatisticsService.EnableOrganizationsUri)} Must Be Initialized");

		AnalyticsService.Track("Manage Organizations Button Tapped");

		OnOrganizationsCarouselViewVisibilityChanged(false);

		return _deepLinkingService.OpenBrowser(_gitTrendsStatisticsService.EnableOrganizationsUri, token);
	}

	void ExecutePreferredChartsChangedCommand(TrendsChartOption trendsChartOption)
	{
		_trendsChartSettingsService.CurrentTrendsChartOption = trendsChartOption;
		PreferredChartsSelectedIndex = (int)trendsChartOption;
	}

	async Task SetNotificationsPreference(bool isNotificationsEnabled, CancellationToken token)
	{
		AccessState? result = null;
		IsRegisterForNotificationsSwitchEnabled = false;

		try
		{
			if (isNotificationsEnabled)
			{
				result = await _notificationService.Register(true, token).ConfigureAwait(false);
				AnalyticsService.Track("Register for Notifications Switch Toggled", new Dictionary<string, string>
				{
					{
						nameof(isNotificationsEnabled), isNotificationsEnabled.ToString()
					},
					{
						nameof(result), result.ToString() ?? "null"
					}
				});
			}
			else
			{
				_notificationService.UnRegister();
				AnalyticsService.Track("Register for Notifications Switch Toggled", new Dictionary<string, string>
				{
					{
						nameof(isNotificationsEnabled), isNotificationsEnabled.ToString()
					},
					{
						nameof(result), result?.ToString() ?? "null"
					}
				});
			}
		}
		finally
		{
			IsRegisterForNotificationsSwitchEnabled = true;
			OnSetNotificationsCompleted(result);
		}
	}

	[RelayCommand]
	Task CopyrightLabelTapped(CancellationToken token)
	{
		AnalyticsService.Track("CreatedBy Label Tapped");
		return _deepLinkingService.OpenApp("twitter://user?id=3418408341", "https://twitter.com/intent/user?user_id=3418408341", token);
	}

	void HandleNameChanged(object? sender, string e) => SetGitHubValues();
	void HandleAliasChanged(object? sender, string e) => SetGitHubValues();
	void HandleAvatarUrlChanged(object? sender, string e) => SetGitHubValues();
	void HandlePreferredLanguageChanged(object? sender, string? e) => InitializeText(_themeService, _trendsChartSettingsService);
	void HandleAuthorizeSessionCompleted(object? sender, AuthorizeSessionCompletedEventArgs e) => SetGitHubValues();
	void HandlePreferenceChanged(object? sender, PreferredTheme e) => UpdateGitHubAvatarImage();

	async void HandleResumed(object? sender, EventArgs e)
	{
		IsRegisterForNotificationsSwitchToggled = _notificationService.ShouldSendNotifications && await _notificationService.AreNotificationsEnabled(CancellationToken.None).ConfigureAwait(false);
		IsRegisterForNotificationsSwitchEnabled = true;
	}

	void InitializeText(in ThemeService themeService, in TrendsChartSettingsService trendsChartSettingsService)
	{
		//Changing the Picker.ItemSource resets the Selected Index to -1
		var originalThemePickerIndex = (int)themeService.Preference;
		var originalPreferredChartsIndex = (int)trendsChartSettingsService.CurrentTrendsChartOption;

		TitleText = PageTitles.SettingsPage;
		AboutLabelText = PageTitles.AboutPage;
		ThemeLabelText = SettingsPageConstants.Theme;
		LanguageLabelText = SettingsPageConstants.Language;
		TryDemoButtonText = GitHubLoginButtonConstants.TryDemo;
		CopyrightLabelText = $"{getVersionNumberText(_versionTracking)}\n{SettingsPageConstants.CreatedBy}";
		PreferredChartsLabelText = SettingsPageConstants.PreferredChartSettingsLabelText;
		RegisterForNotificationsLabelText = SettingsPageConstants.RegisterForNotifications;
		ShouldIncludeOrganizationsLabelText = SettingsPageConstants.IncludeOrganizations;

		ThemePickerItemsSource = [.. ThemePickerConstants.ThemePickerTitles.Values];
		PreferredChartsItemsSource = [.. TrendsChartConstants.TrendsChartTitles.Values];

		ThemePickerSelectedIndex = originalThemePickerIndex;
		PreferredChartsSelectedIndex = originalPreferredChartsIndex;

		SetGitHubValues();

		static string getVersionNumberText(IVersionTracking versionTracking)
		{
#if DEBUG
			return $"v{versionTracking.CurrentVersion} (Debug)";
#elif RELEASE
			return $"v{versionTracking.CurrentVersion} (Release)";
#else
			return $"v{versionTracking.CurrentVersion}";
#endif
		}
	}

	void UpdateGitHubAvatarImage()
	{
		if (!GitHubUserService.IsAuthenticated)
			GitHubAvatarImageSource = BaseTheme.GetDefaultProfileImageSource();
		else if (GitHubUserService.Alias == DemoUserConstants.Alias)
			GitHubAvatarImageSource = BaseTheme.GetGitTrendsImageSource();
	}

	void SetGitHubValues()
	{
		LoginLabelText = GitHubUserService.IsAuthenticated || GitHubUserService.IsDemoUser ? GitHubLoginButtonConstants.Disconnect : GitHubLoginButtonConstants.ConnectToGitHub;

		GitHubAliasLabelText = GitHubUserService.IsAuthenticated || GitHubUserService.IsDemoUser
			? $"@{GitHubUserService.Alias}"
			: LoggedOutGitHubAliasLabelText;

		GitHubNameLabelText = GitHubUserService.IsAuthenticated || GitHubUserService.IsDemoUser
			? GitHubUserService.Name
			: GitHubLoginButtonConstants.NotLoggedIn;

		GitHubAvatarImageSource = GitHubUserService.IsAuthenticated || GitHubUserService.IsDemoUser
			? GitHubUserService.AvatarUrl
			: BaseTheme.GetDefaultProfileImageSource();

		IsShouldIncludeOrganizationsSwitchEnabled = GitHubUserService.IsAuthenticated;
	}

	[RelayCommand(CanExecute = nameof(IsNotAuthenticating))]
	async Task GitHubUserViewTapped(CancellationToken token)
	{
		if (GitHubUserService.IsAuthenticated || GitHubUserService.IsDemoUser)
		{
			var alias = GitHubUserService.IsDemoUser ? nameof(GitTrends) : GitHubUserService.Alias;
			AnalyticsService.Track("Alias Label Tapped", "Alias", alias);

			using var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token, new CancellationTokenSource(TimeSpan.FromSeconds(3)).Token);
			await _deepLinkingService.OpenApp(GitHubConstants.AppScheme, $"{GitHubConstants.GitHubBaseUrl}/{alias}", $"{GitHubConstants.GitHubBaseUrl}/{alias}", cancellationTokenSource.Token).ConfigureAwait(false);
		}
		else
		{
			var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token, new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token);
			await HandleConnectToGitHubButton((cancellationTokenSource.Token, null)).ConfigureAwait(false);
		}
	}

	void OnSetNotificationsCompleted(AccessState? accessState) => _setNotificationsPreferenceCompletedEventManager.RaiseEvent(this, accessState, nameof(SetNotificationsPreferenceCompleted));
	void OnOrganizationsCarouselViewVisibilityChanged(bool isVisible) => _organizationsCarouselViewVisibilityChangedEventManager.RaiseEvent(this, isVisible, nameof(OrganizationsCarouselViewVisibilityChanged));

	partial void OnThemePickerSelectedIndexChanged(int value)
	{
		if (Enum.IsDefined((PreferredTheme)value))
			_themeService.Preference = (PreferredTheme)value;
	}

	partial void OnPreferredChartsSelectedIndexChanged(int value)
	{
		if (Enum.IsDefined((TrendsChartOption)value))
			_trendsChartSettingsService.CurrentTrendsChartOption = (TrendsChartOption)value;
	}

	partial void OnLanguagePickerSelectedIndexChanged(int value) => _languageService.PreferredLanguage = CultureConstants.CulturePickerOptions.Skip(value).First().Key;
	async partial void OnIsRegisterForNotificationsSwitchToggledChanged(bool value) => await SetNotificationsPreference(value, CancellationToken.None).ConfigureAwait(false);
}