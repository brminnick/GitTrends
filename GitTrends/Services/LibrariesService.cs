using System.Text.Json;
using AsyncAwaitBestPractices;
using GitTrends.Common;

namespace GitTrends;

public class LibrariesService(IPreferences preferences,
								IAnalyticsService analyticsService,
								AzureFunctionsApiService azureFunctionsApiService)
{
	readonly IPreferences _preferences = preferences;
	readonly IAnalyticsService _analyticsService = analyticsService;
	readonly AzureFunctionsApiService _azureFunctionsApiService = azureFunctionsApiService;

	public IReadOnlyList<NuGetPackageModel> InstalledLibraries
	{
		get
		{
			var serializedInstalledNuGetPackages = _preferences.Get<string?>(nameof(InstalledLibraries), null);

			return serializedInstalledNuGetPackages is not null
				? JsonSerializer.Deserialize<IReadOnlyList<NuGetPackageModel>>(serializedInstalledNuGetPackages) ?? throw new JsonException()
				: [];
		}
		private set
		{
			var serializedInstalledNuGetPackages = JsonSerializer.Serialize(value);
			_preferences.Set(nameof(InstalledLibraries), serializedInstalledNuGetPackages);
		}
	}

	public async ValueTask Initialize(CancellationToken cancellationToken)
	{
		if (InstalledLibraries.Any())
			initialize().SafeFireAndForget(ex => _analyticsService.Report(ex));
		else
			await initialize().ConfigureAwait(false);

		async Task initialize()
		{
			var libraries = await _azureFunctionsApiService.GetLibraries(cancellationToken).ConfigureAwait(false);

			InstalledLibraries = [.. libraries.OrderBy(static x => x.PackageName)];
		}
	}
}