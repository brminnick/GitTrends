using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using GitTrends.Shared;
using Refit;
using Xamarin.Essentials.Interfaces;

namespace GitTrends
{
    public class GitHubGraphQLApiService : BaseMobileApiService
    {
        readonly static Lazy<IGitHubGraphQLApi> _githubApiClientHolder = new Lazy<IGitHubGraphQLApi>(() => RestService.For<IGitHubGraphQLApi>(CreateHttpClient(GitHubConstants.GitHubGraphQLApi)));

        readonly GitHubUserService _gitHubUserService;

        public GitHubGraphQLApiService(IAnalyticsService analyticsService, IMainThread mainThread, GitHubUserService gitHubUserService) : base(analyticsService, mainThread)
        {
            _gitHubUserService = gitHubUserService;
        }

        static IGitHubGraphQLApi GitHubApiClient => _githubApiClientHolder.Value;

        public async Task<(string login, string name, Uri avatarUri)> GetCurrentUserInfo(CancellationToken cancellationToken)
        {
            var token = await _gitHubUserService.GetGitHubToken().ConfigureAwait(false);
            var data = await ExecuteGraphQLRequest(() => GitHubApiClient.ViewerLoginQuery(new ViewerLoginQueryContent(), GetGitHubBearerTokenHeader(token)), cancellationToken).ConfigureAwait(false);

            return (data.Viewer.Alias, data.Viewer.Name, data.Viewer.AvatarUri);
        }

        public async Task<Repository> GetRepository(string repositoryOwner, string repositoryName, CancellationToken cancellationToken, int numberOfIssuesPerRequest = 100)
        {
            var token = await _gitHubUserService.GetGitHubToken().ConfigureAwait(false);
            var data = await ExecuteGraphQLRequest(() => GitHubApiClient.RepositoryQuery(new RepositoryQueryContent(repositoryOwner, repositoryName, numberOfIssuesPerRequest), GetGitHubBearerTokenHeader(token)), cancellationToken).ConfigureAwait(false);

            return data.Repository;
        }

        public async IAsyncEnumerable<IEnumerable<Repository>> GetRepositories(string repositoryOwner, [EnumeratorCancellation] CancellationToken cancellationToken, int numberOfRepositoriesPerRequest = 100)
        {
            if (_gitHubUserService.IsDemoUser)
            {
                //Yield off of main thread to generate the demoDataList
                await Task.Yield();

                var demoDataList = new List<Repository>();

                for (int i = 0; i < DemoDataConstants.RepoCount; i++)
                {
                    var demoRepo = new Repository($"Repository " + DemoDataConstants.GetRandomText(), DemoDataConstants.GetRandomText(), DemoDataConstants.GetRandomNumber(),
                                                new RepositoryOwner(DemoUserConstants.Alias, _gitHubUserService.AvatarUrl),
                                                new IssuesConnection(DemoDataConstants.GetRandomNumber(), Enumerable.Empty<Issue>()),
                                                _gitHubUserService.AvatarUrl, new StarGazers(DemoDataConstants.GetRandomNumber()), false);
                    demoDataList.Add(demoRepo);
                }

                yield return demoDataList;

                //Allow UI to update
                await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
            }
            else
            {
                RepositoryConnection? repositoryConnection = null;

                do
                {
                    repositoryConnection = await GetRepositoryConnection(repositoryOwner, repositoryConnection?.PageInfo?.EndCursor, cancellationToken, numberOfRepositoriesPerRequest).ConfigureAwait(false);
                    yield return repositoryConnection?.RepositoryList ?? Enumerable.Empty<Repository>();
                }
                while (repositoryConnection?.PageInfo?.HasNextPage is true);
            }
        }

        async Task<RepositoryConnection> GetRepositoryConnection(string repositoryOwner, string? endCursor, CancellationToken cancellationToken, int numberOfRepositoriesPerRequest = 100)
        {
            var token = await _gitHubUserService.GetGitHubToken().ConfigureAwait(false);
            var data = await ExecuteGraphQLRequest(() => GitHubApiClient.RepositoryConnectionQuery(new RepositoryConnectionQueryContent(repositoryOwner, getEndCursorString(endCursor), numberOfRepositoriesPerRequest), GetGitHubBearerTokenHeader(token)), cancellationToken).ConfigureAwait(false);

            return data.GitHubUser.RepositoryConnection;

            static string getEndCursorString(string? endCursor) => string.IsNullOrWhiteSpace(endCursor) ? string.Empty : "after: \"" + endCursor + "\"";
        }

        async Task<T> ExecuteGraphQLRequest<T>(Func<Task<GraphQLResponse<T>>> action, CancellationToken cancellationToken, int numRetries = 2, [CallerMemberName] string callerName = "")
        {
            var response = await AttemptAndRetry_Mobile(action, cancellationToken, numRetries, callerName: callerName).ConfigureAwait(false);

            if (response.Errors != null && response.Errors.Count() > 1)
                throw new AggregateException(response.Errors.Select(x => new Exception(x.Message)));
            else if (response.Errors != null && response.Errors.Any())
                throw new Exception(response.Errors.First().Message.ToString());

            return response.Data;
        }
    }
}
