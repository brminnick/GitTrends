using System.Threading.Tasks;
using Refit;

namespace GitTrends.Shared
{
    [Headers("User-Agent: " + nameof(GitTrends))]
    public interface IGitHubGraphQLApi
    {
        [Post("")]
        Task<ApiResponse<GraphQLResponse<RepositoryResponse>>> RepositoryQuery([Body] RepositoryQueryContent request, [Header("Authorization")] string authorization);

        [Post("")]
        Task<ApiResponse<GraphQLResponse<RepositoryConnectionResponse>>> RepositoryConnectionQuery([Body] RepositoryConnectionQueryContent request, [Header("Authorization")] string authorization);

        [Post("")]
        Task<ApiResponse<GraphQLResponse<GitHubViewerResponse>>> ViewerLoginQuery([Body] ViewerLoginQueryContent request, [Header("Authorization")] string authorization);

        [Post("")]
        Task<ApiResponse<GraphQLResponse<StarGazerResponse>>> StarGazerQuery([Body] StarGazerQueryContent request, [Header("Authorization")] string authorization);
    }

    public class StarGazerQueryContent : GraphQLRequest
    {
        public StarGazerQueryContent(string repositoryName, string repositoryOwner, string? endCursorString, int numberOfStarGazersPerRequest = 100)
            : base("query { repository(name: \"" + repositoryName + "\", owner: \"" + repositoryOwner + "\") { stargazers(first:" + numberOfStarGazersPerRequest + endCursorString + ") { edges { starredAt, cursor } } }} ")
        {

        }
    }

    public class ViewerLoginQueryContent : GraphQLRequest
    {
        public ViewerLoginQueryContent() : base("query { viewer{ login, name, avatarUrl }}")
        {

        }
    }

    public class RepositoryQueryContent : GraphQLRequest
    {
        public RepositoryQueryContent(string repositoryOwner, string repositoryName)
            : base("query { repository(owner:\"" + repositoryOwner + "\" name:\"" + repositoryName + "\"){ name, description, forkCount, url, owner { avatarUrl, login }, isFork, issues(states:OPEN) { totalCount }}}")
        {

        }
    }

    public class RepositoryConnectionQueryContent : GraphQLRequest
    {
        public RepositoryConnectionQueryContent(string repositoryOwner, string endCursorString, int numberOfRepositoriesPerRequest = 100)
            : base("query{ user(login: \"" + repositoryOwner + "\") {repositories(first:" + numberOfRepositoriesPerRequest + endCursorString + ") { nodes { name, description, forkCount, url, owner { avatarUrl, login }, isFork, issues(states:OPEN) { totalCount } }, pageInfo { endCursor, hasNextPage, hasPreviousPage, startCursor } } } }")
        {

        }
    }
}
