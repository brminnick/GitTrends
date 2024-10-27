using GitTrends.Common;
using Refit;

namespace GitTrends.Mobile.Common;

public abstract class AzureFunctionsApiService
{
	static readonly Lazy<IAzureFunctionsApi> _azureFunctionsApiClientHolder = new(() => RestService.For<IAzureFunctionsApi>(new HttpClient { BaseAddress = new Uri(AzureConstants.AzureFunctionsApiUrl) }));

	static IAzureFunctionsApi AzureFunctionsApiClient => _azureFunctionsApiClientHolder.Value;

	public static Task<GitHubToken> GetTestToken(CancellationToken token) => AzureFunctionsApiClient.GetTestToken(token);
}