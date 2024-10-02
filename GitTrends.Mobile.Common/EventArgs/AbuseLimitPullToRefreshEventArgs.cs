using GitTrends.Mobile.Common.Constants;

namespace GitTrends.Mobile.Common;

public class AbuseLimitPullToRefreshEventArgs(TimeSpan retry, bool willRetryAutomatically) : PullToRefreshFailedEventArgs(PullToRefreshFailedConstants.AbuseLimitReached,
			willRetryAutomatically switch
			{
				true => $"{PullToRefreshFailedConstants.GitHubApiAbuseLimit}.\n\n{string.Format(PullToRefreshFailedConstants.AbuseLimitAutomaticRetry, retry.TotalSeconds)}.",
				false => $"{PullToRefreshFailedConstants.GitHubApiAbuseLimit}.\n\n{string.Format(PullToRefreshFailedConstants.AbuseLimitManualRetry, retry.TotalSeconds)}.",
			},
			"OK", PullToRefreshFailedConstants.LearnMore)
{
}