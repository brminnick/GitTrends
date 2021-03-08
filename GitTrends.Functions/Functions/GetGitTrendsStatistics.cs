using System;
using System.Threading.Tasks;
using GitTrends.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace GitTrends.Functions
{
    class GetGitTrendsStatistics
    {
        readonly BlobStorageService _blobStorageService;

        public GetGitTrendsStatistics(BlobStorageService blobStorageService) => _blobStorageService = blobStorageService;

        [FunctionName(nameof(GetGitTrendsStatistics))]
        public Task<GitTrendsStatisticsDTO> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest request, ILogger log) => _blobStorageService.GetGitTrendsStatistics();
    }
}
