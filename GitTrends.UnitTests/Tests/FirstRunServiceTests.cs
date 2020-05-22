using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace GitTrends.UnitTests
{
    class FirstRunServiceTests : BaseTest
    {
        //[Test] ToDo
        public Task FirstRunServiceTest_AuthorizeSessionCompleted()
        {
            throw new NotImplementedException();
        }

        [Test]
        public async Task FirstRunServiceTest_DemoUserActivated()
        {
            //Arrange
            bool isFirstRun_Initial, isFirstRun_Final;

            var activateDemoUserTCS = new TaskCompletionSource<object?>();

            var firstRunService = ContainerService.Container.GetService<FirstRunService>();

            var githubAuthorizationService = ContainerService.Container.GetService<GitHubAuthenticationService>();
            githubAuthorizationService.DemoUserActivated += HandleDemoUserActivated;


            //Act
            isFirstRun_Initial = firstRunService.IsFirstRun;

            await githubAuthorizationService.ActivateDemoUser().ConfigureAwait(false);
            await activateDemoUserTCS.Task.ConfigureAwait(false);

            //Assert
            isFirstRun_Final = firstRunService.IsFirstRun;

            Assert.IsTrue(isFirstRun_Initial);
            Assert.IsFalse(isFirstRun_Final);

            async void HandleDemoUserActivated(object? sender, EventArgs e)
            {
                await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
                activateDemoUserTCS.SetResult(null);
            }
        }
    }
}
