using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace GitTrends.UnitTests
{
    class SyncfusionServiceTests : BaseTest
    {
        [Test]
        public async Task InitializeTest()
        {
            //Arrange
            string? license_initial, license_final;
            var syncfusionService = ServiceCollection.ServiceProvider.GetRequiredService<SyncfusionService>();

            //Act
            license_initial = await syncfusionService.GetLicense().ConfigureAwait(false);

            await syncfusionService.Initialize(CancellationToken.None).ConfigureAwait(false);

            license_final = await syncfusionService.GetLicense().ConfigureAwait(false);

            //Assert
            Assert.IsNull(license_initial);
            Assert.IsNotNull(license_final);
            Assert.Greater(SyncfusionService.AssemblyVersionNumber, 0);
        }
    }
}
