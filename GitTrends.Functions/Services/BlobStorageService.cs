using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GitTrends.Shared;
using Microsoft.Azure.Storage.Blob;
using Newtonsoft.Json;

namespace GitTrends.Functions
{
    class BlobStorageService
    {
        const string _libraryContainerName = "librarycache";
        const string _gitTrendsStatisticsContainerName = "gittrendsstatistics";

        readonly CloudBlobClient _blobClient;

        public BlobStorageService(CloudBlobClient cloudBlobClient) => _blobClient = cloudBlobClient;

        public Task UploadNuGetLibraries(IEnumerable<NuGetPackageModel> nuGetPackageModels, string blobName) => UploadValue(nuGetPackageModels, blobName, _libraryContainerName);

        public Task UploadStatistics(GitTrendsStatisticsDTO gitTrendsStatistics, string blobName) => UploadValue(gitTrendsStatistics, blobName, _gitTrendsStatisticsContainerName);

        public Task<GitTrendsStatisticsDTO> GetGitTrendsStatistics() => GetLatestValue<GitTrendsStatisticsDTO>(_gitTrendsStatisticsContainerName);

        public Task<IReadOnlyList<NuGetPackageModel>> GetNuGetLibraries() => GetLatestValue<IReadOnlyList<NuGetPackageModel>>(_libraryContainerName);

        async Task UploadValue<T>(T data, string blobName, string containerName)
        {
            var container = GetBlobContainer(containerName);
            await container.CreateIfNotExistsAsync().ConfigureAwait(false);

            var blob = container.GetBlockBlobReference(blobName);
            await blob.UploadTextAsync(JsonConvert.SerializeObject(data)).ConfigureAwait(false);
        }

        async Task<T> GetLatestValue<T>(string containerName)
        {
            var blobList = new List<CloudBlockBlob>();
            await foreach (var blob in GetBlobs<CloudBlockBlob>(containerName).ConfigureAwait(false))
            {
                blobList.Add(blob);
            }

            var newestBlob = blobList.OrderByDescending(x => x.Properties.Created).First();
            var serializedBlobContents = await newestBlob.DownloadTextAsync().ConfigureAwait(false);

            return JsonConvert.DeserializeObject<T>(serializedBlobContents) ?? throw new NullReferenceException();
        }

        async IAsyncEnumerable<T> GetBlobs<T>(string containerName, string prefix = "", int? maxresultsPerQuery = null, BlobListingDetails blobListingDetails = BlobListingDetails.None) where T : ICloudBlob
        {
            var blobContainer = GetBlobContainer(containerName);

            BlobContinuationToken? continuationToken = null;

            do
            {
                var response = await blobContainer.ListBlobsSegmentedAsync(prefix, true, blobListingDetails, maxresultsPerQuery, continuationToken, null, null).ConfigureAwait(false);
                continuationToken = response?.ContinuationToken;

                var blobListFromResponse = response?.Results?.OfType<T>() ?? Enumerable.Empty<T>();

                foreach (var blob in blobListFromResponse)
                {
                    yield return blob;
                }

            } while (continuationToken != null);

        }

        CloudBlobContainer GetBlobContainer(string containerName) => _blobClient.GetContainerReference(containerName);
    }
}
