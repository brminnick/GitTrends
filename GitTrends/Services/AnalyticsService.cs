using System.Diagnostics;
using System.Runtime.CompilerServices;
using GitTrends.Mobile.Common;
using GitTrends.Shared;

namespace GitTrends;

public class AnalyticsService(SentryClient client) : IAnalyticsService
{
	readonly ISentryClient _client = client;

	public bool Configured => _client.IsEnabled;

	public void Track(string trackIdentifier, IDictionary<string, string>? table = null)
	{
		var sentryEvent = new SentryEvent
		{
			TransactionName = trackIdentifier
		};

		if (table is not null)
		{
			foreach (var keyValuePair in table)
			{
				sentryEvent.SetExtra(keyValuePair.Key, keyValuePair.Value);
			}
		}

		_client.CaptureEvent(sentryEvent);
	}

	public void Track(string trackIdentifier, string key, string value)
	{
		Track(trackIdentifier, new Dictionary<string, string>
		{
			{
				key, value
			}
		});
	}

	public ITimedEvent TrackTime(string trackIdentifier, IDictionary<string, string>? table = null) =>
		new TimedEvent(this, trackIdentifier, table);

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

		_client.CaptureException(exception);
	}

	[Conditional("DEBUG")]
	static void PrintException(Exception exception, string callerMemberName, int lineNumber, string filePath, IDictionary<string, string>? properties = null)
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
}