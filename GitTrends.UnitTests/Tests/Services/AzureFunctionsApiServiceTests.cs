using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using GitTrends.Shared;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Refit;

namespace GitTrends.UnitTests
{
    class AzureFunctionsApiServiceTests : BaseTest
    {
        [Test]
        public async Task GetGitHubClientIdTest()
        {
            //Arrange
            GetGitHubClientIdDTO? tokenDTO;
            var azureFunctionsApiService = ServiceCollection.ServiceProvider.GetRequiredService<AzureFunctionsApiService>();

            //Act
            tokenDTO = await azureFunctionsApiService.GetGitHubClientId(CancellationToken.None).ConfigureAwait(false);

            //Assert
            Assert.IsNotNull(tokenDTO);
            Assert.IsNotNull(tokenDTO.ClientId);
            Assert.IsFalse(string.IsNullOrWhiteSpace(tokenDTO.ClientId));
        }

        [Test]
        public void GenerateGitTrendsOAuthTokenTest_InvalidDTO()
        {
            //Arrange
            var generateTokenDTO = new GenerateTokenDTO(string.Empty, string.Empty);
            var azureFunctionsApiService = ServiceCollection.ServiceProvider.GetRequiredService<AzureFunctionsApiService>();

            //Act
            var apiException = Assert.ThrowsAsync<ApiException>(() => azureFunctionsApiService.GenerateGitTrendsOAuthToken(generateTokenDTO, CancellationToken.None));

            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, apiException?.StatusCode);
        }

        [Test]
        public async Task GetSyncfusionInformationTest()
        {
            //Arrange
            SyncFusionDTO? syncFusionDTO;
            var azureFunctionsApiService = ServiceCollection.ServiceProvider.GetRequiredService<AzureFunctionsApiService>();

            //Act
            syncFusionDTO = await azureFunctionsApiService.GetSyncfusionInformation(CancellationToken.None).ConfigureAwait(false);

            //Assert
            Assert.IsNotNull(syncFusionDTO);
            Assert.IsNotNull(syncFusionDTO.LicenseKey);
            Assert.IsFalse(string.IsNullOrWhiteSpace(syncFusionDTO.LicenseKey));

            Assert.Greater(syncFusionDTO.LicenseVersion, 0);
        }

        [Test]
        public async Task GetChartStreamingUrlTest()
        {
            //Arrange
            StreamingManifest? streamingManifest;
            var azureFunctionsApiService = ServiceCollection.ServiceProvider.GetRequiredService<AzureFunctionsApiService>();

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
        public async Task GetNotificationHubInformationTest()
        {
            //Arrange
            NotificationHubInformation? notificationHubInformation;
            var azureFunctionsApiService = ServiceCollection.ServiceProvider.GetRequiredService<AzureFunctionsApiService>();

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

        [Test]
        public async Task GetLibrariesTest()
        {
            //Arrange
            IReadOnlyList<NuGetPackageModel> nugetPackageModels;
            var azureFunctionsApiService = ServiceCollection.ServiceProvider.GetRequiredService<AzureFunctionsApiService>();

            //Act
            nugetPackageModels = await azureFunctionsApiService.GetLibraries(CancellationToken.None).ConfigureAwait(false);

            //Assert
            Assert.IsNotNull(nugetPackageModels);
            Assert.IsNotEmpty(nugetPackageModels);

            foreach (var nugetPackage in nugetPackageModels)
            {
                Assert.IsNotNull(nugetPackage.IconUri);
                Assert.IsTrue(nugetPackage.IconUri.IsAbsoluteUri);
                Assert.IsTrue(nugetPackage.IconUri.IsWellFormedOriginalString());

                Assert.IsFalse(string.IsNullOrWhiteSpace(nugetPackage.PackageName));

                Assert.IsNotNull(nugetPackage.WebsiteUri);
                Assert.IsTrue(nugetPackage.WebsiteUri.IsAbsoluteUri);
                Assert.IsTrue(nugetPackage.WebsiteUri.IsWellFormedOriginalString());
            }
        }

        [Test]
        public async Task GetGitTrendsStatisticsTest()
        {
            //Arrange
            GitTrendsStatisticsDTO? gitTrendsStatisticsDTO;
            var azureFunctionsApiService = ServiceCollection.ServiceProvider.GetRequiredService<AzureFunctionsApiService>();

            //Act
            gitTrendsStatisticsDTO = await azureFunctionsApiService.GetGitTrendsStatistics(CancellationToken.None).ConfigureAwait(false);

            //Assert
            Assert.IsNotNull(gitTrendsStatisticsDTO);
            Assert.IsNotEmpty(gitTrendsStatisticsDTO.Contributors);

            Assert.Greater(gitTrendsStatisticsDTO.Forks, 0);
            Assert.Greater(gitTrendsStatisticsDTO.Stars, 0);
            Assert.Greater(gitTrendsStatisticsDTO.Watchers, 0);

            Assert.IsNotNull(gitTrendsStatisticsDTO.GitHubUri);
            Assert.IsTrue(gitTrendsStatisticsDTO.GitHubUri.IsAbsoluteUri);
            Assert.IsTrue(gitTrendsStatisticsDTO.GitHubUri.IsWellFormedOriginalString());
        }

        [Test]
        public async Task GetAppCenterApiKeysTest()
        {
            //Arrange
            AppCenterApiKeyDTO? appCenterApiKeyDTO;
            var azureFunctionsApiService = ServiceCollection.ServiceProvider.GetRequiredService<AzureFunctionsApiService>();

            //Act
            appCenterApiKeyDTO = await azureFunctionsApiService.GetAppCenterApiKeys(CancellationToken.None).ConfigureAwait(false);

            //Assert
            Assert.IsNotNull(appCenterApiKeyDTO);
            Assert.IsNotNull(appCenterApiKeyDTO.iOS);
            Assert.IsNotNull(appCenterApiKeyDTO.Android);
        }

        [Test]
        public async Task GetGitTrendsEnableOrganizationsUriTest()
        {
            //Arrange
            AppCenterApiKeyDTO? appCenterApiKeyDTO;
            var azureFunctionsApiService = ServiceCollection.ServiceProvider.GetRequiredService<AzureFunctionsApiService>();

            //Act
            appCenterApiKeyDTO = await azureFunctionsApiService.GetAppCenterApiKeys(CancellationToken.None).ConfigureAwait(false);

            //Assert
            Assert.IsNotNull(appCenterApiKeyDTO);
            Assert.IsNotNull(appCenterApiKeyDTO.iOS);
            Assert.IsNotNull(appCenterApiKeyDTO.Android);
        }
    }
}
