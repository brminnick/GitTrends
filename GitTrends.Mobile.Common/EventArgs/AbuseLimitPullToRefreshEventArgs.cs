using System;
using GitTrends.Mobile.Common.Constants;

namespace GitTrends.Mobile.Common
{
    public class AbuseLimitPullToRefreshEventArgs : PullToRefreshFailedEventArgs
    {
        public AbuseLimitPullToRefreshEventArgs(TimeSpan retry, bool shouldIncludRetryMessage)
            : base(PullToRefreshFailedConstants.AbuseLimitReached,
                    shouldIncludRetryMessage switch
                    {
                        true => $"{PullToRefreshFailedConstants.GitHubApiAbuseLimit}.\n\n{string.Format(PullToRefreshFailedConstants.AbuseLimitAutomaticRetry, retry.TotalSeconds)}.",
                        false => PullToRefreshFailedConstants.GitHubApiLimit
                    },
                    "OK", PullToRefreshFailedConstants.LearnMore)
        {

        }
    }
}
