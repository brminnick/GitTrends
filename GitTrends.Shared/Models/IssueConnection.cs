using System.Text.Json.Serialization;

namespace GitTrends.Shared;

public record IssuesConnection(
	[property: JsonPropertyName("totalCount")] long IssuesCount,
	[property: JsonPropertyName("nodes")] IReadOnlyList<Issue> IssueList);