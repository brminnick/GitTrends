using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace GitTrends.Shared;

public record IssuesConnection
{
	public IssuesConnection(long totalCount, IEnumerable<Issue>? nodes) =>
		(IssuesCount, IssueList) = (totalCount, (nodes ?? Array.Empty<Issue>()).ToList());

	[JsonProperty("nodes")]
	public IReadOnlyList<Issue> IssueList { get; }

	[JsonProperty("totalCount")]
	public long IssuesCount { get; }
}