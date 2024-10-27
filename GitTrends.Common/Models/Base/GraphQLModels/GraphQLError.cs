using System.Text.Json;
using System.Text.Json.Serialization;

namespace GitTrends.Common;

public record GraphQLError(string Message, GraphQLLocation[] Locations)
{
	[JsonExtensionData]
	public IDictionary<string, JsonElement>? AdditionalEntries { get; set; }
}

public record GraphQLLocation(long Line, long Column);