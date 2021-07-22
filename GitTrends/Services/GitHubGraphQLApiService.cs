using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using GitHubApiStatus;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using GitTrends.Shared;
using Newtonsoft.Json;
using Refit;
using Xamarin.Essentials.Interfaces;

namespace GitTrends
{
    public class GitHubGraphQLApiService : BaseMobileApiService
    {
        readonly IGitHubGraphQLApi _githubApiClient;
        readonly GitHubUserService _gitHubUserService;
        readonly GitHubApiStatusService _gitHubApiStatusService;

        public GitHubGraphQLApiService(IMainThread mainThread,
                                        IAnalyticsService analyticsService,
                                        IGitHubGraphQLApi gitHubGraphQLApi,
                                        GitHubUserService gitHubUserService,
                                        GitHubApiStatusService gitHubApiStatusService) : base(analyticsService, mainThread)
        {
            _githubApiClient = gitHubGraphQLApi;
            _gitHubUserService = gitHubUserService;
            _gitHubApiStatusService = gitHubApiStatusService;
        }

        public async Task<(string login, string name, Uri avatarUri)> GetCurrentUserInfo(CancellationToken cancellationToken)
        {
            var token = await _gitHubUserService.GetGitHubToken().ConfigureAwait(false);
            var data = await ExecuteGraphQLRequest(() => _githubApiClient.ViewerLoginQuery(new ViewerLoginQueryContent(), GetGitHubBearerTokenHeader(token)), cancellationToken).ConfigureAwait(false);

            return (data.Viewer.Alias, data.Viewer.Name, data.Viewer.AvatarUri);
        }

        public async Task<Repository> GetRepository(string repositoryOwner, string repositoryName, CancellationToken cancellationToken)
        {
            var token = await _gitHubUserService.GetGitHubToken().ConfigureAwait(false);

            var repositoryQueryTask = ExecuteGraphQLRequest(() => _githubApiClient.RepositoryQuery(new RepositoryQueryContent(repositoryOwner, repositoryName), GetGitHubBearerTokenHeader(token)), cancellationToken);
            var starGazersQueryTask = GetStarGazers(repositoryName, repositoryOwner, cancellationToken);

            await Task.WhenAll(repositoryQueryTask, starGazersQueryTask).ConfigureAwait(false);

            var starGazersResult = await starGazersQueryTask.ConfigureAwait(false);
            var repositoryResult = await repositoryQueryTask.ConfigureAwait(false);

            return new Repository(repositoryResult.Repository.Name,
                                    repositoryResult.Repository.Description,
                                    repositoryResult.Repository.ForkCount,
                                    repositoryResult.Repository.Owner.Login,
                                    repositoryResult.Repository.Owner.AvatarUrl,
                                    repositoryResult.Repository.Issues.IssuesCount,
                                    repositoryResult.Repository.Watchers.TotalCount,
                                    repositoryResult.Repository.Url.ToString(),
                                    repositoryResult.Repository.IsFork,
                                    DateTimeOffset.UtcNow,
                                    repositoryResult.Repository.Permission,
                                    starredAt: starGazersResult.StarredAt.Select(x => x.StarredAt));
        }

        public async Task<StarGazers> GetStarGazers(string repositoryName, string repositoryOwner, CancellationToken cancellationToken)
        {
            if (_gitHubUserService.IsDemoUser)
            {
                var starCount = DemoDataConstants.GetRandomNumber();
                var starredAtDates = DemoDataConstants.GenerateStarredAtDates(starCount);

                var starGazerInfoList = starredAtDates.Select(x => new StarGazerInfo(x, string.Empty));

                return new StarGazers(starCount, starGazerInfoList);
            }
            else
            {
                var starGazerInfoList = new List<StarGazerInfo>();

                await foreach (var starGazerResponse in GetStarGazers(repositoryName, repositoryOwner, cancellationToken, 100).ConfigureAwait(false))
                {
                    starGazerInfoList.AddRange(starGazerResponse.StarredAt);
                }

                return new StarGazers(starGazerInfoList.Count, starGazerInfoList);
            }
        }

