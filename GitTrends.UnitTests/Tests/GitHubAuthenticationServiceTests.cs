using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using GitTrends.Mobile.Shared;
using GitTrends.Shared;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Xamarin.Essentials.Interfaces;

namespace GitTrends.UnitTests
{
    class GitHubAuthenticationServiceTests : BaseTest
    {
        [Test]
        public async Task AuthorizeSessionStartedTest()
        {
            //Arrange
            var didAuthorizeSessionStartedFire = false;
            var authorizeSessionStartedTCS = new TaskCompletionSource<object?>();

            var gitHubAuthorizationServce = ServiceCollection.ServiceProvider.GetService<GitHubAuthenticationService>();
            gitHubAuthorizationServce.AuthorizeSessionStarted += HandleAuthorizeSessionStarted;

            //Act
            Assert.ThrowsAsync<Exception>(async () => await gitHubAuthorizationServce.AuthorizeSession(new Uri("https://google.com"), CancellationToken.None).ConfigureAwait(false));
            await authorizeSessionStartedTCS.Task.ConfigureAwait(false);

            //Assert
            Assert.IsTrue(didAuthorizeSessionStartedFire);


            void HandleAuthorizeSessionStarted(object? sender, EventArgs e)
            {
                gitHubAuthorizationServce.AuthorizeSessionStarted -= HandleAuthorizeSessionStarted;
                didAuthorizeSessionStartedFire = true;
                authorizeSessionStartedTCS.SetResult(null);
            }
        }

        [Test]
        public async Task AuthorizeSessionCompletedTest()
        {
            //Arrange
            bool isAuthorizationSuccessful;

            var didAuthorizeSessionCompletedFire = false;
            var authorizeSessionCompletedTCS = new TaskCompletionSource<bool>();

            var gitHubAuthorizationServce = ServiceCollection.ServiceProvider.GetService<GitHubAuthenticationService>();
            gitHubAuthorizationServce.AuthorizeSessionCompleted += HandleAuthorizeSessionCompleted;

            //Act
            Assert.ThrowsAsync<Exception>(async () => await gitHubAuthorizationServce.AuthorizeSession(new Uri("https://google.com"), CancellationToken.None).ConfigureAwait(false));
            isAuthorizationSuccessful = await authorizeSessionCompletedTCS.Task.ConfigureAwait(false);

            //Assert
            Assert.IsTrue(didAuthorizeSessionCompletedFire);
            Assert.IsFalse(isAuthorizationSuccessful);


            void HandleAuthorizeSessionCompleted(object? sender, AuthorizeSessionCompletedEventArgs e)
            {
                gitHubAuthorizationServce.AuthorizeSessionCompleted -= HandleAuthorizeSessionCompleted;
                didAuthorizeSessionCompletedFire = true;
                authorizeSessionCompletedTCS.SetResult(e.IsSessionAuthorized);
            }
        }

        [Test]
        public async Task ActivateDemoUserTest()
        {
            //Arrange
            var didDemoUserActivatedFire = false;
            var demoUserActivatedTCS = new TaskCompletionSource<object?>();

            var gitHubUserService = ServiceCollection.ServiceProvider.GetService<GitHubUserService>();

            var gitHubAuthorizationServce = ServiceCollection.ServiceProvider.GetService<GitHubAuthenticationService>();
            gitHubAuthorizationServce.DemoUserActivated += HandleDemoUserActivated;

            //Act
            await gitHubAuthorizationServce.ActivateDemoUser().ConfigureAwait(false);
            await demoUserActivatedTCS.Task.ConfigureAwait(false);

            //Assert
            Assert.IsTrue(didDemoUserActivatedFire);
            Assert.AreEqual(DemoDataConstants.Alias, gitHubUserService.Alias);
            Assert.AreEqual(DemoDataConstants.Name, gitHubUserService.Name);


            void HandleDemoUserActivated(object? sender, EventArgs e)
            {
                gitHubAuthorizationServce.DemoUserActivated -= HandleDemoUserActivated;
                didDemoUserActivatedFire = true;
                demoUserActivatedTCS.SetResult(null);
            }
        }

        [Test]
        public async Task LogOutTest()
        {
            //Arrange
            string gitHubUserName_Initial, gitHubUserName_Final, gitHubUserAlias_Initial, gitHubUserAlias_Final;

            var didDLoggedOutFire = false;
            var loggedOutTCS = new TaskCompletionSource<object?>();

            var gitHubUserService = ServiceCollection.ServiceProvider.GetService<GitHubUserService>();

            var gitHubAuthorizationServce = ServiceCollection.ServiceProvider.GetService<GitHubAuthenticationService>();

            await gitHubAuthorizationServce.ActivateDemoUser().ConfigureAwait(false);
            gitHubAuthorizationServce.LoggedOut += HandleLoggedOut;

            //Act
            gitHubUserName_Initial = gitHubUserService.Name;
            gitHubUserAlias_Initial = gitHubUserService.Alias;

            await gitHubAuthorizationServce.LogOut().ConfigureAwait(false);
            await loggedOutTCS.Task.ConfigureAwait(false);

            gitHubUserAlias_Final = gitHubUserService.Alias;
            gitHubUserName_Final = gitHubUserService.Name;


            //Assert
            Assert.IsTrue(didDLoggedOutFire);
            Assert.AreEqual(DemoDataConstants.Name, gitHubUserName_Initial);
            Assert.AreEqual(DemoDataConstants.Alias, gitHubUserAlias_Initial);

            void HandleLoggedOut(object? sender, EventArgs e)
            {
                gitHubAuthorizationServce.LoggedOut -= HandleLoggedOut;
                didDLoggedOutFire = true;
                loggedOutTCS.SetResult(null);
            }
        }

        [Test]
        public async Task GetGitHubLoginUrlTest()
        {
            //Arrange
            string gitHubLoginUrl, scope, state;
            var preferences = ServiceCollection.ServiceProvider.GetService<IPreferences>();
            var gitHubAuthorizationServce = ServiceCollection.ServiceProvider.GetService<GitHubAuthenticationService>();

            //Act
            gitHubLoginUrl = await gitHubAuthorizationServce.GetGitHubLoginUrl(CancellationToken.None).ConfigureAwait(false);

            scope = HttpUtility.ParseQueryString(gitHubLoginUrl).Get(nameof(scope));
            state = HttpUtility.ParseQueryString(gitHubLoginUrl).Get(nameof(state));

            //Assert
            Assert.IsNotNull(gitHubLoginUrl);
            Assert.IsNotEmpty(gitHubLoginUrl);

            Assert.IsNotNull(scope);
            Assert.IsNotEmpty(scope);
            Assert.AreEqual(HttpUtility.UrlDecode(GitHubConstants.OAuthScope), scope);

            Assert.IsNotNull(state);
            Assert.IsNotEmpty(state);
            Assert.AreEqual(preferences.Get("MostRecentSessionId", string.Empty), state);
        }

        //[Test] Todo
        public void AuthorizeSessionTest()
        {
            throw new NotImplementedException();
        }
    }
}