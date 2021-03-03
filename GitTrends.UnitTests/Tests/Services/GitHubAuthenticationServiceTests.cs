using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
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

            var gitHubAuthenticationService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubAuthenticationService>();
            GitHubAuthenticationService.AuthorizeSessionStarted += HandleAuthorizeSessionStarted;

            //Act
            Assert.ThrowsAsync<Exception>(async () => await gitHubAuthenticationService.AuthorizeSession(new Uri("https://google.com"), CancellationToken.None).ConfigureAwait(false));
            await authorizeSessionStartedTCS.Task.ConfigureAwait(false);

            //Assert
            Assert.IsTrue(didAuthorizeSessionStartedFire);


            void HandleAuthorizeSessionStarted(object? sender, EventArgs e)
            {
                GitHubAuthenticationService.AuthorizeSessionStarted -= HandleAuthorizeSessionStarted;
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

            var gitHubAuthenticationService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubAuthenticationService>();
            GitHubAuthenticationService.AuthorizeSessionCompleted += HandleAuthorizeSessionCompleted;

            //Act
            Assert.ThrowsAsync<Exception>(async () => await gitHubAuthenticationService.AuthorizeSession(new Uri("https://google.com"), CancellationToken.None).ConfigureAwait(false));
            isAuthorizationSuccessful = await authorizeSessionCompletedTCS.Task.ConfigureAwait(false);

            //Assert
            Assert.IsTrue(didAuthorizeSessionCompletedFire);
            Assert.IsFalse(isAuthorizationSuccessful);


            void HandleAuthorizeSessionCompleted(object? sender, AuthorizeSessionCompletedEventArgs e)
            {
                GitHubAuthenticationService.AuthorizeSessionCompleted -= HandleAuthorizeSessionCompleted;
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

            var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();

            var gitHubAuthenticationService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubAuthenticationService>();
            GitHubAuthenticationService.DemoUserActivated += HandleDemoUserActivated;

            //Act
            await gitHubAuthenticationService.ActivateDemoUser().ConfigureAwait(false);
            await demoUserActivatedTCS.Task.ConfigureAwait(false);

            //Assert
            Assert.IsTrue(didDemoUserActivatedFire);
            Assert.AreEqual(DemoUserConstants.Alias, gitHubUserService.Alias);
            Assert.AreEqual(DemoUserConstants.Name, gitHubUserService.Name);


            void HandleDemoUserActivated(object? sender, EventArgs e)
            {
                GitHubAuthenticationService.DemoUserActivated -= HandleDemoUserActivated;
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

            var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();

            var gitHubAuthenticationService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubAuthenticationService>();

            await gitHubAuthenticationService.ActivateDemoUser().ConfigureAwait(false);
            GitHubAuthenticationService.LoggedOut += HandleLoggedOut;

            //Act
            gitHubUserName_Initial = gitHubUserService.Name;
            gitHubUserAlias_Initial = gitHubUserService.Alias;

            await gitHubAuthenticationService.LogOut().ConfigureAwait(false);
            await loggedOutTCS.Task.ConfigureAwait(false);

            gitHubUserAlias_Final = gitHubUserService.Alias;
            gitHubUserName_Final = gitHubUserService.Name;


            //Assert
            Assert.IsTrue(didDLoggedOutFire);
            Assert.AreEqual(DemoUserConstants.Name, gitHubUserName_Initial);
            Assert.AreEqual(DemoUserConstants.Alias, gitHubUserAlias_Initial);

            void HandleLoggedOut(object? sender, EventArgs e)
            {
                GitHubAuthenticationService.LoggedOut -= HandleLoggedOut;
                didDLoggedOutFire = true;
                loggedOutTCS.SetResult(null);
            }
        }

        [Test]
        public async Task GetGitHubLoginUrlTest()
        {
            //Arrange
            string gitHubLoginUrl, scope, state;
            var preferences = ServiceCollection.ServiceProvider.GetRequiredService<IPreferences>();
            var gitHubAuthenticationService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubAuthenticationService>();

            //Act
            gitHubLoginUrl = await gitHubAuthenticationService.GetGitHubLoginUrl(CancellationToken.None).ConfigureAwait(false);

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