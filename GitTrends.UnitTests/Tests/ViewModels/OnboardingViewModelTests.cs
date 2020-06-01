using System;
using System.Threading;
using System.Threading.Tasks;
using GitTrends.Mobile.Shared;
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

            var onboardingViewModel = ServiceCollection.ServiceProvider.GetService<OnboardingViewModel>();
            onboardingViewModel.SkipButtonTapped += HandleSkipButtonTapped;

            //Act
            await onboardingViewModel.DemoButtonCommand.ExecuteAsync(OnboardingConstants.SkipText).ConfigureAwait(false);
            await skipButtonTappedTCS.Task.ConfigureAwait(false);

            //Assert
            Assert.IsTrue(didSkipButtonTappedFire);

            void HandleSkipButtonTapped(object? sender, EventArgs e)
            {
                onboardingViewModel.SkipButtonTapped -= HandleSkipButtonTapped;

                didSkipButtonTappedFire = true;
                skipButtonTappedTCS.SetResult(null);
            }
        }

        [Test]
        public async Task DemoButtonCommand_TryDemo()
        {
            //Arrange
            var onboardingViewModel = ServiceCollection.ServiceProvider.GetService<OnboardingViewModel>();
            var gitHubUserService = ServiceCollection.ServiceProvider.GetService<GitHubUserService>();

            //Act
            await onboardingViewModel.DemoButtonCommand.ExecuteAsync(OnboardingConstants.TryDemoText).ConfigureAwait(false);

            //Assert
            Assert.IsTrue(gitHubUserService.IsDemoUser);
        }

        [Test]
        public async Task ConnectToGitHubButtonCommandTest()
        {
            //Arrange
            bool isAuthenticating_BeforeCommand, isAuthenticating_DuringCommand, isAuthenticating_AfterCommand;
            bool isDemoButtonVisible_BeforeCommand, isDemoButtonVisible_DuringCommand, isDemoButtonVisible_AfterCommand;

            bool didOpenAsyncFire = false;
            var openAsyncExecutedTCS = new TaskCompletionSource<object?>();

            var mockBrowser = (MockBrowser)ServiceCollection.ServiceProvider.GetService<IBrowser>();
            mockBrowser.OpenAsyncExecuted += HandleOpenAsyncExecuted;

            var onboardingViewModel = ServiceCollection.ServiceProvider.GetService<OnboardingViewModel>();

            //Act
            isAuthenticating_BeforeCommand = onboardingViewModel.IsAuthenticating;
            isDemoButtonVisible_BeforeCommand = onboardingViewModel.IsDemoButtonVisible;

            var connectToGitHubButtonCommandTask = onboardingViewModel.ConnectToGitHubButtonCommand.ExecuteAsync((CancellationToken.None, null));
            isAuthenticating_DuringCommand = onboardingViewModel.IsAuthenticating;
            isDemoButtonVisible_DuringCommand = onboardingViewModel.IsDemoButtonVisible;

            await connectToGitHubButtonCommandTask.ConfigureAwait(false);
            await openAsyncExecutedTCS.Task.ConfigureAwait(false);

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

            void HandleOpenAsyncExecuted(object? sender, EventArgs e)
            {
                mockBrowser.OpenAsyncExecuted -= HandleOpenAsyncExecuted;
                didOpenAsyncFire = true;

                openAsyncExecutedTCS.SetResult(null);
            }
        }

        [Test]
        public async Task EnableNotificationsButtonTappedTest()
        {
            //Arrange
            const string successSvg = "check.svg";
            const string bellSvg = "bell.svg";

            string notificationStatusSvgImageSource_Initial, notificationStatusSvgImageSource_Final;
            var onboardingViewModel = ServiceCollection.ServiceProvider.GetService<OnboardingViewModel>();

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
