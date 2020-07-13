using System.Threading;
using System.Threading.Tasks;
using GitTrends.Shared;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace GitTrends.UnitTests
{
    class MediaElementServiceTest : BaseTest
    {
        [Test]
        public async Task InitializeOnboardingChartTest()
        {
            //Arrange
            StreamingManifest? streamingManifest_BeforeInitialization, streamingManifest_AfterInitialization;

            var mediaElementService = ServiceCollection.ServiceProvider.GetRequiredService<MediaElementService>();

            //Act
            streamingManifest_BeforeInitialization = mediaElementService.OnboardingChart;

            await mediaElementService.InitializeOnboardingChart(CancellationToken.None).ConfigureAwait(false);

            streamingManifest_AfterInitialization = mediaElementService.OnboardingChart;

            //Assert
            Assert.IsNull(streamingManifest_BeforeInitialization);
            Assert.IsNotNull(streamingManifest_AfterInitialization);

            Assert.IsFalse(string.IsNullOrWhiteSpace(streamingManifest_AfterInitialization?.HlsUrl));
            Assert.IsFalse(string.IsNullOrWhiteSpace(streamingManifest_AfterInitialization?.ManifestUrl));
            Assert.IsTrue(streamingManifest_AfterInitialization?.HlsUrl.Contains(streamingManifest_AfterInitialization.ManifestUrl));
        }
    }
}
