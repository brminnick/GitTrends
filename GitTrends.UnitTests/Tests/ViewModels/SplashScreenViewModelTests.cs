using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace GitTrends.UnitTests
{
    class AppInitializationServiceTests : BaseTest
    {
        [Test]
        public async Task InitiazeAppCommandTest()
        {
            //Arrange
            bool didInitializationCompleteFire = false;
            var initializeAppCommandTCS = new TaskCompletionSource<InitializationCompleteEventArgs>();

            AppInitializationService.InitializationCompleted += HandleInitializationComplete;

            var appInitializationService = ServiceCollection.ServiceProvider.GetRequiredService<AppInitializationService>();

            //Assert
            Assert.IsFalse(appInitializationService.IsInitializationComplete);

            //Act
            await appInitializationService.InitializeApp(CancellationToken.None).ConfigureAwait(false);
            var initializationCompleteEventArgs = await initializeAppCommandTCS.Task.ConfigureAwait(false);

            //Assert
            Assert.IsTrue(didInitializationCompleteFire);
            Assert.IsTrue(initializationCompleteEventArgs.IsInitializationSuccessful);
            Assert.IsTrue(appInitializationService.IsInitializationComplete);

            void HandleInitializationComplete(object? sender, InitializationCompleteEventArgs e)
            {
                AppInitializationService.InitializationCompleted -= HandleInitializationComplete;

                didInitializationCompleteFire = true;
                initializeAppCommandTCS.SetResult(e);
            }
        }
    }
}
