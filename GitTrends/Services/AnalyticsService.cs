using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using GitTrends.Shared;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

namespace GitTrends
{
    public class AnalyticsService : IAnalyticsService
    {
#if AppStore
        const string _iOSKey = "7baad29b-66fa-4c52-8533-217c11595714";
        const string _androidKey = "d55a09c1-9908-4bc6-99b8-caa4e9083530";
#else
        const string _iOSKey = "0e194e2a-3aad-41c5-a6bc-61900e185075";
        const string _androidKey = "272973ed-d3cc-4ee0-b4f2-5b5d01ad487d";
#endif

        public AnalyticsService() => AppCenter.Start(ApiKey, typeof(Analytics), typeof(Crashes));

        string ApiKey => Xamarin.Forms.Device.RuntimePlatform switch
        {
            Xamarin.Forms.Device.iOS => _iOSKey,
            Xamarin.Forms.Device.Android => _androidKey,
            _ => throw new NotSupportedException()
        };

        public void Track(string trackIdentifier, IDictionary<string, string>? table = null) =>
            Analytics.TrackEvent(trackIdentifier, table);

        public void Track(string trackIdentifier, string key, string value) =>
            Analytics.TrackEvent(trackIdentifier, new Dictionary<string, string> { { key, value } });

        public ITimedEvent TrackTime(string trackIdentifier, IDictionary<string, string>? table = null) =>
            new TimedEvent(trackIdentifier, table);

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
            PrintException(exception, callerMemberName, lineNumber, filePath, properties);

            Crashes.TrackError(exception, properties);
        }

        [Conditional("DEBUG")]
        void PrintException(Exception exception, string callerMemberName, int lineNumber, string filePath, IDictionary<string, string>? properties = null)
        {
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

        public class TimedEvent : ITimedEvent
        {
            readonly Stopwatch _stopwatch;
            readonly string _trackIdentifier;

            public TimedEvent(string trackIdentifier, IDictionary<string, string>? dictionary)
            {
                Data = dictionary ?? new Dictionary<string, string>();
                _trackIdentifier = trackIdentifier;
                _stopwatch = new Stopwatch();
                _stopwatch.Start();
            }

            public IDictionary<string, string> Data { get; }

            public void Dispose()
            {
                _stopwatch.Stop();
                Data.Add("Timed Event", $"{_stopwatch.Elapsed:ss\\.fff}s");
                Analytics.TrackEvent($"{_trackIdentifier} [Timed Event]", Data);
            }
        }
    }
}
