using System.Text.Json.Serialization;

namespace  GitTrends.Common;

public record IssuesConnection(
	[property: JsonPropertyName("totalCount")] long IssuesCount,
	[property: JsonPropertyName("nodes")] IReadOnlyList<Issue> IssueList);