using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using GitTrends.Shared;
using Newtonsoft.Json;
using Xamarin.Essentials.Interfaces;

namespace GitTrends
{
	public class LibrariesService
	{
		readonly IPreferences _preferences;
		readonly IAnalyticsService _analyticsService;
		readonly ImageCachingService _imageCachingService;
		readonly AzureFunctionsApiService _azureFunctionsApiService;

		public LibrariesService(IPreferences preferences,
								IAnalyticsService analyticsService,
								ImageCachingService imageCachingService,
								AzureFunctionsApiService azureFunctionsApiService)
		{
			_preferences = preferences;
			_analyticsService = analyticsService;
			_imageCachingService = imageCachingService;
			_azureFunctionsApiService = azureFunctionsApiService;
		}

		public IReadOnlyList<NuGetPackageModel> InstalledLibraries
		{
			get
			{
				var serializedInstalledNuGetPackages = _preferences.Get(nameof(InstalledLibraries), null);

				return serializedInstalledNuGetPackages is null
					? Array.Empty<NuGetPackageModel>()
					: JsonConvert.DeserializeObject<IReadOnlyList<NuGetPackageModel>>(serializedInstalledNuGetPackages) ?? throw new JsonException();
			}
			private set
			{
				var serializedInstalledNuGetPackages = JsonConvert.SerializeObject(value);
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

				InstalledLibraries = libraries.OrderBy(static x => x.PackageName).ToList();

				foreach (var nugetPackageModel in InstalledLibraries)
				{
					_imageCachingService.PreloadImage(nugetPackageModel.IconUri).SafeFireAndForget(ex =>
					{
						_analyticsService.Report(ex, new Dictionary<string, string>
						{
							{ nameof(nugetPackageModel.PackageName), nugetPackageModel.PackageName },
							{ nameof(nugetPackageModel.IconUri), nugetPackageModel.IconUri.ToString() }
						});
					});
				}
			}
		}
	}
}