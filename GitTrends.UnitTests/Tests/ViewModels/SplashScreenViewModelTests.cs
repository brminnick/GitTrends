using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace GitTrends.UnitTests
{
    class SplashScreenViewModelTests : BaseTest
    {
        [Test]
        public async Task InitiazeAppCommandTest()
        {
            //Arrange
            bool didInitializationCompleteFire = false;
            var initializeAppCommandTCS = new TaskCompletionSource<InitializationCompleteEventArgs>();

            SplashScreenViewModel.InitializationCompleted += HandleInitializationComplete;

            var splashScreenViewModel = ServiceCollection.ServiceProvider.GetRequiredService<SplashScreenViewModel>();

            //Act
            await splashScreenViewModel.InitializeAppCommand.ExecuteAsync().ConfigureAwait(false);
            var initializationCompleteEventArgs = await initializeAppCommandTCS.Task.ConfigureAwait(false);

            //Assert
            Assert.IsTrue(didInitializationCompleteFire);
            Assert.IsTrue(initializationCompleteEventArgs.IsInitializationSuccessful);

            void HandleInitializationComplete(object? sender, InitializationCompleteEventArgs e)
            {
                SplashScreenViewModel.InitializationCompleted -= HandleInitializationComplete;

                didInitializationCompleteFire = true;
                initializeAppCommandTCS.SetResult(e);
            }
        }
    }
}
