using System;
using System.Threading;
using System.Threading.Tasks;
using GitTrends.Shared;
using NUnit.Framework;

namespace GitTrends.UnitTests
{
    class AzureFunctionsApiServiceTests : BaseTest
    {
        [Test]
        public async Task GetGitHubClientId()
        {
            //Arrange
            GetGitHubClientIdDTO? tokenDTO;
            var azureFunctionsApiService = new AzureFunctionsApiService(new MockAnalyticsService(), new MockMainThread());

            //Act
            tokenDTO = await azureFunctionsApiService.GetGitHubClientId(CancellationToken.None).ConfigureAwait(false);

            //Assert
            Assert.IsNotNull(tokenDTO);
            Assert.IsNotNull(tokenDTO.ClientId);
            Assert.IsFalse(string.IsNullOrWhiteSpace(tokenDTO.ClientId));
        }

        [Test]
        public async Task GenerateGitTrendsOAuthToken_InvalidDTO()
        {
            //Arrange
            GitHubToken? gitHubToken;
            var generateTokenDTO = new GenerateTokenDTO(string.Empty, string.Empty);
            var azureFunctionsApiService = new AzureFunctionsApiService(new MockAnalyticsService(), new MockMainThread());

            //Act
            gitHubToken = await azureFunctionsApiService.GenerateGitTrendsOAuthToken(generateTokenDTO, CancellationToken.None).ConfigureAwait(false);

            //Assert
            Assert.IsNotNull(gitHubToken);
            Assert.IsEmpty(GitHubToken.Empty.AccessToken);
            Assert.IsEmpty(GitHubToken.Empty.Scope);
            Assert.IsEmpty(GitHubToken.Empty.TokenType);
        }

        [Test]
        public async Task GetSyncfusionInformation()
        {
            //Arrange
            SyncFusionDTO? syncFusionDTO;
            var azureFunctionsApiService = new AzureFunctionsApiService(new MockAnalyticsService(), new MockMainThread());

            //Act
            syncFusionDTO = await azureFunctionsApiService.GetSyncfusionInformation(CancellationToken.None).ConfigureAwait(false);

            //Assert
            Assert.IsNotNull(syncFusionDTO);
            Assert.IsNotNull(syncFusionDTO.LicenseKey);
            Assert.IsFalse(string.IsNullOrWhiteSpace(syncFusionDTO.LicenseKey));

            Assert.Greater(syncFusionDTO.LicenseVersion, 0);
        }

        public async Task GetChartStreamingUrl()
        {
            //Arrange
            StreamingManifest? streamingManifest;
            var azureFunctionsApiService = new AzureFunctionsApiService(new MockAnalyticsService(), new MockMainThread());

            //Act
            streamingManifest = await azureFunctionsApiService.GetChartStreamingUrl(CancellationToken.None).ConfigureAwait(false);

            //Assert
            Assert.IsNotNull(streamingManifest);
            Assert.IsNotNull(streamingManifest.HlsUrl);
            Assert.IsNotNull(streamingManifest.ManifestUrl);
            Assert.IsFalse(string.IsNullOrWhiteSpace(streamingManifest.HlsUrl));
            Assert.IsFalse(string.IsNullOrWhiteSpace(streamingManifest.ManifestUrl));
        }

        [Test]
        public async Task GetNotificationHubInformation()
        {
            //Arrange
            NotificationHubInformation? notificationHubInformation;
            var azureFunctionsApiService = new AzureFunctionsApiService(new MockAnalyticsService(), new MockMainThread());

            //Act
            notificationHubInformation = await azureFunctionsApiService.GetNotificationHubInformation(CancellationToken.None).ConfigureAwait(false);

            //Assert
            Assert.IsNotNull(notificationHubInformation);
            Assert.IsNotNull(notificationHubInformation.ConnectionString);
            Assert.IsNotNull(notificationHubInformation.ConnectionString_Debug);
            Assert.IsNotNull(notificationHubInformation.Name);
            Assert.IsNotNull(notificationHubInformation.Name_Debug);

            Assert.IsFalse(string.IsNullOrWhiteSpace(notificationHubInformation.ConnectionString));
            Assert.IsFalse(string.IsNullOrWhiteSpace(notificationHubInformation.ConnectionString_Debug));
            Assert.IsFalse(string.IsNullOrWhiteSpace(notificationHubInformation.Name));
            Assert.IsFalse(string.IsNullOrWhiteSpace(notificationHubInformation.Name_Debug));
        }
    }
}
