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

            var splashScreenViewModel = ServiceCollection.ServiceProvider.GetService<SplashScreenViewModel>();
            splashScreenViewModel.InitializationComplete += HandleInitializationComplete;

            //Act
            await splashScreenViewModel.InitializeAppCommand.ExecuteAsync().ConfigureAwait(false);
            var initializationCompleteEventArgs = await initializeAppCommandTCS.Task.ConfigureAwait(false);

            //Assert
            Assert.IsTrue(didInitializationCompleteFire);
            Assert.IsTrue(initializationCompleteEventArgs.IsInitializationSuccessful);

            void HandleInitializationComplete(object? sender, InitializationCompleteEventArgs e)
            {
                splashScreenViewModel.InitializationComplete -= HandleInitializationComplete;

                didInitializationCompleteFire = true;
                initializeAppCommandTCS.SetResult(e);
            }
        }
    }
}
