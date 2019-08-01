using System;
using System.Reflection;
using System.Threading.Tasks;
using GitTrends.Shared;
using Refit;

namespace GitTrends
{
    public abstract class AzureFunctionsApiService : BaseMobileApiService
    {
        #region Constant Fields
        static readonly Lazy<IAzureFunctionsApi> _azureFunctionsApiClientHolder = new Lazy<IAzureFunctionsApi>(() => RestService.For<IAzureFunctionsApi>(CreateHttpClient(AzureConstants.AzureFunctionsApiUrl)));
        #endregion

        #region Properties
        static IAzureFunctionsApi AzureFunctionsApiClient => _azureFunctionsApiClientHolder.Value;
        #endregion

        #region Methods
        public static Task<GetGitHubClientIdDTO> GetGitHubClientId() => ExecuteMobilePollyFunction(() => AzureFunctionsApiClient.GetGitTrendsClientId());
        public static Task<GitHubToken> GenerateGitTrendsOAuthToken(GenerateTokenDTO generateTokenDTO) => ExecuteMobilePollyFunction(() => AzureFunctionsApiClient.GenerateGitTrendsOAuthToken(generateTokenDTO));
        public static Task<SyncFusionDTO> GetSyncFusionInformation()
        {
            var syncfusionVersionNumber = Assembly.GetAssembly(typeof(Syncfusion.CoreAssembly)).GetName().Version.ToString();
            return ExecuteMobilePollyFunction(() => AzureFunctionsApiClient.GetSyncFusionInformation(syncfusionVersionNumber));
        }
        #endregion
    }
}
