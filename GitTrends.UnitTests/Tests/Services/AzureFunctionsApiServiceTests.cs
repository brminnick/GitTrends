using GitTrends.Common;

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
	public async Task GetGitTrendsEnableOrganizationsUriTest()
	{
		//Arrange
		GitTrendsEnableOrganizationsUriDTO? gitTrendsEnableOrganizationsUriDTO;
		var azureFunctionsApiService = ServiceCollection.ServiceProvider.GetRequiredService<AzureFunctionsApiService>();

		//Act
		gitTrendsEnableOrganizationsUriDTO = await azureFunctionsApiService.GetGitTrendsEnableOrganizationsUri(CancellationToken.None).ConfigureAwait(false);

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(gitTrendsEnableOrganizationsUriDTO, Is.Not.Null);
			Assert.That(gitTrendsEnableOrganizationsUriDTO.Uri, Is.Not.Null);
		});
	}
}