using System.Diagnostics;
using System.Runtime.CompilerServices;
using GitTrends.Common;

namespace GitTrends;

public class AnalyticsService(ISentryClient client) : IAnalyticsService
{
	readonly ISentryClient _client = client;

	public bool Configured => _client.IsEnabled;

	public void Track(string trackIdentifier) => Trace.WriteLine(trackIdentifier);

	public void Track(string trackIdentifier, string key, string value) => Track(trackIdentifier);

	public void Track(string trackIdentifier, IDictionary<string, string>? table) => Track(trackIdentifier);

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
		var fileName = Path.GetFileName(filePath);

		Trace.WriteLine(exception.GetType());
		Trace.WriteLine($"Error: {exception.Message}");
		Trace.WriteLine($"Line Number: {lineNumber}");
		Trace.WriteLine($"Caller Name: {callerMemberName}");
		Trace.WriteLine($"File Name: {fileName}");

		if (properties is not null)
		{
			foreach (var property in properties)
				Trace.WriteLine($"{property.Key}: {property.Value}");
		}

		Trace.WriteLine(exception);
	}
}