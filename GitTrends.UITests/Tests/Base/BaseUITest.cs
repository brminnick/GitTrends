using System;
using System.Threading.Tasks;
using GitTrends.Mobile.Common;
using GitTrends.Shared;
using NUnit.Framework;

using Xamarin.UITest;

namespace GitTrends.UITests
{
    enum UserType { Demo, LoggedIn, Neither }

    abstract class BaseUITest
    {
        readonly Platform _platform;

        IApp? _app;
        ReferringSitesPage? _referringSitesPage;
        RepositoryPage? _repositoryPage;
        SettingsPage? _settingsPage;
        TrendsPage? _trendsPage;
        SplashScreenPage? _splashScreenPage;
        OnboardingPage? _onboardingPage;
        WelcomePage? _welcomePage;

        protected BaseUITest(Platform platform, UserType userType) => (_platform, UserType) = (platform, userType);

        protected UserType UserType { get; }

        protected IApp App => _app ?? throw new NullReferenceException();
        protected ReferringSitesPage ReferringSitesPage => _referringSitesPage ?? throw new NullReferenceException();
        protected RepositoryPage RepositoryPage => _repositoryPage ?? throw new NullReferenceException();
        protected SettingsPage SettingsPage => _settingsPage ?? throw new NullReferenceException();
        protected TrendsPage TrendsPage => _trendsPage ?? throw new NullReferenceException();
        protected SplashScreenPage SplashScreenPage => _splashScreenPage ?? throw new NullReferenceException();
        protected OnboardingPage OnboardingPage => _onboardingPage ?? throw new NullReferenceException();
        protected WelcomePage WelcomePage => _welcomePage ?? throw new NullReferenceException();

        protected string LoggedInUserAlias => App.InvokeBackdoorMethod<string>(BackdoorMethodConstants.GetLoggedInUserAlias);
        protected string LoggedInUserName => App.InvokeBackdoorMethod<string>(BackdoorMethodConstants.GetLoggedInUserName);
        protected string LoggedInUserAvatarUrl => App.InvokeBackdoorMethod<string>(BackdoorMethodConstants.GetLoggedInUserAvatarUrl);

        [SetUp]
        public virtual Task BeforeEachTest()
        {
            _app = AppInitializer.StartApp(_platform);

            _splashScreenPage = new SplashScreenPage(App);
            _referringSitesPage = new ReferringSitesPage(App);
            _repositoryPage = new RepositoryPage(App);
            _settingsPage = new SettingsPage(App);
            _trendsPage = new TrendsPage(App);
            _onboardingPage = new OnboardingPage(App);
            _welcomePage = new WelcomePage(App);

            App.Screenshot("App Initialized");

            return UserType switch
            {
                UserType.Demo => SetupDemoUser(),
                UserType.LoggedIn => SetupLoggedInUser(),
                UserType.Neither => SetupNeither(),
                _ => throw new NotSupportedException()
            };
        }

        protected Task SetupNeither() => OnboardingPage.WaitForPageToLoad();

        protected async Task SetupDemoUser()
        {
            await OnboardingPage.WaitForPageToLoad().ConfigureAwait(false);

            OnboardingPage.TapNextButton();
            OnboardingPage.TapNextButton();

            await RepositoryPage.WaitForPageToLoad().ConfigureAwait(false);
        }

        protected async Task SetupLoggedInUser()
        {
            await OnboardingPage.WaitForPageToLoad().ConfigureAwait(false);

            await LoginToGitHub().ConfigureAwait(false);

            OnboardingPage.PopPage();

            await RepositoryPage.WaitForPageToLoad().ConfigureAwait(false);
        }

        protected async Task LoginToGitHub()
        {
            var token = await AzureFunctionsApiService.GetTestToken().ConfigureAwait(false);

            App.InvokeBackdoorMethod(BackdoorMethodConstants.SetGitHubUser, token.AccessToken);

            GitHubToken? currentUserToken = null;

            while (currentUserToken is null)
            {
                await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
                currentUserToken = App.InvokeBackdoorMethod<GitHubToken?>(BackdoorMethodConstants.GetGitHubToken);
            }

            await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);

            App.Screenshot("Logged In");
        }
    }
}

