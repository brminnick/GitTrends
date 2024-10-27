using System.Runtime.CompilerServices;
using AsyncAwaitBestPractices;
using GitHubApiStatus;
using GitTrends.Common;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using Refit;

namespace GitTrends;

public class GitHubGraphQLApiService(
	IAnalyticsService analyticsService,
	IGitHubGraphQLApi gitHubGraphQLApi,
	GitHubUserService gitHubUserService,
	IGitHubApiStatusService gitHubApiStatusService) : BaseMobileApiService(analyticsService)
{
	static readonly WeakEventManager<(string, TimeSpan)> _abuseRateLimitFound_GetOrganizationRepositoriesEventManager = new();

	readonly IGitHubGraphQLApi _githubApiClient = gitHubGraphQLApi;
	readonly GitHubUserService _gitHubUserService = gitHubUserService;
	readonly IGitHubApiStatusService _gitHubApiStatusService = gitHubApiStatusService;

	public static event EventHandler<(string OrganizationName, TimeSpan RetryTimeSpan)> AbuseRateLimitFound_GetOrganizationRepositories
	{
		add => _abuseRateLimitFound_GetOrganizationRepositoriesEventManager.AddEventHandler(value);
		remove => _abuseRateLimitFound_GetOrganizationRepositoriesEventManager.RemoveEventHandler(value);
	}

	public async Task<(string login, string name, Uri avatarUri)> GetCurrentUserInfo(CancellationToken cancellationToken)
	{
		var token = await _gitHubUserService.GetGitHubToken().ConfigureAwait(false);
		var data = await ExecuteGraphQLRequest(() => _githubApiClient.ViewerLoginQuery(new ViewerLoginQueryContent(), GetGitHubBearerTokenHeader(token), cancellationToken)).ConfigureAwait(false);

		return (data.Viewer.Alias, data.Viewer.Name, data.Viewer.AvatarUri);
	}

	public async Task<Repository> GetRepository(string repositoryOwner, string repositoryName, CancellationToken cancellationToken)
	{
		var token = await _gitHubUserService.GetGitHubToken().ConfigureAwait(false);

		var repositoryQueryTask = ExecuteGraphQLRequest(() => _githubApiClient.RepositoryQuery(new RepositoryQueryContent(repositoryOwner, repositoryName), GetGitHubBearerTokenHeader(token), cancellationToken));
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
			starGazersResult.StarredAt.Count,
			repositoryResult.Repository.Url.ToString(),
			repositoryResult.Repository.IsFork,
			DateTimeOffset.UtcNow,
			repositoryResult.Repository.Permission,
			repositoryResult.Repository.IsArchived,
			starredAt: starGazersResult.StarredAt.Select(static x => x.StarredAt));
	}

	public async Task<StarGazers> GetStarGazers(string repositoryName, string repositoryOwner, CancellationToken cancellationToken)
	{
		if (_gitHubUserService.IsDemoUser)
		{
			var starCount = DemoDataConstants.GetRandomNumber();
			var starredAtDates = DemoDataConstants.GenerateStarredAtDates(starCount);

			var starGazerInfoList = starredAtDates.Select(static x => new StarGazerInfo(x, string.Empty));

			return new StarGazers(starCount, [.. starGazerInfoList]);
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
			await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken).ConfigureAwait(false);

			for (int i = 0; i < DemoDataConstants.RepoCount; i++)
			{
				var demoRepo = new Repository($"Repository " + DemoDataConstants.GetRandomText(), DemoDataConstants.GetRandomText(), DemoDataConstants.GetRandomNumber(),
					DemoUserConstants.Alias, _gitHubUserService.AvatarUrl, DemoDataConstants.GetRandomNumber(), DemoDataConstants.GetRandomNumber(),
					DemoDataConstants.GetRandomNumber(), _gitHubUserService.AvatarUrl, false, DateTimeOffset.UtcNow, RepositoryPermission.ADMIN, false);
				yield return demoRepo;
			}

			//Allow UI to update
			await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken).ConfigureAwait(false);
		}
		else
		{
			await foreach (var repository in GetOwnedRepositories(repositoryOwner, cancellationToken, numberOfRepositoriesPerRequest).ConfigureAwait(false))
			{
				yield return repository;
			}

			if (_gitHubUserService.ShouldIncludeOrganizations)
			{
				await foreach (var repository in GetViewerOrganizationRepositories(cancellationToken, numberOfRepositoriesPerRequest).ConfigureAwait(false))
				{
					yield return repository;
				}
			}
		}
	}

	public async IAsyncEnumerable<Repository> GetViewerOrganizationRepositories([EnumeratorCancellation] CancellationToken cancellationToken, int numberOfRepositoriesPerRequest = 100)
	{
		var organizationNameList = new List<string>();
		var getOrganizationTaskList = new List<Task<IReadOnlyList<Repository>>>();

		var token = await _gitHubUserService.GetGitHubToken().ConfigureAwait(false);

		await foreach (var organization in GetOrganizationNames(token, cancellationToken).ConfigureAwait(false))
		{
			organizationNameList.Add(organization);
			getOrganizationTaskList.Add(GetOrganizationRepositories(organization, cancellationToken, numberOfRepositoriesPerRequest));
		}

		while (getOrganizationTaskList.Any())
		{
			var finishedTask = await Task.WhenAny(getOrganizationTaskList).ConfigureAwait(false);
			getOrganizationTaskList.Remove(finishedTask);

			var repositories = await finishedTask.ConfigureAwait(false);

			foreach (var repository in repositories)
				yield return repository;
		}
	}

	public async Task<IReadOnlyList<Repository>> GetOrganizationRepositories(string organization, CancellationToken cancellationToken, int numberOfRepositoriesPerRequest = 100)
	{
		var token = await _gitHubUserService.GetGitHubToken().ConfigureAwait(false);

		var repositoryList = new List<Repository>();
		RepositoryConnection? repositoryConnection = null;

		try
		{
			do
			{
				repositoryConnection = await GetOrganizationRepositoryConnection(organization, token, repositoryConnection?.PageInfo.EndCursor, cancellationToken, numberOfRepositoriesPerRequest).ConfigureAwait(false);

				// Views + Clones statistics are only available for repositories with write access
				repositoryList.AddRange(repositoryConnection.RepositoryList.Where(static x => x?.Permission is RepositoryPermission.ADMIN or RepositoryPermission.MAINTAIN or RepositoryPermission.WRITE).OfType<RepositoryConnectionNode>().Select(repository => new Repository(repository.Name, repository.Description, repository.ForkCount, repository.Owner.Login, repository.Owner.AvatarUrl, repository.Issues.IssuesCount, repository.Watchers.TotalCount, repository.Stargazers.TotalCount, repository.Url.ToString(), repository.IsFork, repository.DataDownloadedAt, repository.Permission, repository.IsArchived)));

			} while (repositoryConnection.PageInfo.HasNextPage);
		}
		catch (ApiException e) when (_gitHubApiStatusService.IsAbuseRateLimit(e, out var retryDelta))
		{
			OnAbuseRateLimitFound_GetOrganizationRepositories(organization, retryDelta.Value);
		}

		return repositoryList;
	}

	static string GetEndCursorString(string? endCursor) => string.IsNullOrWhiteSpace(endCursor) ? string.Empty : "after: \"" + endCursor + "\"";

	static async Task<T> ExecuteGraphQLRequest<T>(Func<Task<ApiResponse<GraphQLResponse<T>>>> action, [CallerMemberName] string callerName = "")
	{
		var response = await action().ConfigureAwait(false);

		await response.EnsureSuccessStatusCodeAsync().ConfigureAwait(false);

		if (response.Error is not null)
			throw response.Error;

		if (response.Content?.Errors is not null)
			throw new GraphQLException<T>(response.Content.Data, response.Content.Errors, response.StatusCode, response.Headers);

		if (response.Content is null)
			throw new InvalidOperationException("GraphQL Content Cannot Be Null");

		return response.Content.Data;
	}

	async IAsyncEnumerable<StarGazers> GetStarGazers(string repositoryName, string repositoryOwner, [EnumeratorCancellation] CancellationToken cancellationToken, int numberOfStarGazersPerRequest = 100)
	{


		StarGazerResponse? starGazerResponse = null;

		var token = await _gitHubUserService.GetGitHubToken().ConfigureAwait(false);

		do
		{
			var endCursor = GetEndCursorString(starGazerResponse?.Repository.StarGazers.StarredAt.LastOrDefault()?.Cursor);
			starGazerResponse = await ExecuteGraphQLRequest(() => _githubApiClient.StarGazerQuery(new StarGazerQueryContent(repositoryName, repositoryOwner, endCursor, numberOfStarGazersPerRequest), GetGitHubBearerTokenHeader(token), cancellationToken)).ConfigureAwait(false);

			yield return starGazerResponse.Repository.StarGazers;

		} while (starGazerResponse.Repository.StarGazers.StarredAt.Count == numberOfStarGazersPerRequest);
	}

	async IAsyncEnumerable<Repository> GetOwnedRepositories(string repositoryOwner, [EnumeratorCancellation] CancellationToken cancellationToken, int numberOfRepositoriesPerRequest = 100)
	{
		RepositoryConnection? repositoryConnection = null;

		do
		{
			repositoryConnection = await GetUserRepositoryConnection(repositoryOwner, repositoryConnection?.PageInfo.EndCursor, cancellationToken, numberOfRepositoriesPerRequest).ConfigureAwait(false);

			// Views + Clones statistics are only available for repositories with write access
			foreach (var repository in repositoryConnection.RepositoryList.Where(static x => x?.Permission is RepositoryPermission.ADMIN or RepositoryPermission.MAINTAIN or RepositoryPermission.WRITE))
			{
				if (repository is not null)
					yield return new Repository(repository.Name, repository.Description, repository.ForkCount, repository.Owner.Login, repository.Owner.AvatarUrl,
						repository.Issues.IssuesCount, repository.Watchers.TotalCount, repository.Stargazers.TotalCount, repository.Url.ToString(), repository.IsFork, repository.DataDownloadedAt, repository.Permission, repository.IsArchived);
			}
		} while (repositoryConnection.PageInfo.HasNextPage);
	}

	async IAsyncEnumerable<string> GetOrganizationNames(GitHubToken token, [EnumeratorCancellation] CancellationToken cancellationToken)
	{
		GitHubViewerOrganizationResponse? gitHubViewerOrganizationResponse = null;

		do
		{
			gitHubViewerOrganizationResponse = await ExecuteGraphQLRequest(() => _githubApiClient.ViewerOrganizationsQuery(new ViewerOrganizationsQueryContent(GetEndCursorString(gitHubViewerOrganizationResponse?.Viewer.Organizations.PageInfo.EndCursor)), GetGitHubBearerTokenHeader(token), cancellationToken));

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
			githubUserResponse = await ExecuteGraphQLRequest(() => _githubApiClient.UserRepositoryConnectionQuery(new UserRepositoryConnectionQueryContent(repositoryOwner, GetEndCursorString(endCursor), numberOfRepositoriesPerRequest), GetGitHubBearerTokenHeader(token), cancellationToken)).ConfigureAwait(false);
		}
		catch (GraphQLException<GitHubUserResponse> e) when (e.ContainsSamlOrganizationAuthenticationError(out _) || e.IsForbidden())
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
			githubOrganizationResponse = await ExecuteGraphQLRequest(() => _githubApiClient.OrganizationRepositoryConnectionQuery(new OrganizationRepositoryConnectionQueryContent(organizationLogin, GetEndCursorString(endCursor), numberOfRepositoriesPerRequest), GetGitHubBearerTokenHeader(token), cancellationToken)).ConfigureAwait(false);
		}
		catch (GraphQLException<GitHubOrganizationResponse> e) when (e.ContainsSamlOrganizationAuthenticationError(out _) || e.IsForbidden())
		{
			githubOrganizationResponse = e.GraphQLData;
		}

		return githubOrganizationResponse.Organization.RepositoryConnection;
	}

	void OnAbuseRateLimitFound_GetOrganizationRepositories(in string organizationName, TimeSpan delta) =>
		_abuseRateLimitFound_GetOrganizationRepositoriesEventManager.RaiseEvent(this, (organizationName, delta), nameof(AbuseRateLimitFound_GetOrganizationRepositories));
}