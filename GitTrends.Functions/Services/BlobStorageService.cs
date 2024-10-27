using System.Text.Json;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using GitTrends.Common;

namespace GitTrends.Functions;

class BlobStorageService
{
	const string _libraryContainerName = "librarycache";
	const string _gitTrendsStatisticsContainerName = "gittrendsstatistics";

	readonly BlobServiceClient _blobServiceClient;

	public BlobStorageService(BlobServiceClient cloudBlobClient) => _blobServiceClient = cloudBlobClient;

	public Task UploadNuGetLibraries(IEnumerable<NuGetPackageModel> nuGetPackageModels, string blobName) => UploadValue(nuGetPackageModels, blobName, _libraryContainerName);

	public Task UploadStatistics(GitTrendsStatisticsDTO gitTrendsStatistics, string blobName) => UploadValue(gitTrendsStatistics, blobName, _gitTrendsStatisticsContainerName);

	public Task<GitTrendsStatisticsDTO> GetGitTrendsStatistics() => GetLatestValue<GitTrendsStatisticsDTO>(_gitTrendsStatisticsContainerName);

	public Task<IReadOnlyList<NuGetPackageModel>> GetNuGetLibraries() => GetLatestValue<IReadOnlyList<NuGetPackageModel>>(_libraryContainerName);

	async Task UploadValue<T>(T data, string blobName, string containerName)
	{
		var containerClient = GetBlobContainerClient(containerName);
		await containerClient.CreateIfNotExistsAsync().ConfigureAwait(false);

		var blobClient = containerClient.GetBlobClient(blobName);

		var blobContent = JsonSerializer.Serialize(data);
		await blobClient.UploadAsync(new BinaryData(blobContent)).ConfigureAwait(false);
	}

	async Task<T> GetLatestValue<T>(string containerName)
	{
		var blobList = new List<BlobItem>();
		await foreach (var blob in GetBlobs(containerName).ConfigureAwait(false))
		{
			blobList.Add(blob);
		}

		var newestBlob = blobList.OrderByDescending(static x => x.Properties.CreatedOn).First();

		var blobClient = GetBlobContainerClient(containerName).GetBlobClient(newestBlob.Name);
		var blobContentResponse = await blobClient.DownloadContentAsync().ConfigureAwait(false);

		var serializedBlobContents = blobContentResponse.Value.Content;

		return JsonSerializer.Deserialize<T>(serializedBlobContents.ToString()) ?? throw new NullReferenceException();
	}

	async IAsyncEnumerable<BlobItem> GetBlobs(string containerName)
	{
		var blobContainerClient = GetBlobContainerClient(containerName);

		await foreach (var blob in blobContainerClient.GetBlobsAsync().ConfigureAwait(false))
		{
			if (blob is not null)
				yield return blob;
		}
	}

	BlobContainerClient GetBlobContainerClient(string containerName) => _blobServiceClient.GetBlobContainerClient(containerName);
}