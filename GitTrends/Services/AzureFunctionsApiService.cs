using System;
using System.Threading.Tasks;
using Octokit;
using Refit;

namespace GitTrends
{
    abstract class AzureFunctionsApiService : BaseApiService
    {
        #region Constant Fields
        static readonly Lazy<IAzureFunctionsApi> _azureFunctionsApiClientHolder = new Lazy<IAzureFunctionsApi>(() => RestService.For<IAzureFunctionsApi>(CreateHttpClient(AzureConstants.AzureFunctionsApiUrl)));
        #endregion

        #region Properties
        static IAzureFunctionsApi AzureFunctionsApiClient => _azureFunctionsApiClientHolder.Value;
        #endregion

        #region Methods
        public static Task<string> GetGitHubClientId() => ExecutePollyFunction(() => AzureFunctionsApiClient.GetGitTrendsClientId(AzureConstants.GetGitHubClientIdApiKey));
        public static Task<string> GenerateGitTrendsOAuthToken(string loginCode) => ExecutePollyFunction(() => AzureFunctionsApiClient.GenerateGitTrendsOAuthToken(loginCode, AzureConstants.GenerateOAuthTokenApiKey));
        #endregion
    }
}
