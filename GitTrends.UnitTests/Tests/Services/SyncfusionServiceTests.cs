namespace GitTrends.UnitTests;

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
		Assert.IsTrue(IsBase64String(license_final ?? throw new InvalidOperationException($"{nameof(license_final)} cannot be null")));
		Assert.Greater(SyncfusionService.AssemblyVersionNumber, 0);
	}

	static bool IsBase64String(string text)
	{
		var buffer = new Span<byte>(new byte[text.Length]);
		return Convert.TryFromBase64String(text, buffer, out _);
	}
}