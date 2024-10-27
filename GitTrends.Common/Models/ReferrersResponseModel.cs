using System.Text.Json.Serialization;

namespace GitTrends.Common;

public record ReferrersResponseModel(
	[property: JsonPropertyName("referrer")] string Referrer,
	[property: JsonPropertyName("count")] int Count,
	[property: JsonPropertyName("uniques")] int Uniques
);