using GitTrends.Shared;

namespace GitTrends.UnitTests;

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
		Assert.Multiple(() =>
		{
			Assert.That(tokenDTO, Is.Not.Null);
			Assert.That(tokenDTO.ClientId, Is.Not.Null);
			Assert.That(string.IsNullOrWhiteSpace(tokenDTO.ClientId), Is.False);
		});
	}

	[Test]
	public async Task GenerateGitTrendsOAuthTokenTest_InvalidDTO()
	{
		//Arrange
		GitHubToken? gitHubToken;
		var generateTokenDTO = new GenerateTokenDTO(string.Empty, string.Empty);
		var azureFunctionsApiService = ServiceCollection.ServiceProvider.GetRequiredService<AzureFunctionsApiService>();

		//Act
		gitHubToken = await azureFunctionsApiService.GenerateGitTrendsOAuthToken(generateTokenDTO, CancellationToken.None).ConfigureAwait(false);

		Assert.Multiple(() =>
		{
			//Assert
			Assert.That(gitHubToken, Is.Not.Null);
			Assert.That(GitHubToken.Empty.AccessToken, Is.Empty);
			Assert.That(GitHubToken.Empty.Scope, Is.Empty);
			Assert.That(GitHubToken.Empty.TokenType, Is.Empty);
		});
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
		Assert.Multiple(() =>
		{
			Assert.That(syncFusionDTO, Is.Not.Null);
			Assert.That(syncFusionDTO.LicenseKey, Is.Not.Null);
			Assert.That(string.IsNullOrWhiteSpace(syncFusionDTO.LicenseKey), Is.False);
			Assert.That(syncFusionDTO.LicenseVersion, Is.GreaterThan(0));
		});
	}

	[Test]
	public async Task GetStreamingManifestsTest()
	{
		//Arrange
		IReadOnlyDictionary<string, StreamingManifest>? streamingManifests;
		var azureFunctionsApiService = ServiceCollection.ServiceProvider.GetRequiredService<AzureFunctionsApiService>();

		//Act
		streamingManifests = await azureFunctionsApiService.GetStreamingManifests(CancellationToken.None).ConfigureAwait(false);

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(streamingManifests, Is.Not.Null);

			Assert.That(Uri.IsWellFormedUriString(streamingManifests[StreamingConstants.Chart].HlsUrl, UriKind.Absolute));
			Assert.That(Uri.IsWellFormedUriString(streamingManifests[StreamingConstants.Chart].ManifestUrl, UriKind.Absolute));

			Assert.That(Uri.IsWellFormedUriString(streamingManifests[StreamingConstants.EnableOrganizations].HlsUrl, UriKind.Absolute));
			Assert.That(Uri.IsWellFormedUriString(streamingManifests[StreamingConstants.EnableOrganizations].ManifestUrl, UriKind.Absolute));

			Assert.That(streamingManifests[StreamingConstants.EnableOrganizations].HlsUrl, Is.Not.EqualTo(streamingManifests[StreamingConstants.Chart].HlsUrl));
			Assert.That(streamingManifests[StreamingConstants.EnableOrganizations].ManifestUrl, Is.Not.EqualTo(streamingManifests[StreamingConstants.Chart].ManifestUrl));
		});
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
		Assert.Multiple(() =>
		{
			Assert.That(notificationHubInformation, Is.Not.Null);
			Assert.That(notificationHubInformation.ConnectionString, Is.Not.Null);
			Assert.That(notificationHubInformation.ConnectionString_Debug, Is.Not.Null);
			Assert.That(notificationHubInformation.Name, Is.Not.Null);
			Assert.That(notificationHubInformation.Name_Debug, Is.Not.Null);

			Assert.That(string.IsNullOrWhiteSpace(notificationHubInformation.ConnectionString), Is.False);
			Assert.That(string.IsNullOrWhiteSpace(notificationHubInformation.ConnectionString_Debug), Is.False);
			Assert.That(string.IsNullOrWhiteSpace(notificationHubInformation.Name), Is.False);
			Assert.That(string.IsNullOrWhiteSpace(notificationHubInformation.Name_Debug), Is.False);
		});
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
		Assert.That(nugetPackageModels, Is.Not.Null);
		Assert.That(nugetPackageModels, Is.Not.Empty);

		foreach (var nugetPackage in nugetPackageModels)
		{
			Assert.Multiple(() =>
			{
				Assert.That(nugetPackage.IconUri, Is.Not.Null);
				Assert.That(nugetPackage.IconUri.IsAbsoluteUri);
				Assert.That(nugetPackage.IconUri.IsWellFormedOriginalString());

				Assert.That(string.IsNullOrWhiteSpace(nugetPackage.PackageName), Is.False);

				Assert.That(nugetPackage.WebsiteUri, Is.Not.Null);
				Assert.That(nugetPackage.WebsiteUri.IsAbsoluteUri);
				Assert.That(nugetPackage.WebsiteUri.IsWellFormedOriginalString());
			});
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
		Assert.Multiple(() =>
		{
			Assert.That(gitTrendsStatisticsDTO, Is.Not.Null);
			Assert.That(gitTrendsStatisticsDTO.Contributors, Is.Not.Empty);

			Assert.That(gitTrendsStatisticsDTO.Forks, Is.GreaterThan(0));
			Assert.That(gitTrendsStatisticsDTO.Stars, Is.GreaterThan(0));
			Assert.That(gitTrendsStatisticsDTO.Watchers, Is.GreaterThan(0));

			Assert.That(gitTrendsStatisticsDTO.GitHubUri, Is.Not.Null);
			Assert.That(gitTrendsStatisticsDTO.GitHubUri.IsAbsoluteUri);
			Assert.That(gitTrendsStatisticsDTO.GitHubUri.IsWellFormedOriginalString());
		});
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
		Assert.Multiple(() =>
		{
			Assert.That(appCenterApiKeyDTO, Is.Not.Null);
			Assert.That(appCenterApiKeyDTO.iOS, Is.Not.Null);
			Assert.That(appCenterApiKeyDTO.Android, Is.Not.Null);
		});
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
		Assert.Multiple(() =>
		{
			Assert.That(appCenterApiKeyDTO, Is.Not.Null);
			Assert.That(appCenterApiKeyDTO.iOS, Is.Not.Null);
			Assert.That(appCenterApiKeyDTO.Android, Is.Not.Null);
		});
	}
}