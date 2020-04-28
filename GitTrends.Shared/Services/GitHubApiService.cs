using System;
using System.Linq;
using System.Net;
using Refit;

namespace GitTrends.Shared
{
    static class GitHubApiService
    {
        public static bool HasReachedMaximimApiCallLimit(ApiException exception)
        {
            if (exception.StatusCode != HttpStatusCode.Forbidden)
                return false;

            var rateLimitRemainingHeader = exception.Headers.SingleOrDefault(x => x.Key is "X-RateLimit-Remaining");
            if (rateLimitRemainingHeader.Key == default && rateLimitRemainingHeader.Value == default)
                return false;

            var remainingApiRequests = int.Parse(rateLimitRemainingHeader.Value.First());
            return remainingApiRequests <= 0;
        }

        public static DateTimeOffset GetRateLimitResetDateTime(ApiException exception) =>
            DateTimeOffset.FromUnixTimeSeconds(GetRateLimitResetDateTime_UnixEpochSeconds(exception));

        public static long GetRateLimitResetDateTime_UnixEpochSeconds(ApiException exception)
        {
            if (!HasReachedMaximimApiCallLimit(exception))
                throw new ArgumentException("Maximum API Call Limit Has Not Been Reached");

            var rateLimitResetHeader = exception.Headers.Single(x => x.Key is "X-RateLimit-Reset");
            return long.Parse(rateLimitResetHeader.Value.First());
        }
    }
}
