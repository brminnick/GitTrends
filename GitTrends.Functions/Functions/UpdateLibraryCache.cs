using GitTrends.Shared;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GitTrends.Functions;

class UpdateLibraryCache
{
	readonly NuGetService _nuGetService;
	readonly BlobStorageService _blobStorageService;

	public UpdateLibraryCache(NuGetService nuGetService, BlobStorageService blobStorageService) =>
		(_nuGetService, _blobStorageService) = (nuGetService, blobStorageService);

	[Function(nameof(UpdateLibraryCache))]
	public async Task Run([TimerTrigger("0 0 0 * * *")] TimerInfo myTimer, FunctionContext context)
	{
		var log = context.GetLogger<UpdateLibraryCache>();
		var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(15));

		log.LogInformation("Retrieving NuGet Packages");

		var nugetPackageDictionary = new Dictionary<string, (Uri IconUri, Uri NugetUri)>();
		await foreach (var (Title, ImageUri, NugetUri) in _nuGetService.GetPackageInfo(cancellationTokenSource.Token))
		{
			log.LogInformation($"Found {Title}");
			if (!nugetPackageDictionary.Any(x => x.Key.Equals(Title, StringComparison.OrdinalIgnoreCase)))
			{
				log.LogInformation($"Added NuGet Package: {Title}");
				nugetPackageDictionary.Add(Title, (ImageUri, NugetUri));
			}
		}

		var nugetPackageModelList = new List<NuGetPackageModel>();
		foreach (var nugetPackage in nugetPackageDictionary)
			nugetPackageModelList.Add(new NuGetPackageModel(nugetPackage.Key, nugetPackage.Value.IconUri, nugetPackage.Value.NugetUri));

		var blobName = $"Libraries_{DateTime.UtcNow:o}.json";
		log.LogInformation($"Saving NuGet Pacakges to Blob Storage: {blobName}");

		await _blobStorageService.UploadNuGetLibraries(nugetPackageModelList, blobName).ConfigureAwait(false);
	}
}