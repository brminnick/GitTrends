using System;
using System.Threading;
using System.Threading.Tasks;
using GitTrends.Shared;

namespace GitTrends.Mobile.Common;

public abstract class AzureFunctionsApiService : BaseApiService
{
	readonly static Lazy<IAzureFunctionsApi> _azureFunctionsApiClientHolder = new(() => RefitExtensions.For<IAzureFunctionsApi>(CreateHttpClient(AzureConstants.AzureFunctionsApiUrl)));

	static IAzureFunctionsApi AzureFunctionsApiClient => _azureFunctionsApiClientHolder.Value;

	public static Task<GitHubToken> GetTestToken() => AttemptAndRetry(() => AzureFunctionsApiClient.GetTestToken(), CancellationToken.None);
}