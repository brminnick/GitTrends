using System.Threading.Tasks;
using Refit;

namespace GitTrends.Shared
{
    [Headers("User-Agent: " + nameof(GitTrends))]
    interface IGitHubGraphQLApi
    {
        [Post("")]
        Task<GraphQLResponse<GitHubUserResponse>> UserQuery([Body] UserQueryContent request, [Header("Authorization")] string authorization);

        [Post("")]
        Task<GraphQLResponse<RepositoryResponse>> RepositoryQuery([Body] RepositoryQueryContent request, [Header("Authorization")] string authorization);

        [Post("")]
        Task<GraphQLResponse<RepositoryConnectionResponse>> RepositoryConnectionQuery([Body] RepositoryConnectionQueryContent request, [Header("Authorization")] string authorization);
    }

    class UserQueryContent : GraphQLRequest
    {
        public UserQueryContent(string username) : base("query { user(login:" + username + "){ name, company, createdAt, followers{ totalCount }}}")
        {

        }
    }

    class RepositoryQueryContent : GraphQLRequest
    {
        public RepositoryQueryContent(string repositoryOwner, string repositoryName, int numberOfIssuesPerRequest = 100)
            : base("query { repository(owner:\"" + repositoryOwner + "\" name:\"" + repositoryName + "\"){ name, description, forkCount, url, owner { avatarUrl, login }, stargazers { totalCount }, issues(first:" + numberOfIssuesPerRequest + "){ nodes { title, body, createdAt, closedAt, state }}}}")
        {

        }
    }

    class RepositoryConnectionQueryContent : GraphQLRequest
    {
        public RepositoryConnectionQueryContent(string repositoryOwner, string endCursorString, int numberOfRepositoriesPerRequest = 100)
            : base("query{ user(login:" + repositoryOwner + ") {followers { totalCount }, repositories(first:" + numberOfRepositoriesPerRequest + endCursorString + ") { nodes { name, description, forkCount, url, owner { avatarUrl, login }, stargazers { totalCount }, issues(states:OPEN) { totalCount } }, pageInfo { endCursor, hasNextPage, hasPreviousPage, startCursor } } } }")
        {

        }
    }
}
