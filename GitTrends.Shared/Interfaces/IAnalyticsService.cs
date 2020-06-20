using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace GitTrends.Shared
{
    public interface IAnalyticsService
    {
        public void Track(string trackIdentifier, IDictionary<string, string>? table = null);

        public void Track(string trackIdentifier, string key, string value);

        public ITimedEvent TrackTime(string trackIdentifier, IDictionary<string, string>? table = null);

        public ITimedEvent TrackTime(string trackIdentifier, string key, string value);

        public void Report(Exception exception,
                                  string key,
                                  string value,
                                  [CallerMemberName] string callerMemberName = "",
                                  [CallerLineNumber] int lineNumber = 0,
                                  [CallerFilePath] string filePath = "");

        public void Report(Exception exception,
                                  IDictionary<string, string>? properties = null,
                                  [CallerMemberName] string callerMemberName = "",
                                  [CallerLineNumber] int lineNumber = 0,
                                  [CallerFilePath] string filePath = "");
    }

    public interface ITimedEvent : IDisposable
    {
        IDictionary<string, string> Data { get; }
    }
}