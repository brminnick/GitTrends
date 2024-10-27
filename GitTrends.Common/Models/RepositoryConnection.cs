using System.Text.Json.Serialization;

namespace GitTrends.Common;

public record RepositoryConnection(
	[property: JsonPropertyName("nodes")] IReadOnlyList<RepositoryConnectionNode?> RepositoryList,
	[property: JsonPropertyName("pageInfo")] PageInfo PageInfo
);

public record RepositoryConnectionNode(bool IsArchived, string ViewerPermission, string Name, string Description, long ForkCount, Uri Url, RepositoryOwner Owner, bool IsFork, IssuesConnection Issues, Watchers Watchers, StarGazersConnection Stargazers)
{
	public DateTimeOffset DataDownloadedAt { get; } = DateTimeOffset.UtcNow;

	public RepositoryPermission Permission { get; } = Enum.TryParse<RepositoryPermission>(ViewerPermission, out var permission) ? permission : RepositoryPermission.UNKNOWN;
}

public record RepositoryOwner(string Login, string AvatarUrl);