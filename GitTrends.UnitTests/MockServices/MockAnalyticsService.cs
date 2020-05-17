using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using GitTrends.Shared;

namespace GitTrends.UnitTests
{
    class MockAnalyticsService : IAnalyticsService
    {
        public void Track(string trackIdentifier, IDictionary<string, string>? table = null)
        {

        }

        public void Track(string trackIdentifier, string key, string value) => Track(trackIdentifier, new Dictionary<string, string> { { key, value } });

        public ITimedEvent TrackTime(string trackIdentifier, IDictionary<string, string>? table = null) => new TimedEvent(trackIdentifier, table);

        public ITimedEvent TrackTime(string trackIdentifier, string key, string value) =>
            TrackTime(trackIdentifier, new Dictionary<string, string> { { key, value } });

        public void Report(Exception exception,
                                  string key,
                                  string value,
                                  [CallerMemberName] string callerMemberName = "",
                                  [CallerLineNumber] int lineNumber = 0,
                                  [CallerFilePath] string filePath = "")
        {
            Report(exception, new Dictionary<string, string>{ { key, value } }, callerMemberName, lineNumber, filePath);
        }

        public void Report(Exception exception,
                                  IDictionary<string, string>? properties = null,
                                  [CallerMemberName] string callerMemberName = "",
                                  [CallerLineNumber] int lineNumber = 0,
                                  [CallerFilePath] string filePath = "")
        {

        }

        public class TimedEvent : ITimedEvent
        {
            readonly string _trackIdentifier;

            public TimedEvent(string trackIdentifier, IDictionary<string, string>? dictionary)
            {
                Data = dictionary ?? new Dictionary<string, string>();
                _trackIdentifier = trackIdentifier;
            }

            public IDictionary<string, string> Data { get; }

            public void Dispose()
            {

            }
        }
    }
}
