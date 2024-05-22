using System.Text.Json.Serialization;

namespace GitTrends.Shared;

public record IssuesConnection
{
	public IssuesConnection(long totalCount, IEnumerable<Issue>? nodes) =>
		(IssuesCount, IssueList) = (totalCount, (nodes ?? []).ToList());

	[JsonPropertyName("nodes")]
	public IReadOnlyList<Issue> IssueList { get; }

	[JsonPropertyName("totalCount")]
	public long IssuesCount { get; }
}