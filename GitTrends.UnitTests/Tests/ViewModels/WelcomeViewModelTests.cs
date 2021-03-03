using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Xamarin.Essentials.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using GitTrends.Shared;

namespace GitTrends.UnitTests
{
    class WelcomeViewModelTests : BaseTest
    {
        [Test]
        public async Task ConnectToGitHubButtonCommandTest()
        {
            //Arrange
            string openedUrl;
            bool isAuthenticating_BeforeCommand, isAuthenticating_DuringCommand, isAuthenticating_AfterCommand;
            bool isDemoButtonVisible_BeforeCommand, isDemoButtonVisible_DuringCommand, isDemoButtonVisible_AfterCommand;

            bool didOpenAsyncFire = false;
            var openAsyncExecutedTCS = new TaskCompletionSource<Uri>();

            MockBrowser.OpenAsyncExecuted += HandleOpenAsyncExecuted;

            var welcomeViewModel = ServiceCollection.ServiceProvider.GetRequiredService<WelcomeViewModel>();

            //Act
            isAuthenticating_BeforeCommand = welcomeViewModel.IsAuthenticating;
            isDemoButtonVisible_BeforeCommand = welcomeViewModel.IsDemoButtonVisible;

            var connectToGitHubButtonCommandTask = welcomeViewModel.ConnectToGitHubButtonCommand.ExecuteAsync((CancellationToken.None, null));
            isAuthenticating_DuringCommand = welcomeViewModel.IsAuthenticating;
            isDemoButtonVisible_DuringCommand = welcomeViewModel.IsDemoButtonVisible;

            await connectToGitHubButtonCommandTask.ConfigureAwait(false);
            var openedUri = await openAsyncExecutedTCS.Task.ConfigureAwait(false);
            openedUrl = openedUri.AbsoluteUri;

            isAuthenticating_AfterCommand = welcomeViewModel.IsAuthenticating;
            isDemoButtonVisible_AfterCommand = welcomeViewModel.IsDemoButtonVisible;

            //Assert
            Assert.IsTrue(didOpenAsyncFire);

            Assert.IsFalse(isAuthenticating_BeforeCommand);
            Assert.True(isDemoButtonVisible_BeforeCommand);

            Assert.IsTrue(isAuthenticating_DuringCommand);
            Assert.False(isDemoButtonVisible_DuringCommand);

            Assert.IsFalse(isAuthenticating_AfterCommand);
            Assert.True(isDemoButtonVisible_AfterCommand);

            Assert.IsTrue(openedUrl.Contains($"{GitHubConstants.GitHubBaseUrl}/login/oauth/authorize?client_id="));
            Assert.IsTrue(openedUrl.Contains($"&scope={GitHubConstants.OAuthScope}&state="));

            void HandleOpenAsyncExecuted(object? sender, Uri e)
            {
                MockBrowser.OpenAsyncExecuted -= HandleOpenAsyncExecuted;
                didOpenAsyncFire = true;

                openAsyncExecutedTCS.SetResult(e);
            }
        }

        [Test]
        public async Task DemoButtonCommandTest()
        {
            //Arrange
            bool isDemoButtonVisible_Initial, isDemoButtonVisible_Final;

            var welcomeViewModel = ServiceCollection.ServiceProvider.GetRequiredService<WelcomeViewModel>();
            var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();

            //Act
            isDemoButtonVisible_Initial = welcomeViewModel.IsDemoButtonVisible;

            await welcomeViewModel.DemoButtonCommand.ExecuteAsync(null).ConfigureAwait(false);

            isDemoButtonVisible_Final = welcomeViewModel.IsDemoButtonVisible;

            //Assert
            Assert.IsTrue(gitHubUserService.IsDemoUser);

            Assert.IsTrue(isDemoButtonVisible_Initial);
            Assert.IsFalse(isDemoButtonVisible_Final);
        }
    }
}
