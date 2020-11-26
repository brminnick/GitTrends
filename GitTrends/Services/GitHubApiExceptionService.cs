using System;
using System.Net;
using GitHubApiStatus;
using GitTrends.Shared;
using Refit;

namespace GitTrends
{
    public class GitHubApiExceptionService
    {
        readonly GitHubApiStatusService _gitHubApiStatusService;

        public GitHubApiExceptionService(GitHubApiStatusService gitHubApiStatusService) => _gitHubApiStatusService = gitHubApiStatusService;

        public bool HasReachedMaximimApiCallLimit(in Exception exception) => exception switch
        {
            ApiException apiException when apiException.StatusCode is HttpStatusCode.Forbidden => _gitHubApiStatusService.HasReachedMaximimApiCallLimit(apiException.Headers),
            GraphQLException graphQLException => _gitHubApiStatusService.HasReachedMaximimApiCallLimit(graphQLException.ResponseHeaders),
            _ => false
        };
    }
}
