using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GitTrends.Shared;
using Refit;

namespace GitTrends
{
    abstract class GitHubGraphQLApiService : BaseMobileApiService
    {
        #region Constant Fields
        readonly static Lazy<IGitHubGraphQLApi> _githubApiClientHolder = new Lazy<IGitHubGraphQLApi>(() => RestService.For<IGitHubGraphQLApi>(CreateHttpClient(GitHubConstants.GitHubGraphQLApi)));
        #endregion

        #region Properties
        static IGitHubGraphQLApi GitHubApiClient => _githubApiClientHolder.Value;
        #endregion

        #region Methods
        public static async Task<(string login, string name, Uri avatarUri)> GetCurrentUserInfo()
        {
            var token = await GitHubAuthenticationService.GetGitHubToken().ConfigureAwait(false);
            var data = await ExecuteGraphQLRequest(() => GitHubApiClient.ViewerLoginQuery(new ViewerLoginQueryContent(), GetGitHubBearerTokenHeader(token))).ConfigureAwait(false);

            return (data.Viewer.Alias, data.Viewer.Name, data.Viewer.AvatarUri);
        }

        public static async Task<User> GetUser(string username)
        {
            var token = await GitHubAuthenticationService.GetGitHubToken().ConfigureAwait(false);
            var data = await ExecuteGraphQLRequest(() => GitHubApiClient.UserQuery(new UserQueryContent(username), GetGitHubBearerTokenHeader(token))).ConfigureAwait(false);

            return data.User;
        }

        public static async Task<Repository> GetRepository(string repositoryOwner, string repositoryName, int numberOfIssuesPerRequest = 100)
        {
            var token = await GitHubAuthenticationService.GetGitHubToken().ConfigureAwait(false);
            var data = await ExecuteGraphQLRequest(() => GitHubApiClient.RepositoryQuery(new RepositoryQueryContent(repositoryOwner, repositoryName, numberOfIssuesPerRequest), GetGitHubBearerTokenHeader(token))).ConfigureAwait(false);

            return data.Repository;
        }

        public static async Task<IEnumerable<Repository>> GetRepositories(string repositoryOwner, int numberOfRepositoriesPerRequest = 100)
        {
            RepositoryConnection repositoryConnection = null;

            List<Repository> gitHubRepositoryList = new List<Repository>();

            do
            {
                repositoryConnection = await GetRepositoryConnection(repositoryOwner, repositoryConnection?.PageInfo?.EndCursor, numberOfRepositoriesPerRequest).ConfigureAwait(false);
                gitHubRepositoryList.AddRange(repositoryConnection?.RepositoryList);
            }
            while (repositoryConnection?.PageInfo?.HasNextPage is true);

            return gitHubRepositoryList;
        }

        static async Task<RepositoryConnection> GetRepositoryConnection(string repositoryOwner, string? endCursor, int numberOfRepositoriesPerRequest = 100)
        {
            var token = await GitHubAuthenticationService.GetGitHubToken().ConfigureAwait(false);
            var data = await ExecuteGraphQLRequest(() => GitHubApiClient.RepositoryConnectionQuery(new RepositoryConnectionQueryContent(repositoryOwner, GetEndCursorString(endCursor), numberOfRepositoriesPerRequest), GetGitHubBearerTokenHeader(token))).ConfigureAwait(false);

            return data.GitHubUser.RepositoryConnection;
        }

        static async Task<T> ExecuteGraphQLRequest<T>(Func<Task<GraphQLResponse<T>>> action, int numRetries = 2)
        {
            var response = await ExecuteMobilePollyFunction(action, numRetries).ConfigureAwait(false);

            if (response.Errors != null)
                throw new AggregateException(response.Errors.Select(x => new Exception(x.Message)));

            return response.Data;
        }

        static string GetEndCursorString(string? endCursor) => string.IsNullOrWhiteSpace(endCursor) ? string.Empty : "after: \"" + endCursor + "\"";
        #endregion
    }
}
