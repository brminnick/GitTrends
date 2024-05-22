using System.Text.Json;
using System.Text.Json.Serialization;

namespace GitTrends.Shared;

public record GraphQLError(string Message, GraphQLLocation[] Locations)
{
	[JsonExtensionData]
	public IDictionary<string, JsonDocument>? AdditonalEntries { get; set; }
}

public record GraphQLLocation(long Line, long Column);