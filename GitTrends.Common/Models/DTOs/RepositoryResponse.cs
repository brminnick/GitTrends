namespace GitTrends.Common;

public record RepositoryResponse(RepositoryResponseData Repository);

public record RepositoryResponseData(string Name,
										string Description,
										long ForkCount,
										Uri Url,
										RepositoryOwner Owner,
										bool IsFork,
										IssuesConnection Issues,
										Watchers Watchers,
										string ViewerPermission,
										bool IsArchived)
{
	public RepositoryPermission Permission => (RepositoryPermission)Enum.Parse(typeof(RepositoryPermission), ViewerPermission);
}

public record Watchers(long TotalCount);