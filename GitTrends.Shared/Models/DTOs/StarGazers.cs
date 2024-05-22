using System.Text.Json.Serialization;

namespace GitTrends.Shared;

public record StarGazers
{
	public StarGazers(long totalCount, IEnumerable<StarGazerInfo> edges) =>
		(TotalCount, StarredAt) = (totalCount, edges.ToList());

	[JsonPropertyName("totalCount")]
	public long TotalCount { get; }

	[JsonPropertyName("edges")]
	public IReadOnlyList<StarGazerInfo> StarredAt { get; }
}

public record StarGazerInfo(DateTimeOffset StarredAt, string Cursor);

public record StarGazer
{
	public StarGazer(DateTimeOffset starred_at) => StarredAt = starred_at;

	public DateTimeOffset StarredAt { get; }
}

public record StarGazersConnection(long TotalCount);