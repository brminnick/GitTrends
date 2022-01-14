using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using GitTrends.Shared;
using Newtonsoft.Json;

namespace GitTrends.Functions
{
	class BlobStorageService
	{
		const string _libraryContainerName = "librarycache";
		const string _gitTrendsStatisticsContainerName = "gittrendsstatistics";

		readonly BlobServiceClient _blobClient;

		public BlobStorageService(BlobServiceClient cloudBlobClient) => _blobClient = cloudBlobClient;

		public Task UploadNuGetLibraries(IEnumerable<NuGetPackageModel> nuGetPackageModels, string blobName) => UploadValue(nuGetPackageModels, blobName, _libraryContainerName);

		public Task UploadStatistics(GitTrendsStatisticsDTO gitTrendsStatistics, string blobName) => UploadValue(gitTrendsStatistics, blobName, _gitTrendsStatisticsContainerName);

		public Task<GitTrendsStatisticsDTO> GetGitTrendsStatistics() => GetLatestValue<GitTrendsStatisticsDTO>(_gitTrendsStatisticsContainerName);

		public Task<IReadOnlyList<NuGetPackageModel>> GetNuGetLibraries() => GetLatestValue<IReadOnlyList<NuGetPackageModel>>(_libraryContainerName);

		async Task UploadValue<T>(T data, string blobName, string containerName)
		{
			var containerClient = GetBlobContainerClient(containerName);
			await containerClient.CreateIfNotExistsAsync().ConfigureAwait(false);

			var blobUri = new Uri($"{containerClient.Uri}/{blobName}");
			var blobClient = new BlobClient(blobUri);

			var blobContent = JsonConvert.SerializeObject(data);

			await blobClient.UploadAsync(new BinaryData(blobContent)).ConfigureAwait(false);
		}

		async Task<T> GetLatestValue<T>(string containerName)
		{
			var blobList = new List<BlobItem>();
			await foreach (var blob in GetBlobs(containerName).ConfigureAwait(false))
			{
				blobList.Add(blob);
			}

			var newestBlob = blobList.OrderByDescending(x => x.Properties.CreatedOn).First();

			var blobClient = new BlobClient(new Uri($"{GetBlobContainerClient(containerName).Uri}/{newestBlob.Name}"));
			var blobContentResponse = await blobClient.DownloadContentAsync().ConfigureAwait(false);

			var serializedBlobContents = blobContentResponse.Value.Content;

			return JsonConvert.DeserializeObject<T>(serializedBlobContents.ToString()) ?? throw new NullReferenceException();
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

		BlobContainerClient GetBlobContainerClient(string containerName) => _blobClient.GetBlobContainerClient(containerName);
	}
}