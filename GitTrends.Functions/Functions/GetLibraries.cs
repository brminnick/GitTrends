using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GitTrends.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace GitTrends.Functions
{
    class GetLibraries
    {
        readonly BlobStorageService _blobStorageService;

        public GetLibraries(BlobStorageService blobStorageService) => _blobStorageService = blobStorageService;

        [FunctionName(nameof(GetLibraries))]
        public Task<IReadOnlyList<NuGetPackageModel>> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest request, ILogger log) => _blobStorageService.GetNuGetLibraries();
    }
}