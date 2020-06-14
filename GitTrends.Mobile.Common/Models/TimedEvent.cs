using System;
using System.Collections.Generic;
using System.Diagnostics;
using GitTrends.Shared;

namespace GitTrends.Mobile.Common
{
    public class TimedEvent : ITimedEvent
    {
        readonly Stopwatch _stopwatch = new Stopwatch();
        readonly string _trackIdentifier;
        readonly IAnalyticsService _analyticsService;

        public TimedEvent(in IAnalyticsService analyticsService, string trackIdentifier, IDictionary<string, string>? dictionary)
        {
            Data = dictionary ?? new Dictionary<string, string>();
            _trackIdentifier = trackIdentifier;
            _analyticsService = analyticsService;

            _stopwatch.Start();
        }

        public IDictionary<string, string> Data { get; }

        public void Dispose()
        {
            _stopwatch.Stop();

            Data.Add("Timed Event", $"{_stopwatch.Elapsed:ss\\.fff}s");

            _analyticsService.Track($"{_trackIdentifier} [Timed Event]", Data);
        }
    }
}
