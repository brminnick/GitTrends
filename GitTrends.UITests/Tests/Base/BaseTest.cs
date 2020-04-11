using System;
using System.Threading.Tasks;
using GitTrends.Mobile.Shared;
using NUnit.Framework;

using Xamarin.UITest;

namespace GitTrends.UITests
{
    abstract class BaseTest
    {
        readonly Platform _platform;
        readonly UserType _userType;

        IApp? _app;
        ReferringSitesPage? _referringSitesPage;
        RepositoryPage? _repositoryPage;
        SettingsPage? _settingsPage;
        TrendsPage? _trendsPage;
        SplashScreenPage? _splashScreenPage;

        protected BaseTest(Platform platform, UserType userType) => (_platform, _userType) = (platform, userType);

        protected IApp App => _app ?? throw new NullReferenceException();
        protected ReferringSitesPage ReferringSitesPage => _referringSitesPage ?? throw new NullReferenceException();
        protected RepositoryPage RepositoryPage => _repositoryPage ?? throw new NullReferenceException();
        protected SettingsPage SettingsPage => _settingsPage ?? throw new NullReferenceException();
        protected TrendsPage TrendsPage => _trendsPage ?? throw new NullReferenceException();
        protected SplashScreenPage SplashScreenPage => _splashScreenPage ?? throw new NullReferenceException();

        [SetUp]
        public virtual Task BeforeEachTest()
        {
            _app = AppInitializer.StartApp(_platform);

            _splashScreenPage = new SplashScreenPage(App);
            _referringSitesPage = new ReferringSitesPage(App);
            _repositoryPage = new RepositoryPage(App);
            _settingsPage = new SettingsPage(App);
            _trendsPage = new TrendsPage(App);

            App.Screenshot("App Initialized");

            return _userType switch
            {
                UserType.Demo => SetupDemoUser(),
                UserType.LoggedIn => SetupLoggedInUser(),
                UserType.Neither => SetupNeitherUser(),
                _ => throw new NotSupportedException()
            };
        }

        protected Task SetupNeitherUser() => Task.CompletedTask;

        protected async Task SetupDemoUser()
        {
            await RepositoryPage.WaitForPageToLoad().ConfigureAwait(false);

            try
            {
                RepositoryPage.WaitForGitHubUserNotFoundPopup();
                RepositoryPage.AcceptGitHubUserNotFoundPopup();
            }
            catch
            {
                RepositoryPage.TapSettingsButton();
            }

            await SettingsPage.WaitForPageToLoad().ConfigureAwait(false);
            SettingsPage.DismissSyncfusionLicensePopup();

            SettingsPage.TapDemoModeButton();
            SettingsPage.WaitForGitHubLoginToComplete();

            SettingsPage.TapBackButton();

            await RepositoryPage.WaitForPageToLoad().ConfigureAwait(false);
            await RepositoryPage.WaitForNoPullToRefreshIndicator().ConfigureAwait(false);
        }

        async Task SetupLoggedInUser()
        {
            await LoginToGitHub().ConfigureAwait(false);

            await RepositoryPage.WaitForPageToLoad().ConfigureAwait(false);

            try
            {
                RepositoryPage.WaitForGitHubUserNotFoundPopup();
                RepositoryPage.AcceptGitHubUserNotFoundPopup();
            }
            catch
            {
                RepositoryPage.TapSettingsButton();
            }

            await SettingsPage.WaitForPageToLoad().ConfigureAwait(false);
            SettingsPage.DismissSyncfusionLicensePopup();

            SettingsPage.WaitForGitHubLoginToComplete();
            SettingsPage.TapBackButton();

            await RepositoryPage.WaitForPageToLoad().ConfigureAwait(false);
            await RepositoryPage.WaitForNoPullToRefreshIndicator().ConfigureAwait(false);
        }

        protected async Task LoginToGitHub()
        {
            var gitHubToken = await AzureFunctionsApiService.GenerateGitTrendsOAuthToken().ConfigureAwait(false);

            App.InvokeBackdoorMethod(BackdoorMethodConstants.SetGitHubUser, gitHubToken.AccessToken);
        }
    }

    enum UserType { Demo, LoggedIn, Neither }
}

