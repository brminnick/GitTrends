using GitHubApiStatus;
using Microsoft.Extensions.Logging;

namespace GitTrends.Functions;

abstract class BaseGitHubApiService(IGitHubApiStatusService gitHubApiStatusService, ILogger<BaseGitHubApiService> logger)
{
	readonly ILogger _logger = logger;

	protected async Task<T> AttemptAndRetry_Functions<T>(Func<Task<T>> action)
	{
		var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(3));

		var rateLimits = await gitHubApiStatusService.GetApiRateLimits(cancellationTokenSource.Token).ConfigureAwait(false);
		var parentClasName = GetType().Name;

		var remainingRequestCount = parentClasName switch
		{
			nameof(GitHubApiV3Service) => rateLimits.RestApi.RemainingRequestCount,
			nameof(GitHubGraphQLApiService) => rateLimits.GraphQLApi.RemainingRequestCount,
			_ => throw new NotSupportedException($"{parentClasName} is not supported")
		};

		_logger.LogInformation($"Remaining Request Count for {parentClasName}: {remainingRequestCount}");

		if (remainingRequestCount < 4000)
			throw new InvalidOperationException("GitHub API Rate Limit Exceeded");

		return await action().WaitAsync(cancellationTokenSource.Token).ConfigureAwait(false);
	}
}