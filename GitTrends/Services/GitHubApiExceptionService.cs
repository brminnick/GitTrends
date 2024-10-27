using System.Diagnostics.CodeAnalysis;
using System.Net;
using GitHubApiStatus;
using GitTrends.Common;
using Refit;

namespace GitTrends;

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
			ApiException { StatusCode: HttpStatusCode.Forbidden } apiException => gitHubApiStatusService.HasReachedMaximumApiCallLimit(apiException.Headers),
			GraphQLException graphQLException => gitHubApiStatusService.HasReachedMaximumApiCallLimit(graphQLException.ResponseHeaders),
			_ => false
		};
	}

	public static bool IsAbuseRateLimit(this IGitHubApiStatusService gitHubApiStatusService, in Exception exception, [NotNullWhen(true)] out TimeSpan? delta)
	{
		delta = null;

		return exception switch
		{
			ApiException { StatusCode: HttpStatusCode.Forbidden } apiException => gitHubApiStatusService.IsAbuseRateLimit(apiException.Headers, out delta),
			GraphQLException graphQLException => gitHubApiStatusService.IsAbuseRateLimit(graphQLException.ResponseHeaders, out delta),
			_ => false
		};
	}
}