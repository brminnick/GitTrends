using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using GitTrends.Shared;
using Shiny;
using Xamarin.Essentials.Interfaces;

namespace GitTrends
{
	public partial class SettingsViewModel : GitHubAuthenticationViewModel
	{
		static readonly WeakEventManager<bool> _organizationsCarouselViewVisiblilityChangedEventManager = new();
		static readonly WeakEventManager<AccessState?> _setNotificationsPreferenceCompletedEventManager = new();

		readonly ThemeService _themeService;
		readonly LanguageService _languageService;
		readonly IVersionTracking _versionTracking;
		readonly DeepLinkingService _deepLinkingService;
		readonly NotificationService _notificationService;
		readonly AzureFunctionsApiService _azureFunctionsApiService;
		readonly GitTrendsStatisticsService _gitTrendsStatisticsService;
		readonly TrendsChartSettingsService _trendsChartSettingsService;

		[ObservableProperty]
		IReadOnlyList<string> _themePickerItemsSource = [], _preferredChartsItemsSource = [];

		[ObservableProperty]
		string _titleText = string.Empty, _aboutLabelText = string.Empty, _themeLabelText = string.Empty,
			_languageLabelText = string.Empty, _tryDemoButtonText = string.Empty, _copyrightLabelText = string.Empty,
			_gitHubAvatarImageSource = string.Empty, _gitHubAliasLabelText = string.Empty, _preferredChartsLabelText = string.Empty,
			_registerForNotificationsLabelText = string.Empty, _shouldIncludeOrganizationsLabelText = string.Empty, _gitHubNameLabelText = string.Empty;

		[ObservableProperty, NotifyPropertyChangedFor(nameof(IsDemoButtonVisible)), NotifyPropertyChangedFor(nameof(IsAliasLabelVisible))]
		string _loginLabelText = string.Empty;

		[ObservableProperty]
		bool _isRegisterForNotificationsSwitchToggled, _isRegisterForNotificationsSwitchEnabled = true;

		[ObservableProperty, NotifyPropertyChangedFor(nameof(IsShouldIncludeOrganizationsSwitchToggled))]
		bool _isShouldIncludeOrganizationsSwitchEnabled;

		[ObservableProperty]
		int _themePickerSelectedIndex, _preferredChartsSelectedIndex, _languagePickerSelectedIndex;

		public SettingsViewModel(IMainThread mainThread,
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
				: base(mainThread, analyticsService, gitHubUserService, deepLinkingService, gitHubAuthenticationService)
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

			async Task initializeIsRegisterForNotificationsSwitch() => IsRegisterForNotificationsSwitchToggled = notificationService.ShouldSendNotifications && await notificationService.AreNotificationsEnabled().ConfigureAwait(false);
		}

		public static event EventHandler<AccessState?> SetNotificationsPreferenceCompleted
		{
			add => _setNotificationsPreferenceCompletedEventManager.AddEventHandler(value);
			remove => _setNotificationsPreferenceCompletedEventManager.RemoveEventHandler(value);
		}

		public static event EventHandler<bool> OrganizationsCarouselViewVisiblilityChanged
		{
			add => _organizationsCarouselViewVisiblilityChangedEventManager.AddEventHandler(value);
			remove => _organizationsCarouselViewVisiblilityChangedEventManager.RemoveEventHandler(value);
		}

		public IReadOnlyList<string> LanguagePickerItemsSource { get; } = CultureConstants.CulturePickerOptions.Values.ToList();

		public bool IsAliasLabelVisible => !IsAuthenticating && LoginLabelText == GitHubLoginButtonConstants.Disconnect;

		public override bool IsDemoButtonVisible => base.IsDemoButtonVisible && LoginLabelText == GitHubLoginButtonConstants.ConnectToGitHub;

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

