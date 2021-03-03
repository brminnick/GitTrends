using System;
using System.Threading;
using System.Threading.Tasks;
using GitTrends.Mobile.Common.Constants;
using GitTrends.Shared;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Xamarin.Essentials.Interfaces;

namespace GitTrends.UnitTests
{
    class OnboardingViewModelTests : BaseTest
    {
        [Test]
        public async Task DemoButtonCommand_Skip()
        {
            //Arrange
            bool didSkipButtonTappedFire = false;
            var skipButtonTappedTCS = new TaskCompletionSource<object?>();

            var onboardingViewModel = ServiceCollection.ServiceProvider.GetRequiredService<OnboardingViewModel>();
            OnboardingViewModel.SkipButtonTapped += HandleSkipButtonTapped;

            //Act
            await onboardingViewModel.DemoButtonCommand.ExecuteAsync(OnboardingConstants.SkipText).ConfigureAwait(false);
            await skipButtonTappedTCS.Task.ConfigureAwait(false);

            //Assert
            Assert.IsTrue(didSkipButtonTappedFire);

            void HandleSkipButtonTapped(object? sender, EventArgs e)
            {
                OnboardingViewModel.SkipButtonTapped -= HandleSkipButtonTapped;

                didSkipButtonTappedFire = true;
                skipButtonTappedTCS.SetResult(null);
            }
        }

        [Test]
        public async Task DemoButtonCommand_TryDemo()
        {
            //Arrange
            var onboardingViewModel = ServiceCollection.ServiceProvider.GetRequiredService<OnboardingViewModel>();
            var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();

            //Act
            await onboardingViewModel.DemoButtonCommand.ExecuteAsync(OnboardingConstants.TryDemoText).ConfigureAwait(false);

            //Assert
            Assert.IsTrue(gitHubUserService.IsDemoUser);
        }

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

            var onboardingViewModel = ServiceCollection.ServiceProvider.GetRequiredService<OnboardingViewModel>();

            //Act
            isAuthenticating_BeforeCommand = onboardingViewModel.IsAuthenticating;
            isDemoButtonVisible_BeforeCommand = onboardingViewModel.IsDemoButtonVisible;

            var connectToGitHubButtonCommandTask = onboardingViewModel.ConnectToGitHubButtonCommand.ExecuteAsync((CancellationToken.None, null));
            isAuthenticating_DuringCommand = onboardingViewModel.IsAuthenticating;
            isDemoButtonVisible_DuringCommand = onboardingViewModel.IsDemoButtonVisible;

            await connectToGitHubButtonCommandTask.ConfigureAwait(false);
            var openedUri = await openAsyncExecutedTCS.Task.ConfigureAwait(false);
            openedUrl = openedUri.AbsoluteUri;

            isAuthenticating_AfterCommand = onboardingViewModel.IsAuthenticating;
            isDemoButtonVisible_AfterCommand = onboardingViewModel.IsDemoButtonVisible;

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
        public async Task EnableNotificationsButtonTappedTest()
        {
            //Arrange
            const string successSvg = "check.svg";
            const string bellSvg = "bell.svg";

            string notificationStatusSvgImageSource_Initial, notificationStatusSvgImageSource_Final;
            var onboardingViewModel = ServiceCollection.ServiceProvider.GetRequiredService<OnboardingViewModel>();

            //Act
            notificationStatusSvgImageSource_Initial = onboardingViewModel.NotificationStatusSvgImageSource;

            await onboardingViewModel.EnableNotificationsButtonTapped.ExecuteAsync().ConfigureAwait(false);

            notificationStatusSvgImageSource_Final = onboardingViewModel.NotificationStatusSvgImageSource;

            //Assert
            Assert.AreEqual(SvgService.GetFullPath(bellSvg), notificationStatusSvgImageSource_Initial);
            Assert.AreEqual(SvgService.GetFullPath(successSvg), notificationStatusSvgImageSource_Final);
        }
    }
}
