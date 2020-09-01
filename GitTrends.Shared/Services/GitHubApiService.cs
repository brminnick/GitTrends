using System;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using Refit;

namespace GitTrends.Shared
{
    public static class GitHubApiService
    {
        public const string RateLimitRemainingHeader = "X-RateLimit-Remaining";
        public const string RateLimitResetHeader = "X-RateLimit-Reset";

        public static int GetNumberOfApiRequestsRemaining(in HttpResponseHeaders httpResponseHeaders)
        {
            var rateLimitRemainingHeader = httpResponseHeaders.First(x => x.Key.Equals(RateLimitRemainingHeader, StringComparison.OrdinalIgnoreCase));
            var remainingApiRequests = int.Parse(rateLimitRemainingHeader.Value.First());

            return remainingApiRequests;
        }

        public static bool HasReachedMaximimApiCallLimit(in HttpResponseHeaders httpResponseHeaders)
        {
            var remainingApiRequests = GetNumberOfApiRequestsRemaining(httpResponseHeaders);
            return remainingApiRequests <= 0;
        }

        public static bool HasReachedMaximimApiCallLimit(in Exception exception)
        {
            if (exception is ApiException apiException && apiException.StatusCode is HttpStatusCode.Forbidden)
                return HasReachedMaximimApiCallLimit(apiException.Headers);

            return false;
        }

        public static bool IsUserAuthenticated(HttpResponseHeaders httpResponseHeaders) => httpResponseHeaders.Vary.Any(x => x is "Authorization");

        public static DateTimeOffset GetRateLimitResetDateTime(in HttpResponseHeaders httpResponseHeaders) =>
            DateTimeOffset.FromUnixTimeSeconds(GetRateLimitResetDateTime_UnixEpochSeconds(httpResponseHeaders));

        public static long GetRateLimitResetDateTime_UnixEpochSeconds(in HttpResponseHeaders httpResponseHeaders)
        {
            if (!HasReachedMaximimApiCallLimit(httpResponseHeaders))
                throw new ArgumentException("Maximum API Call Limit Has Not Been Reached");

            var rateLimitResetHeader = httpResponseHeaders.First(x => x.Key.Equals(RateLimitResetHeader, StringComparison.OrdinalIgnoreCase));
            return long.Parse(rateLimitResetHeader.Value.First());
        }
    }
}
