using GitTrends.Common;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GitTrends.Functions;

class UpdateLibraryCache(NuGetService nuGetService, BlobStorageService blobStorageService)
{
	readonly NuGetService _nuGetService = nuGetService;
	readonly BlobStorageService _blobStorageService = blobStorageService;

	[Function(nameof(UpdateLibraryCache))]
	public async Task Run([TimerTrigger("0 0 0 * * *")] TimerInfo myTimer, FunctionContext context)
	{
		var log = context.GetLogger<UpdateLibraryCache>();
		var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(15));

		log.LogInformation("Retrieving NuGet Packages");

		var nugetPackageDictionary = new Dictionary<string, (Uri IconUri, Uri NugetUri)>();
		await foreach (var (title, imageUri, nugetUri) in _nuGetService.GetPackageInfo(cancellationTokenSource.Token).ConfigureAwait(false))
		{
			log.LogInformation($"Found {title}");
			if (!nugetPackageDictionary.Any(x => x.Key.Equals(title, StringComparison.OrdinalIgnoreCase)))
			{
				log.LogInformation($"Added NuGet Package: {title}");
				nugetPackageDictionary.Add(title, (imageUri, nugetUri));
			}
		}

		var nugetPackageModelList = new List<NuGetPackageModel>();
		foreach (var nugetPackage in nugetPackageDictionary)
			nugetPackageModelList.Add(new NuGetPackageModel(nugetPackage.Key, nugetPackage.Value.IconUri, nugetPackage.Value.NugetUri));

		var blobName = $"Libraries_{DateTime.UtcNow:o}.json";
		log.LogInformation($"Saving NuGet Packages to Blob Storage: {blobName}");

		await _blobStorageService.UploadNuGetLibraries(nugetPackageModelList, blobName).ConfigureAwait(false);
	}
}