					OnOrganizationsCarouselViewVisiblilityChanged(value);
				}
			}
		}

		protected override async void NotifyIsAuthenticatingPropertyChanged()
		{
			base.NotifyIsAuthenticatingPropertyChanged();

			OnPropertyChanged(nameof(IsAliasLabelVisible));
			await MainThread.InvokeOnMainThreadAsync(GitHubUserViewTappedCommand.NotifyCanExecuteChanged).ConfigureAwait(false);
		}

		protected override async Task HandleConnectToGitHubButton((CancellationToken cancellationToken, Xamarin.Essentials.BrowserLaunchOptions? browserLaunchOptions) parameter)
		{
			AnalyticsService.Track($"{nameof(SettingsViewModel)}.{nameof(HandleConnectToGitHubButton)}", nameof(GitHubUserService.IsAuthenticated), GitHubUserService.IsAuthenticated.ToString());

			if (GitHubUserService.IsAuthenticated || GitHubUserService.IsDemoUser)
			{
				await GitHubAuthenticationService.LogOut().ConfigureAwait(false);

				SetGitHubValues();
			}
			else
			{
				await base.HandleConnectToGitHubButton(parameter).ConfigureAwait(false);
			}
		}

		protected override async Task HandleDemoButtonTapped(string? buttonText)
		{
			var demoUserActivatedTCS = new TaskCompletionSource();
			GitHubAuthenticationService.DemoUserActivated += HandleDemoUserActivated;

			try
			{
				await base.HandleDemoButtonTapped(buttonText).ConfigureAwait(false);

				AnalyticsService.Track("Settings Try Demo Button Tapped");
				await GitHubAuthenticationService.ActivateDemoUser().ConfigureAwait(false);
				await demoUserActivatedTCS.Task.WaitAsync(TestCancellationTokenSource.Token).ConfigureAwait(false);
			}
			finally
			{
				IsAuthenticating = false;
			}

			void HandleDemoUserActivated(object sender, EventArgs e)
			{
				GitHubAuthenticationService.DemoUserActivated -= HandleDemoUserActivated;

				SetGitHubValues();

				demoUserActivatedTCS.SetResult(null);
			}
		}

		bool CanOpenGitTrendsOrganizationBrowserCommandExecute() => _gitTrendsStatisticsService.EnableOrganizationsUri is not null;

		[RelayCommand(CanExecute = nameof(CanOpenGitTrendsOrganizationBrowserCommandExecute))]
		Task OpenGitTrendsOrganizationBrowser()
		{
			if (_gitTrendsStatisticsService.EnableOrganizationsUri is null)
				throw new InvalidOperationException($"{nameof(GitTrendsStatisticsService)}.{nameof(GitTrendsStatisticsService.EnableOrganizationsUri)} Must Be Initialized");

			AnalyticsService.Track("Manage Organizations Button Tapped");

			OnOrganizationsCarouselViewVisiblilityChanged(false);

			return _deepLinkingService.OpenBrowser(_gitTrendsStatisticsService.EnableOrganizationsUri);
		}

		void ExecutePreferredChartsChangedCommand(TrendsChartOption trendsChartOption)
		{
			_trendsChartSettingsService.CurrentTrendsChartOption = trendsChartOption;
			PreferredChartsSelectedIndex = (int)trendsChartOption;
		}

		async Task SetNotificationsPreference(bool isNotificationsEnabled)
		{
			AccessState? result = null;
			IsRegisterForNotificationsSwitchEnabled = false;

			try
			{
				if (isNotificationsEnabled)
				{
					result = await _notificationService.Register(true).ConfigureAwait(false);
					AnalyticsService.Track("Register for Notifications Switch Toggled", new Dictionary<string, string>
					{
						{ nameof(isNotificationsEnabled), isNotificationsEnabled.ToString() },
						{ nameof(result), result.ToString() }
					});
				}
				else
				{
					_notificationService.UnRegister();
					AnalyticsService.Track("Register for Notifications Switch Toggled", new Dictionary<string, string>
					{
						{ nameof(isNotificationsEnabled), isNotificationsEnabled.ToString() },
						{ nameof(result), result.ToString() }
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
		Task CopyrightLabelTapped()
		{
			AnalyticsService.Track("CreatedBy Label Tapped");
			return _deepLinkingService.OpenApp("twitter://user?id=3418408341", "https://twitter.com/intent/user?user_id=3418408341");
		}

		void HandleNameChanged(object sender, string e) => SetGitHubValues();
		void HandleAliasChanged(object sender, string e) => SetGitHubValues();
		void HandleAvatarUrlChanged(object sender, string e) => SetGitHubValues();
		void HandlePreferredLanguageChanged(object sender, string? e) => InitializeText(_themeService, _trendsChartSettingsService);
		void HandleAuthorizeSessionCompleted(object sender, AuthorizeSessionCompletedEventArgs e) => SetGitHubValues();
		void HandlePreferenceChanged(object sender, PreferredTheme e) => UpdateGitHubAvatarImage();

		async void HandleResumed(object sender, EventArgs e)
		{
			IsRegisterForNotificationsSwitchToggled = _notificationService.ShouldSendNotifications && await _notificationService.AreNotificationsEnabled().ConfigureAwait(false);
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

			ThemePickerItemsSource = ThemePickerConstants.ThemePickerTitles.Values.ToList();
			PreferredChartsItemsSource = TrendsChartConstants.TrendsChartTitles.Values.ToList();

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

			GitHubAliasLabelText = GitHubUserService.IsAuthenticated || GitHubUserService.IsDemoUser ? $"@{GitHubUserService.Alias}" : string.Empty;
			GitHubNameLabelText = GitHubUserService.IsAuthenticated || GitHubUserService.IsDemoUser ? GitHubUserService.Name : GitHubLoginButtonConstants.NotLoggedIn;
			GitHubAvatarImageSource = GitHubUserService.IsAuthenticated || GitHubUserService.IsDemoUser ? GitHubUserService.AvatarUrl : BaseTheme.GetDefaultProfileImageSource();

			IsShouldIncludeOrganizationsSwitchEnabled = GitHubUserService.IsAuthenticated;
		}

		[RelayCommand(CanExecute = nameof(IsNotAuthenticating))]
		Task GitHubUserViewTapped()
		{
			if (GitHubUserService.IsAuthenticated || GitHubUserService.IsDemoUser)
			{
				string alias = GitHubUserService.IsDemoUser ? nameof(GitTrends) : GitHubUserService.Alias;
				AnalyticsService.Track("Alias Label Tapped", "Alias", alias);

				return _deepLinkingService.OpenApp(GitHubConstants.AppScheme, $"{GitHubConstants.GitHubBaseUrl}/{alias}", $"{GitHubConstants.GitHubBaseUrl}/{alias}");
			}
			else
			{
				var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
				return HandleConnectToGitHubButton((cancellationTokenSource.Token, null));
			}
		}

		void OnSetNotificationsCompleted(AccessState? accessState) => _setNotificationsPreferenceCompletedEventManager.RaiseEvent(this, accessState, nameof(SetNotificationsPreferenceCompleted));
		void OnOrganizationsCarouselViewVisiblilityChanged(bool isVisible) => _organizationsCarouselViewVisiblilityChangedEventManager.RaiseEvent(this, isVisible, nameof(OrganizationsCarouselViewVisiblilityChanged));

		partial void OnThemePickerSelectedIndexChanged(int value)
		{
			if (Enum.IsDefined(typeof(PreferredTheme), value))
				_themeService.Preference = (PreferredTheme)value;
		}

		partial void OnPreferredChartsSelectedIndexChanged(int value)
		{
			if (Enum.IsDefined(typeof(TrendsChartOption), value))
				_trendsChartSettingsService.CurrentTrendsChartOption = (TrendsChartOption)value;
		}

		partial void OnLanguagePickerSelectedIndexChanged(int value) => _languageService.PreferredLanguage = CultureConstants.CulturePickerOptions.Skip(value).First().Key;
		async partial void OnIsRegisterForNotificationsSwitchToggledChanged(bool value) => await SetNotificationsPreference(value).ConfigureAwait(false);
	}
}