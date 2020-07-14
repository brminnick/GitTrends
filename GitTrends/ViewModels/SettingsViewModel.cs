using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using AsyncAwaitBestPractices;
using AsyncAwaitBestPractices.MVVM;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using GitTrends.Shared;
using Shiny;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;

namespace GitTrends
{
    public class SettingsViewModel : GitHubAuthenticationViewModel
    {
        readonly static WeakEventManager<AccessState?> _setNotificationsPreferenceCompletedEventManager = new WeakEventManager<AccessState?>();

        readonly ThemeService _themeService;
        readonly LanguageService _languageService;
        readonly IVersionTracking _versionTracking;
        readonly DeepLinkingService _deepLinkingService;
        readonly NotificationService _notificationService;
        readonly TrendsChartSettingsService _trendsChartSettingsService;

        string _titleText = string.Empty;
        string _themeLabelText = string.Empty;
        string _gitHubButtonText = string.Empty;
        string _languageLabelText = string.Empty;
        string _tryDemoButtonText = string.Empty;
        string _copyrightLabelText = string.Empty;
        string _gitHubNameLabelText = string.Empty;
        string _gitHubUserImageSource = string.Empty;
        string _gitHubUserNameLabelText = string.Empty;
        string _preferredChartsLabelText = string.Empty;
        string _registerForNotificationsLabelText = string.Empty;

        bool _isRegisterForNotificationsSwitchEnabled = true;
        bool _isRegisterForNotificationsSwitchToggled;

        int _themePickerSelectedIndex;
        int _languagePickerSelectedIndex;
        int _preferredChartsSelectedIndex;

        public SettingsViewModel(IMainThread mainThread,
                                    ThemeService themeService,
                                    LanguageService languageService,
                                    IVersionTracking versionTracking,
                                    IAnalyticsService analyticsService,
                                    GitHubUserService gitHubUserService,
                                    DeepLinkingService deepLinkingService,
                                    NotificationService notificationService,
                                    TrendsChartSettingsService trendsChartSettingsService,
                                    GitHubAuthenticationService gitHubAuthenticationService)
                : base(mainThread, analyticsService, gitHubUserService, deepLinkingService, gitHubAuthenticationService)
        {
            _themeService = themeService;
            _versionTracking = versionTracking;
            _languageService = languageService;
            _deepLinkingService = deepLinkingService;
            _notificationService = notificationService;
            _trendsChartSettingsService = trendsChartSettingsService;

            CopyrightLabelTappedCommand = new AsyncCommand(ExecuteCopyrightLabelTappedCommand);
            GitHubUserViewTappedCommand = new AsyncCommand(ExecuteGitHubUserViewTappedCommand, _ => IsNotAuthenticating);

            App.Resumed += HandleResumed;

            GitHubUserService.NameChanged += HandleNameChanged;
            GitHubUserService.AliasChanged += HandleAliasChanged;
            GitHubUserService.AvatarUrlChanged += HandleAvatarUrlChanged;

            ThemeService.PreferenceChanged += HandlePreferenceChanged;
            LanguageService.PreferredLanguageChanged += HandlePreferredLanguageChanged;
            GitHubAuthenticationService.AuthorizeSessionCompleted += HandleAuthorizeSessionCompleted;

            ThemePickerSelectedIndex = (int)themeService.Preference;
            PreferredChartsSelectedIndex = (int)trendsChartSettingsService.CurrentTrendsChartOption;
            LanguagePickerSelectedIndex = CultureConstants.CulturePickerOptions.Keys.ToList().IndexOf(languageService.PreferredLanguage ?? string.Empty);

            initializeIsRegisterForNotificationsSwitch().SafeFireAndForget();

            InitializeText();

            async Task initializeIsRegisterForNotificationsSwitch() => IsRegisterForNotificationsSwitchToggled = notificationService.ShouldSendNotifications && await notificationService.AreNotificationsEnabled().ConfigureAwait(false);
        }

        public static event EventHandler<AccessState?> SetNotificationsPreferenceCompleted
        {
            add => _setNotificationsPreferenceCompletedEventManager.AddEventHandler(value);
            remove => _setNotificationsPreferenceCompletedEventManager.RemoveEventHandler(value);
        }

        public ICommand CopyrightLabelTappedCommand { get; }
        public IAsyncCommand GitHubUserViewTappedCommand { get; }
        public IReadOnlyList<string> ThemePickerItemsSource { get; } = Enum.GetNames(typeof(PreferredTheme));
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

