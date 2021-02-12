using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using GitTrends.Shared;

namespace GitTrends.Functions
{
    class BlobStorageService
    {
        const string _libraryContainerName = "librarycache";
        readonly CloudBlobClient _blobClient;

        public BlobStorageService(CloudBlobClient cloudBlobClient) => _blobClient = cloudBlobClient;

        public Task UploadNuGetLibraries(IEnumerable<NuGetPackageModel> nuGetPackageModels, string blobName)
        {
            var container = GetBlobContainer(_libraryContainerName);
            container.CreateIfNotExistsAsync().ConfigureAwait(false);

            var blob = container.GetBlockBlobReference(blobName);
            return blob.UploadTextAsync(JsonConvert.SerializeObject(nuGetPackageModels));
        }

        public async Task<IReadOnlyList<NuGetPackageModel>> GetNuGetLibraries()
        {
            var blobList = new List<CloudBlockBlob>();
            await foreach (var blob in GetBlobs<CloudBlockBlob>(_libraryContainerName).ConfigureAwait(false))
            {
                blobList.Add(blob);
            }

            var nugetPackageModelBlob = blobList.OrderByDescending(x => x.Properties.Created).First();
            var serializedNuGetPackageList = await nugetPackageModelBlob.DownloadTextAsync().ConfigureAwait(false);

            return JsonConvert.DeserializeObject<IReadOnlyList<NuGetPackageModel>>(serializedNuGetPackageList) ?? throw new NullReferenceException();
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