        public async IAsyncEnumerable<Repository> GetRepositories(string repositoryOwner, [EnumeratorCancellation] CancellationToken cancellationToken, int numberOfRepositoriesPerRequest = 100)
        {
            if (_gitHubUserService.IsDemoUser)
            {
                //Yield off of main thread to generate the demoDataList
                await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);

                for (int i = 0; i < DemoDataConstants.RepoCount; i++)
                {
                    var demoRepo = new Repository($"Repository " + DemoDataConstants.GetRandomText(), DemoDataConstants.GetRandomText(), DemoDataConstants.GetRandomNumber(),
                                                DemoUserConstants.Alias, _gitHubUserService.AvatarUrl, DemoDataConstants.GetRandomNumber(), DemoDataConstants.GetRandomNumber(),
                                                _gitHubUserService.AvatarUrl, false, DateTimeOffset.UtcNow, RepositoryPermission.ADMIN);
                    yield return demoRepo;
                }

                //Allow UI to update
                await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
            }
            else
            {
                await foreach (var repository in GetOwnedRepositories(repositoryOwner, cancellationToken, numberOfRepositoriesPerRequest).ConfigureAwait(false))
                {
                    yield return repository;
                }

                if (_gitHubUserService.ShouldIncludeOrganizations)
                {
                    await foreach (var repository in GetOrganizationRepositories(cancellationToken, numberOfRepositoriesPerRequest).ConfigureAwait(false))
                    {
                        yield return repository;
                    }
                }
            }
        }

        public async IAsyncEnumerable<Repository> GetOrganizationRepositories([EnumeratorCancellation] CancellationToken cancellationToken, int numberOfRepositoriesPerRequest = 100)
        {
            var organizationNameList = new List<string>();

            var token = await _gitHubUserService.GetGitHubToken().ConfigureAwait(false);

            await foreach (var organization in GetOrganizationNames(token, cancellationToken).ConfigureAwait(false))
            {
                organizationNameList.Add(organization);
            }

            var getOrganizationTaskList = new List<Task<IReadOnlyList<Repository>>>();

            foreach (var organization in organizationNameList)
                getOrganizationTaskList.Add(getOrganizationRepositories(organization));

            while (getOrganizationTaskList.Any())
            {
                var finishedTask = await Task.WhenAny(getOrganizationTaskList).ConfigureAwait(false);
                getOrganizationTaskList.Remove(finishedTask);

                IReadOnlyList<Repository> repositories = Array.Empty<Repository>();
                try
                {
                    repositories = await finishedTask.ConfigureAwait(false);
                }
                catch(ApiException e) when (_gitHubApiStatusService.IsAbuseRateLimit(e, out var retryDelta))
                {

#if AppStore
#error Queue Retry
#endif
                }

                foreach (var repository in repositories)
                    yield return repository;
            }

            async Task<IReadOnlyList<Repository>> getOrganizationRepositories(string repositoryName)
            {
                var repositoryList = new List<Repository>();

                RepositoryConnection? repositoryConnection = null;

                do
                {
                    repositoryConnection = await GetOrganizationRepositoryConnection(repositoryName, token, repositoryConnection?.PageInfo?.EndCursor, cancellationToken, numberOfRepositoriesPerRequest).ConfigureAwait(false);

                    // Views + Clones statistics are only available for repositories with write access
                    foreach (var repository in repositoryConnection.RepositoryList.Where(x => x?.Permission is RepositoryPermission.ADMIN or RepositoryPermission.MAINTAIN or RepositoryPermission.WRITE))
                    {
                        if (repository is not null)
                            repositoryList.Add(new Repository(repository.Name, repository.Description, repository.ForkCount, repository.Owner.Login, repository.Owner.AvatarUrl,
                                                        repository.Issues.IssuesCount, repository.Watchers.TotalCount, repository.Url.ToString(), repository.IsFork, repository.DataDownloadedAt, repository.Permission));
                    }
                }
                while (repositoryConnection?.PageInfo?.HasNextPage is true);

                return repositoryList;
            }
        }

        static string GetEndCursorString(string? endCursor) => string.IsNullOrWhiteSpace(endCursor) ? string.Empty : "after: \"" + endCursor + "\"";

        async IAsyncEnumerable<StarGazers> GetStarGazers(string repositoryName, string repositoryOwner, [EnumeratorCancellation] CancellationToken cancellationToken, int numberOfStarGazersPerRequest = 100)
        {
            StarGazerResponse? starGazerResponse = null;

            var token = await _gitHubUserService.GetGitHubToken().ConfigureAwait(false);

            do
            {
                var endCursor = GetEndCursorString(starGazerResponse?.Repository.StarGazers.StarredAt.LastOrDefault()?.Cursor);
                starGazerResponse = await ExecuteGraphQLRequest(() => _githubApiClient.StarGazerQuery(new StarGazerQueryContent(repositoryName, repositoryOwner, endCursor, numberOfStarGazersPerRequest), GetGitHubBearerTokenHeader(token)), cancellationToken).ConfigureAwait(false);

                if (starGazerResponse?.Repository.StarGazers != null)
                    yield return starGazerResponse.Repository.StarGazers;

            } while (starGazerResponse?.Repository.StarGazers.StarredAt.Count == numberOfStarGazersPerRequest);
        }