        public string LoginLabelText
        {
            get => _gitHubButtonText;
            set => SetProperty(ref _gitHubButtonText, value, () =>
            {
                OnPropertyChanged(nameof(IsDemoButtonVisible));
                OnPropertyChanged(nameof(IsAliasLabelVisible));
            });
        }

        public string TryDemoButtonText
        {
            get => _tryDemoButtonText;
            set => SetProperty(ref _tryDemoButtonText, value);
        }

        public string ThemeLabelText
        {
            get => _themeLabelText;
            set => SetProperty(ref _themeLabelText, value);
        }

        public string GitHubAvatarImageSource
        {
            get => _gitHubUserImageSource;
            set => SetProperty(ref _gitHubUserImageSource, value);
        }

        public string PreferredChartsLabelText
        {
            get => _preferredChartsLabelText;
            set => SetProperty(ref _preferredChartsLabelText, value);
        }

        public string CopyrightLabelText
        {
            get => _copyrightLabelText;
            set => SetProperty(ref _copyrightLabelText, value);
        }

        public string GitHubAliasLabelText
        {
            get => _gitHubUserNameLabelText;
            set => SetProperty(ref _gitHubUserNameLabelText, value);
        }

        public string GitHubNameLabelText
        {
            get => _gitHubNameLabelText;
            set => SetProperty(ref _gitHubNameLabelText, value);
        }

        public string LanguageLabelText
        {
            get => _languageLabelText;
            set => SetProperty(ref _languageLabelText, value);
        }

        public int LanguagePickerSelectedIndex
        {
            get => _languagePickerSelectedIndex;
            set => SetProperty(ref _languagePickerSelectedIndex, value, () => _languageService.PreferredLanguage = CultureConstants.CulturePickerOptions.Skip(value).First().Key);
        }

        public string TitleText
        {
            get => _titleText;
            set => SetProperty(ref _titleText, value);
        }

        public int ThemePickerSelectedIndex
        {
            get => _themePickerSelectedIndex;
            set => SetProperty(ref _themePickerSelectedIndex, value, () => _themeService.Preference = (PreferredTheme)value);
        }

        public bool IsRegisterForNotificationsSwitchEnabled
        {
            get => _isRegisterForNotificationsSwitchEnabled;
            set => SetProperty(ref _isRegisterForNotificationsSwitchEnabled, value);
        }

        public bool IsRegisterForNotificationsSwitchToggled
        {
            get => _isRegisterForNotificationsSwitchToggled;
            set => SetProperty(ref _isRegisterForNotificationsSwitchToggled, value, async () => await SetNotificationsPreference(value).ConfigureAwait(false));
        }

        public string RegisterForNotificationsLabelText
        {
            get => _registerForNotificationsLabelText;
            set => SetProperty(ref _registerForNotificationsLabelText, value);
        }

        public int PreferredChartsSelectedIndex
        {
            get => _preferredChartsSelectedIndex;
            set
            {
                _trendsChartSettingsService.CurrentTrendsChartOption = (TrendsChartOption)value;
                SetProperty(ref _preferredChartsSelectedIndex, value);
            }
        }

        protected override async void NotifyIsAuthenticatingPropertyChanged()
        {
            base.NotifyIsAuthenticatingPropertyChanged();
            OnPropertyChanged(nameof(IsAliasLabelVisible));
            await MainThread.InvokeOnMainThreadAsync(GitHubUserViewTappedCommand.RaiseCanExecuteChanged).ConfigureAwait(false);
        }

        protected override async Task ExecuteConnectToGitHubButtonCommand(GitHubAuthenticationService gitHubAuthenticationService, DeepLinkingService deepLinkingService, GitHubUserService gitHubUserService, CancellationToken cancellationToken, Xamarin.Essentials.BrowserLaunchOptions? browserLaunchOptions)
        {
            AnalyticsService.Track("Login Button Tapped", nameof(gitHubUserService.IsAuthenticated), gitHubUserService.IsAuthenticated.ToString());

            if (gitHubUserService.IsAuthenticated)
            {
                await gitHubAuthenticationService.LogOut().ConfigureAwait(false);

                SetGitHubValues();
            }
            else
            {
                await base.ExecuteConnectToGitHubButtonCommand(gitHubAuthenticationService, deepLinkingService, gitHubUserService, cancellationToken, browserLaunchOptions).ConfigureAwait(false);
            }
        }

