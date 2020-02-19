using System;
using System.Threading.Tasks;
using GitTrends.Shared;
using Refit;

namespace GitTrends.UITests
{
    abstract class AzureFunctionsApiService : BaseApiService
    {
        readonly static Lazy<IAzureFunctionsApi> _azureFunctionsApiClientHolder = new Lazy<IAzureFunctionsApi>(() => RestService.For<IAzureFunctionsApi>(CreateHttpClient(AzureConstants.AzureFunctionsApiUrl)));

        static IAzureFunctionsApi AzureFunctionsApiClient => _azureFunctionsApiClientHolder.Value;

        public static Task<GitHubToken> GenerateGitTrendsOAuthToken(GenerateTokenDTO generateTokenDTO) => AttemptAndRetry(() => AzureFunctionsApiClient.GenerateGitTrendsOAuthToken(generateTokenDTO));
    }
}