        async IAsyncEnumerable<Repository> GetOwnedRepositories(string repositoryOwner, [EnumeratorCancellation] CancellationToken cancellationToken, int numberOfRepositoriesPerRequest = 100)
        {
            RepositoryConnection? repositoryConnection = null;

            do
            {
                repositoryConnection = await GetUserRepositoryConnection(repositoryOwner, repositoryConnection?.PageInfo?.EndCursor, cancellationToken, numberOfRepositoriesPerRequest).ConfigureAwait(false);

                // Views + Clones statistics are only available for repositories with write access
                foreach (var repository in repositoryConnection.RepositoryList.Where(x => x?.Permission is RepositoryPermission.ADMIN or RepositoryPermission.MAINTAIN or RepositoryPermission.WRITE))
                {
                    if (repository is not null)
                        yield return new Repository(repository.Name, repository.Description, repository.ForkCount, repository.Owner.Login, repository.Owner.AvatarUrl,
                                                    repository.Issues.IssuesCount, repository.Watchers.TotalCount, repository.Url.ToString(), repository.IsFork, repository.DataDownloadedAt, repository.Permission);
                }
            }
            while (repositoryConnection?.PageInfo?.HasNextPage is true);
        }

        async IAsyncEnumerable<string> GetOrganizationNames(GitHubToken token, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            GitHubViewerOrganizationResponse? gitHubViewerOrganizationResponse = null;

            do
            {
                gitHubViewerOrganizationResponse = await ExecuteGraphQLRequest(() => _githubApiClient.ViewerOrganizationsQuery(new ViewerOrganizationsQueryContent(GetEndCursorString(gitHubViewerOrganizationResponse?.Viewer.Organizations.PageInfo.EndCursor)), GetGitHubBearerTokenHeader(token)), cancellationToken);

                foreach (var repository in gitHubViewerOrganizationResponse.Viewer.Organizations.Nodes)
                {
                    if (!string.IsNullOrWhiteSpace(repository.Login))
                    {
                        yield return repository.Login;
                    }
                }
            } while (gitHubViewerOrganizationResponse?.Viewer.Organizations.PageInfo.HasNextPage is true);
        }

        async Task<RepositoryConnection> GetUserRepositoryConnection(string repositoryOwner, string? endCursor, CancellationToken cancellationToken, int numberOfRepositoriesPerRequest = 100)
        {
            GitHubUserResponse? githubUserResponse;

            var token = await _gitHubUserService.GetGitHubToken().ConfigureAwait(false);

            try
            {
                githubUserResponse = await ExecuteGraphQLRequest(() => _githubApiClient.UserRepositoryConnectionQuery(new UserRepositoryConnectionQueryContent(repositoryOwner, GetEndCursorString(endCursor), numberOfRepositoriesPerRequest), GetGitHubBearerTokenHeader(token)), cancellationToken).ConfigureAwait(false);
            }
            catch (GraphQLException<GitHubUserResponse> e) when (e.ContainsSamlOrganizationAthenticationError(out _))
            {
                githubUserResponse = e.GraphQLData;
            }

            return githubUserResponse.User.RepositoryConnection;
        }

        async Task<RepositoryConnection> GetOrganizationRepositoryConnection(string organizationLogin, GitHubToken token, string? endCursor, CancellationToken cancellationToken, int numberOfRepositoriesPerRequest = 100)
        {
            GitHubOrganizationResponse? githubOrganizationResponse;

            try
            {
                githubOrganizationResponse = await ExecuteGraphQLRequest(() => _githubApiClient.OrganizationRepositoryConnectionQuery(new OrganizationRepositoryConnectionQueryContent(organizationLogin, GetEndCursorString(endCursor), numberOfRepositoriesPerRequest), GetGitHubBearerTokenHeader(token)), cancellationToken).ConfigureAwait(false);
            }
            catch (GraphQLException<GitHubOrganizationResponse> e) when (e.ContainsSamlOrganizationAthenticationError(out _))
            {
                githubOrganizationResponse = e.GraphQLData;
            }

            return githubOrganizationResponse.Organization.RepositoryConnection;
        }

        async Task<T> ExecuteGraphQLRequest<T>(Func<Task<ApiResponse<GraphQLResponse<T>>>> action, CancellationToken cancellationToken, int numRetries = 2, [CallerMemberName] string callerName = "")
        {
            var response = await AttemptAndRetry_Mobile(action, cancellationToken, numRetries, callerName: callerName).ConfigureAwait(false);

            await response.EnsureSuccessStatusCodeAsync().ConfigureAwait(false);

            if (response?.Content?.Errors is not null)
                throw new GraphQLException<T>(response.Content.Data, response.Content.Errors, response.StatusCode, response.Headers);

            if (response?.Content is null)
                throw new JsonSerializationException();

            return response.Content.Data;
        }
    }
}
