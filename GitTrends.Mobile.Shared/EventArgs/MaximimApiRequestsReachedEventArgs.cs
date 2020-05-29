using System;

namespace GitTrends.Mobile.Shared
{
    public class MaximimApiRequestsReachedEventArgs : PullToRefreshFailedEventArgs
    {
        public MaximimApiRequestsReachedEventArgs(DateTimeOffset resetDateTime) : base("Usage Limit Exceeded", $"The GitHub API limits our API requests to 5,000 requests per user per hour.\n\nThe current limit is scheduled to reset in {GetMinutesRemaining(resetDateTime)} minutes.", "OK", "Learn More")
        {

        }

        public MaximimApiRequestsReachedEventArgs(long resetTime_UnixEpochSeconds) : this(DateTimeOffset.FromUnixTimeSeconds(resetTime_UnixEpochSeconds).ToLocalTime())
        {

        }

        static double GetMinutesRemaining(DateTimeOffset resetDateTime) =>
            Math.Round(resetDateTime.AddMinutes(1).Subtract(DateTimeOffset.UtcNow).TotalMinutes, MidpointRounding.AwayFromZero);
    }
}
