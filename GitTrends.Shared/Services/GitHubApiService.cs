using System;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using Refit;

namespace GitTrends.Shared
{
    static class GitHubApiService
    {
        public static int GetNumberOfApiRequestsRemaining(in HttpResponseHeaders httpResponseHeaders)
        {
            var rateLimitRemainingHeader = httpResponseHeaders.Single(x => x.Key is "X-RateLimit-Remaining");
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
