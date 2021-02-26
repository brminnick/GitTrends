using System;
using System.Threading;
using System.Threading.Tasks;
using GitHubApiStatus;
using GitTrends.Shared;
using Microsoft.Extensions.Logging;

namespace GitTrends.Functions
{
    abstract class BaseGitHubApiService : BaseApiService
    {
        readonly ILogger _logger;
        readonly IGitHubApiStatusService _gitHubApiStatusService;

        protected BaseGitHubApiService(IGitHubApiStatusService gitHubApiStatusService, ILogger<BaseGitHubApiService> logger) =>
            (_gitHubApiStatusService, _logger) = (gitHubApiStatusService, logger);

        protected async Task<T> AttemptAndRetry_Functions<T>(Func<Task<T>> action, CancellationToken cancellationToken, int numRetries = 3)
        {
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(3));

            var rateLimits = await _gitHubApiStatusService.GetApiRateLimits(cancellationTokenSource.Token).ConfigureAwait(false);
            var parentClasName = GetType().Name;

            int remainingRequestCount = parentClasName switch
            {
                nameof(GitHubApiV3Service) => rateLimits.RestApi.RemainingRequestCount,
                nameof(GitHubGraphQLApiService) => rateLimits.GraphQLApi.RemainingRequestCount,
                _ => throw new NotSupportedException($"{parentClasName} is not supported")
            };

            _logger.LogInformation($"Remaining Request Count for {parentClasName}: {remainingRequestCount}");

            if (remainingRequestCount < 4000)
                throw new InvalidOperationException("GitHub API Rate Limit Exceeded");

            return await AttemptAndRetry(action, cancellationToken, numRetries).ConfigureAwait(false);
        }
    }
}
