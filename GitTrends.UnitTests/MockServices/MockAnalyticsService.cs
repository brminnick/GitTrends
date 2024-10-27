using System.Diagnostics;
using System.Runtime.CompilerServices;
using GitTrends.Common;
using GitTrends.Mobile.Common;

namespace GitTrends.UnitTests;

class MockAnalyticsService : IAnalyticsService
{
	public bool Configured { get; private set; }

	public void Start(string apiKey) => Configured = true;

	public void Track(string trackIdentifier) => Track(trackIdentifier, null);

	public void Track(string trackIdentifier, IDictionary<string, string>? table)
	{
		PrintHeader();
		Trace.WriteLine(trackIdentifier);

		if (table is not null)
		{
			foreach (var property in table)
				Debug.WriteLine($"{property.Key}: {property.Value}");
		}
	}

	public void Track(string trackIdentifier, string key, string value) => Track(trackIdentifier, new Dictionary<string, string> { { key, value } });

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

		var fileName = Path.GetFileName(filePath);

		Trace.WriteLine(exception.GetType());
		Trace.WriteLine($"Error: {exception.Message}");
		Trace.WriteLine($"Line Number: {lineNumber}");
		Trace.WriteLine($"Caller Name: {callerMemberName}");
		Trace.WriteLine($"File Name: {fileName}");

		if (properties != null)
		{
			foreach (var property in properties)
				Trace.WriteLine($"{property.Key}: {property.Value}");
		}

		Trace.WriteLine(exception);
	}

	static void PrintHeader([CallerMemberName] string title = "")
	{
		const string headerBreak = "**************************";

		Trace.WriteLine("");
		Trace.WriteLine("");

		Trace.WriteLine(headerBreak);
		Trace.WriteLine(title);
		Trace.WriteLine(headerBreak);
	}
}