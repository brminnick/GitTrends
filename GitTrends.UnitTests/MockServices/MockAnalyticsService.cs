using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using GitTrends.Mobile.Common;
using GitTrends.Shared;

namespace GitTrends.UnitTests
{
    class MockAnalyticsService : IAnalyticsService
    {
        public void Track(string trackIdentifier, IDictionary<string, string>? table = null)
        {
            PrintHeader();
            Debug.WriteLine(trackIdentifier);

            if (table != null)
            {
                foreach (var property in table)
                    Debug.WriteLine($"{property.Key}: {property.Value}");
            }
        }

        public void Track(string trackIdentifier, string key, string value) => Track(trackIdentifier, new Dictionary<string, string> { { key, value } });

        public ITimedEvent TrackTime(string trackIdentifier, IDictionary<string, string>? table = null) => new TimedEvent(this, trackIdentifier, table);

        public ITimedEvent TrackTime(string trackIdentifier, string key, string value) =>
            TrackTime(trackIdentifier, new Dictionary<string, string> { { key, value } });

        public void Report(Exception exception,
                                  string key,
                                  string value,
                                  [CallerMemberName] string callerMemberName = "",
                                  [CallerLineNumber] int lineNumber = 0,
                                  [CallerFilePath] string filePath = "")
        {
            Report(exception, new Dictionary<string, string> { { key, value } }, callerMemberName, lineNumber, filePath);
        }

        public void Report(Exception exception,
                                  IDictionary<string, string>? properties = null,
                                  [CallerMemberName] string callerMemberName = "",
                                  [CallerLineNumber] int lineNumber = 0,
                                  [CallerFilePath] string filePath = "")
        {
            PrintHeader();

            var fileName = System.IO.Path.GetFileName(filePath);

            Debug.WriteLine(exception.GetType());
            Debug.WriteLine($"Error: {exception.Message}");
            Debug.WriteLine($"Line Number: {lineNumber}");
            Debug.WriteLine($"Caller Name: {callerMemberName}");
            Debug.WriteLine($"File Name: {fileName}");

            if (properties != null)
            {
                foreach (var property in properties)
                    Debug.WriteLine($"{property.Key}: {property.Value}");
            }

            Debug.WriteLine(exception);
        }

        static void PrintHeader([CallerMemberName] string title = "")
        {
            const string headerBreak = "**************************";

            Debug.WriteLine("");
            Debug.WriteLine("");

            Debug.WriteLine(headerBreak);
            Debug.WriteLine(title);
            Debug.WriteLine(headerBreak);
        }
    }
}
