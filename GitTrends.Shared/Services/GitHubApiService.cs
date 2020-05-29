using System;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using Refit;

namespace GitTrends.Shared
{
    static class GitHubApiService
    {
        public static bool HasReachedMaximimApiCallLimit(in HttpResponseHeaders httpResponseHeaders)
        {
            var rateLimitRemainingHeader = httpResponseHeaders.SingleOrDefault(x => x.Key is "X-RateLimit-Remaining");
            if (rateLimitRemainingHeader.Key == default && rateLimitRemainingHeader.Value == default)
                return false;

            var remainingApiRequests = int.Parse(rateLimitRemainingHeader.Value.First());
            return remainingApiRequests <= 0;
        }

        public static bool HasReachedMaximimApiCallLimit(in ApiException exception)
        {
            if (exception.StatusCode != HttpStatusCode.Forbidden)
                return false;

            return HasReachedMaximimApiCallLimit(exception.Headers);
        }

        public static DateTimeOffset GetRateLimitResetDateTime(in HttpResponseHeaders httpResponseHeaders) =>
            DateTimeOffset.FromUnixTimeSeconds(GetRateLimitResetDateTime_UnixEpochSeconds(httpResponseHeaders));

        public static long GetRateLimitResetDateTime_UnixEpochSeconds(in HttpResponseHeaders httpResponseHeaders)
        {
            if (!HasReachedMaximimApiCallLimit(httpResponseHeaders))
                throw new ArgumentException("Maximum API Call Limit Has Not Been Reached");

            var rateLimitResetHeader = httpResponseHeaders.Single(x => x.Key is "X-RateLimit-Reset");
            return long.Parse(rateLimitResetHeader.Value.First());
        }
    }
}
