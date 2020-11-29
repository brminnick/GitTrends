using System;
using System.Net;
using GitHubApiStatus;
using GitTrends.Shared;
using Refit;

namespace GitTrends
{
    public static class GitHubApiExceptionService
    {
        public static bool HasReachedMaximimApiCallLimit(this IGitHubApiStatusService gitHubApiStatusService, in Exception exception) => exception switch
        {
            ApiException apiException when apiException.StatusCode is HttpStatusCode.Forbidden => gitHubApiStatusService.HasReachedMaximimApiCallLimit(apiException.Headers),
            GraphQLException graphQLException => gitHubApiStatusService.HasReachedMaximimApiCallLimit(graphQLException.ResponseHeaders),
            _ => false
        };
    }
}
