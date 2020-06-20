using System.Threading.Tasks;
using Refit;

namespace GitTrends.Shared
{
    [Headers("User-Agent: " + nameof(GitTrends))]
    public interface IGitHubGraphQLApi
    {
        [Post("")]
        Task<GraphQLResponse<RepositoryResponse>> RepositoryQuery([Body] RepositoryQueryContent request, [Header("Authorization")] string authorization);

        [Post("")]
        Task<GraphQLResponse<RepositoryConnectionResponse>> RepositoryConnectionQuery([Body] RepositoryConnectionQueryContent request, [Header("Authorization")] string authorization);

        [Post("")]
        Task<GraphQLResponse<GitHubViewerResponse>> ViewerLoginQuery([Body] ViewerLoginQueryContent request, [Header("Authorization")] string authorization);
    }

    public class ViewerLoginQueryContent : GraphQLRequest
    {
        public ViewerLoginQueryContent() : base("query { viewer{ login, name, avatarUrl }}")
        {

        }
    }

    public class RepositoryQueryContent : GraphQLRequest
    {
        public RepositoryQueryContent(string repositoryOwner, string repositoryName, int numberOfIssuesPerRequest = 100)
            : base("query { repository(owner:\"" + repositoryOwner + "\" name:\"" + repositoryName + "\"){ name, description, forkCount, url, owner { avatarUrl, login }, stargazers { totalCount }, isFork, issues(first:" + numberOfIssuesPerRequest + "){ nodes { title, body, createdAt, closedAt, state }}}}")
        {

        }
    }

    public class RepositoryConnectionQueryContent : GraphQLRequest
    {
        public RepositoryConnectionQueryContent(string repositoryOwner, string endCursorString, int numberOfRepositoriesPerRequest = 100)
            : base("query{ user(login: \"" + repositoryOwner + "\") {followers { totalCount }, repositories(first:" + numberOfRepositoriesPerRequest + endCursorString + ") { nodes { name, description, forkCount, url, owner { avatarUrl, login }, stargazers { totalCount }, isFork, issues(states:OPEN) { totalCount } }, pageInfo { endCursor, hasNextPage, hasPreviousPage, startCursor } } } }")
        {

        }
    }
}
