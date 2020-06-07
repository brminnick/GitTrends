using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Xamarin.Essentials.Interfaces;

namespace GitTrends.UnitTests
{
    public class SettingsViewModelTests
    {
        //public IReadOnlyList<string> ThemePickerItemsSource { get; } = Enum.GetNames(typeof(PreferredTheme));
        //public IAsyncCommand<(CancellationToken CancellationToken, Xamarin.Essentials.BrowserLaunchOptions? BrowserLaunchOptions)> ConnectToGitHubButtonCommand { get; }
        //public IAsyncCommand<string> DemoButtonCommand { get; }
        //public bool IsAliasLabelVisible => !IsAuthenticating && LoginLabelText is GitHubLoginButtonConstants.Disconnect;
        //public override bool IsDemoButtonVisible => base.IsDemoButtonVisible && LoginLabelText is GitHubLoginButtonConstants.ConnectToGitHub;

        //public bool ShouldShowClonesByDefaultSwitchValue
        //public bool ShouldShowUniqueClonesByDefaultSwitchValue
        //public bool ShouldShowViewsByDefaultSwitchValue
        //public bool ShouldShowUniqueViewsByDefaultSwitchValue

        //public string LoginLabelText

        //public string GitHubAvatarImageSource
        //public string GitHubAliasLabelText
        //public string GitHubNameLabelText

        //public int ThemePickerSelectedThemeIndex

        //public bool IsRegisterForNotificationsSwitchEnabled
        //public bool IsRegisterForNotificationsSwitchToggled

        //public bool IsNotAuthenticating => !IsAuthenticating;
        //public virtual bool IsDemoButtonVisible => !IsAuthenticating && GitHubUserService.Alias != DemoDataConstants.Alias;


        [Test]
        public async Task CopyrightLabelTappedCommandTest()
        {
            //Arrange
            bool didOpenAsyncFire = false;
            var openAsyncTCS = new TaskCompletionSource<object?>();

            var settingsViewModel = ServiceCollection.ServiceProvider.GetService<SettingsViewModel>();

            var launcher = (MockLauncher)ServiceCollection.ServiceProvider.GetService<ILauncher>();
            launcher.OpenAsyncExecuted += HandleOpenAsyncExecuted;

            //Act
            settingsViewModel.CopyrightLabelTappedCommand.Execute(null);
            await openAsyncTCS.Task.ConfigureAwait(false);

            //Assert
            Assert.IsTrue(didOpenAsyncFire);

            void HandleOpenAsyncExecuted(object? sender, EventArgs e)
            {
                launcher.OpenAsyncExecuted -= HandleOpenAsyncExecuted;

                didOpenAsyncFire = true;
                openAsyncTCS.SetResult(null);
            }
        }
    }
}
