using System.Text.Json.Serialization;

namespace GitTrends.Shared;

public record RepositoryConnection
{
	public RepositoryConnection(IEnumerable<RepositoryConnectionNode>? nodes, PageInfo pageInfo)
	{
		RepositoryList = (nodes ?? []).ToList();
		PageInfo = pageInfo;
	}

	[JsonPropertyName("nodes")]
	public IReadOnlyList<RepositoryConnectionNode?> RepositoryList { get; }

	[JsonPropertyName("pageInfo")]
	public PageInfo PageInfo { get; }
}

public record RepositoryConnectionNode(bool IsArchived, string ViewerPermission, string Name, string Description, long ForkCount, Uri Url, RepositoryOwner Owner, bool IsFork, IssuesConnection Issues, Watchers Watchers, StarGazersConnection Stargazers)
{
	public DateTimeOffset DataDownloadedAt { get; } = DateTimeOffset.UtcNow;

	public RepositoryPermission Permission
	{
		get
		{
			if (Enum.TryParse<RepositoryPermission>(ViewerPermission, out var permission))
				return permission;

			return RepositoryPermission.UNKNOWN;
		}
	}
}

public record RepositoryOwner(string Login, string AvatarUrl);