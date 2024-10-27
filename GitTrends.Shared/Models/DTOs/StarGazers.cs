using System.Text.Json.Serialization;

namespace  GitTrends.Common;

public record StarGazers(
	[property: JsonPropertyName("totalCount")] long TotalCount,
	[property: JsonPropertyName("edges")] IReadOnlyList<StarGazerInfo> StarredAt);

public record StarGazerInfo(DateTimeOffset StarredAt, string Cursor);

public record StarGazer(DateTimeOffset StarredAt);

public record StarGazersConnection(long TotalCount);