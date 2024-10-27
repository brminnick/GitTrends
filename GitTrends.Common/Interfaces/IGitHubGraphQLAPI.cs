using Refit;

namespace GitTrends.Common;

[Headers("User-Agent: " + nameof(GitTrends))]
public interface IGitHubGraphQLApi
{
	[Post("")]
	Task<ApiResponse<GraphQLResponse<RepositoryResponse>>> RepositoryQuery([Body(true)] RepositoryQueryContent request, [Header("Authorization")] string authorization, CancellationToken token);

	[Post("")]
	Task<ApiResponse<GraphQLResponse<GitHubUserResponse>>> UserRepositoryConnectionQuery([Body(true)] UserRepositoryConnectionQueryContent request, [Header("Authorization")] string authorization, CancellationToken token);

	[Post("")]
	Task<ApiResponse<GraphQLResponse<GitHubOrganizationResponse>>> OrganizationRepositoryConnectionQuery([Body(true)] OrganizationRepositoryConnectionQueryContent request, [Header("Authorization")] string authorization, CancellationToken token);

	[Post("")]
	Task<ApiResponse<GraphQLResponse<GitHubViewerLoginResponse>>> ViewerLoginQuery([Body(true)] ViewerLoginQueryContent request, [Header("Authorization")] string authorization, CancellationToken token);

	[Post("")]
	Task<ApiResponse<GraphQLResponse<GitHubViewerOrganizationResponse>>> ViewerOrganizationsQuery([Body(true)] ViewerOrganizationsQueryContent request, [Header("Authorization")] string authorization, CancellationToken token);

	[Post("")]
	Task<ApiResponse<GraphQLResponse<StarGazerResponse>>> StarGazerQuery([Body(true)] StarGazerQueryContent request, [Header("Authorization")] string authorization, CancellationToken token);
}

public record StarGazerQueryContent : GraphQLRequest
{
	public StarGazerQueryContent(string repositoryName, string repositoryOwner, string? endCursorString, int numberOfStarGazersPerRequest = 100)
		: base("query { repository(name: \"" + repositoryName + "\", owner: \"" + repositoryOwner + "\") { stargazers(first:" + numberOfStarGazersPerRequest + endCursorString + ") { edges { starredAt, cursor } } }} ")
	{

	}
}

public record ViewerLoginQueryContent() : GraphQLRequest("query { viewer{ login, name, avatarUrl }}");

public record ViewerOrganizationsQueryContent : GraphQLRequest
{
	public ViewerOrganizationsQueryContent(string? endCursorString, int numberOfRepositoriesPerRequest = 100)
		: base("query { viewer { organizations(first:" + numberOfRepositoriesPerRequest + endCursorString + ") { nodes { login }, pageInfo { endCursor, hasNextPage, startCursor, hasPreviousPage } } } }")
	{

	}
}

public record RepositoryQueryContent : GraphQLRequest
{
	public RepositoryQueryContent(string repositoryOwner, string repositoryName)
		: base("query { repository(owner:\"" + repositoryOwner + "\" name:\"" + repositoryName + "\"){ isArchived, viewerPermission, name, description, forkCount, url, owner { avatarUrl, login }, isFork, issues(states:OPEN) { totalCount }, watchers{ totalCount }}}")
	{

	}
}

public record UserRepositoryConnectionQueryContent : GraphQLRequest
{
	public UserRepositoryConnectionQueryContent(string repositoryOwner, string endCursorString, int numberOfRepositoriesPerRequest = 100)
		: base("query{ user(login: \"" + repositoryOwner + "\") {repositories(first:" + numberOfRepositoriesPerRequest + endCursorString + ") { nodes { isArchived, viewerPermission, name, description, forkCount, url, owner { avatarUrl, login }, isFork, issues(states:OPEN) { totalCount }, watchers{ totalCount }, stargazers { totalCount } }, pageInfo { endCursor, hasNextPage, hasPreviousPage, startCursor } } } }")
	{

	}
}

public record OrganizationRepositoryConnectionQueryContent : GraphQLRequest
{
	public OrganizationRepositoryConnectionQueryContent(string organization, string endCursorString, int numberOfRepositoriesPerRequest = 100)
		: base("query{ organization(login: \"" + organization + "\") {repositories(first:" + numberOfRepositoriesPerRequest + endCursorString + ") { nodes { isArchived, viewerPermission, name, description, forkCount, url, owner { avatarUrl, login }, isFork, issues(states:OPEN) { totalCount }, watchers{ totalCount }, stargazers { totalCount } }, pageInfo { endCursor, hasNextPage, hasPreviousPage, startCursor } } } }")
	{

	}
}