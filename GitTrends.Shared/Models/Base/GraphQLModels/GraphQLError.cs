using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GitTrends.Shared;

public record GraphQLError(string Message, GraphQLLocation[] Locations)
{
	[JsonExtensionData]
	public IDictionary<string, JToken>? AdditonalEntries { get; set; }
}

public record GraphQLLocation(long Line, long Column);