        protected override async Task ExecuteDemoButtonCommand(string? buttonText)
        {
            try
            {
                await base.ExecuteDemoButtonCommand(buttonText).ConfigureAwait(false);

                AnalyticsService.Track("Settings Try Demo Button Tapped");
                await GitHubAuthenticationService.ActivateDemoUser().ConfigureAwait(false);
                SetGitHubValues();
            }
            finally
            {
                IsAuthenticating = false;
            }
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
                    AnalyticsService.Track("Settings Notification Changed", new Dictionary<string, string>
                    {
                        { nameof(isNotificationsEnabled), isNotificationsEnabled.ToString() },
                        { "Result", result.ToString()}
                    });
                }
                else
                {
                    _notificationService.UnRegister();
                    AnalyticsService.Track("Register for Notifications Switch Toggled", nameof(isNotificationsEnabled), isNotificationsEnabled.ToString());
                }
            }
            finally
            {
                IsRegisterForNotificationsSwitchEnabled = true;
                OnSetNotificationsCompleted(result);
            }
        }

        Task ExecuteCopyrightLabelTappedCommand()
        {
            AnalyticsService.Track("CreatedBy Label Tapped");
            return _deepLinkingService.OpenApp("twitter://user?id=3418408341", "https://twitter.com/intent/user?user_id=3418408341");
        }

        void HandleNameChanged(object sender, string e) => SetGitHubValues();
        void HandleAliasChanged(object sender, string e) => SetGitHubValues();
        void HandleAvatarUrlChanged(object sender, string e) => SetGitHubValues();
        void HandlePreferredLanguageChanged(object sender, string? e) => InitializeText();
        void HandleAuthorizeSessionCompleted(object sender, AuthorizeSessionCompletedEventArgs e) => SetGitHubValues();
        void HandlePreferenceChanged(object sender, PreferredTheme e) => UpdateGitHubAvatarImage();

        async void HandleResumed(object sender, EventArgs e)
        {
            IsRegisterForNotificationsSwitchToggled = _notificationService.ShouldSendNotifications && await _notificationService.AreNotificationsEnabled().ConfigureAwait(false);
            IsRegisterForNotificationsSwitchEnabled = true;
        }

        void InitializeText()
        {
            TitleText = PageTitles.SettingsPage;
            TryDemoButtonText = GitHubLoginButtonConstants.TryDemo;
            CopyrightLabelText = $"{getVersionNumberText(_versionTracking)}\n{SettingsPageConstants.CreatedBy}";
            PreferredChartsLabelText = SettingsPageConstants.PreferredChartSettingsLabelText;

            LanguageLabelText = SettingsPageConstants.Language;

            RegisterForNotificationsLabelText = SettingsPageConstants.RegisterForNotifications;

            ThemeLabelText = SettingsPageConstants.Theme;

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
            GitHubAliasLabelText = GitHubUserService.IsAuthenticated ? $"@{GitHubUserService.Alias}" : string.Empty;
            GitHubNameLabelText = GitHubUserService.IsAuthenticated ? GitHubUserService.Name : GitHubLoginButtonConstants.NotLoggedIn;
            LoginLabelText = GitHubUserService.IsAuthenticated ? GitHubLoginButtonConstants.Disconnect : GitHubLoginButtonConstants.ConnectToGitHub;
            GitHubAvatarImageSource = GitHubUserService.IsAuthenticated ? GitHubUserService.AvatarUrl : BaseTheme.GetDefaultProfileImageSource();
        }

        Task ExecuteGitHubUserViewTappedCommand()
        {
            if (GitHubUserService.IsAuthenticated || GitHubUserService.IsDemoUser)
            {
                string alias = GitHubUserService.IsDemoUser ? nameof(GitTrends) : GitHubUserService.Alias;
                AnalyticsService.Track("Alias Label Tapped", "Alias", alias);

                return _deepLinkingService.OpenApp($"github://", $"{GitHubConstants.GitHubBaseUrl}/{alias}", $"{GitHubConstants.GitHubBaseUrl}/{alias}");
            }
            else
            {
                var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                return ExecuteConnectToGitHubButtonCommand(GitHubAuthenticationService, _deepLinkingService, GitHubUserService, cancellationTokenSource.Token, null);
            }
        }

        void OnSetNotificationsCompleted(AccessState? accessState) => _setNotificationsPreferenceCompletedEventManager.RaiseEvent(this, accessState, nameof(SetNotificationsPreferenceCompleted));
    }
}
