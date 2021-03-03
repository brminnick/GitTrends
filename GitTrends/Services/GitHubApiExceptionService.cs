using System;
using System.Net;
using GitHubApiStatus;
using GitTrends.Shared;
using Refit;

namespace GitTrends
{
    public static class GitHubApiExceptionService
    {
        public static bool HasReachedMaximumApiCallLimit(this IGitHubApiStatusService gitHubApiStatusService, in Exception exception)
        {
            var doesContainGitHubRateLimitRemainingHeader = exception switch
            {
                ApiException apiException => apiException.Headers.DoesContainGitHubRateLimitRemainingHeader(),
                GraphQLException graphQLException => graphQLException.ResponseHeaders.DoesContainGitHubRateLimitRemainingHeader(),
                _ => false
            };

            if (!doesContainGitHubRateLimitRemainingHeader)
                return false;

            return exception switch
            {
                ApiException apiException when apiException.StatusCode is HttpStatusCode.Forbidden => gitHubApiStatusService.HasReachedMaximimApiCallLimit(apiException.Headers),
                GraphQLException graphQLException => gitHubApiStatusService.HasReachedMaximimApiCallLimit(graphQLException.ResponseHeaders),
                _ => false
            };
        }

        public static bool IsAbuseRateLimit(this IGitHubApiStatusService gitHubApiStatusService, in Exception exception, out TimeSpan? delta)
        {
            delta = null;

            return exception switch
            {
                ApiException apiException when apiException.StatusCode is HttpStatusCode.Forbidden => gitHubApiStatusService.IsAbuseRateLimit(apiException.Headers, out delta),
                GraphQLException graphQLException => gitHubApiStatusService.IsAbuseRateLimit(graphQLException.ResponseHeaders, out delta),
                _ => false
            };
        }
    }
}
