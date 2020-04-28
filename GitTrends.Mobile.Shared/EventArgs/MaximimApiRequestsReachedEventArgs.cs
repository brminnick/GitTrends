using System;

namespace GitTrends.Mobile.Shared
{
    public class MaximimApiRequestsReachedEventArgs : PullToRefreshFailedEventArgs
    {
        public MaximimApiRequestsReachedEventArgs(DateTimeOffset resetDateTime) : base("Usage Limit Exceeded", $"Please try again in {GetMinutesRemaining(resetDateTime)} minutes")
        {

        }

        public MaximimApiRequestsReachedEventArgs(long resetTime_UnixEpochSeconds) : this(DateTimeOffset.FromUnixTimeSeconds(resetTime_UnixEpochSeconds).ToLocalTime())
        {

        }

        static double GetMinutesRemaining(DateTimeOffset resetDateTime) =>
            Math.Round(resetDateTime.AddMinutes(1).Subtract(DateTimeOffset.UtcNow).TotalMinutes, MidpointRounding.AwayFromZero);
    }
}
