namespace  GitTrends.Common;

public record GetRepositoryResponse : IRepository
{
	public GetRepositoryResponse(string name,
									bool fork,
									string html_url,
									long forks_count,
									long subscribers_count,
									string description,
									long open_issues_count,
									long stargazers_count,
									bool archived,
									Permissions permissions,
									Owner_GetRepositoryResponse owner)
	{
		Name = name;
		IsFork = fork;
		Url = html_url;
		ForkCount = forks_count;
		OwnerLogin = owner.Login;
		StarCount = stargazers_count;
		Description = description;
		WatchersCount = subscribers_count;
		OwnerAvatarUrl = owner.AvatarUrl;
		IssuesCount = open_issues_count;
		IsArchived = archived;

		DataDownloadedAt = DateTimeOffset.UtcNow;

		Permission = permissions switch
		{
			{ Admin: true } => RepositoryPermission.ADMIN,
			{ Maintain: true } => RepositoryPermission.MAINTAIN,
			{ Push: true } => RepositoryPermission.WRITE,
			{ Triage: true } => RepositoryPermission.TRIAGE,
			{ Pull: true } => RepositoryPermission.READ,
			_ => RepositoryPermission.UNKNOWN
		};
	}

	public DateTimeOffset DataDownloadedAt { get; }

	public string OwnerLogin { get; }

	public string OwnerAvatarUrl { get; }

	public long StarCount { get; }

	public long IssuesCount { get; }

	public string Name { get; }

	public bool IsFork { get; }

	public string Description { get; }

	public long ForkCount { get; }

	public long WatchersCount { get; }

	public string Url { get; }

	public bool IsArchived { get; }

	public long? TotalViews { get; }

	public long? TotalUniqueViews { get; }

	public long? TotalClones { get; }

	public long? TotalUniqueClones { get; }

	public bool? IsFavorite { get; }

	public RepositoryPermission Permission { get; }
}

public record Permissions(bool Admin, bool Maintain, bool Push, bool Triage, bool Pull);

public record Owner_GetRepositoryResponse : RepositoryOwner
{
	public Owner_GetRepositoryResponse(string login, string avatar_url) : base(login, avatar_url)
	{

	}
}