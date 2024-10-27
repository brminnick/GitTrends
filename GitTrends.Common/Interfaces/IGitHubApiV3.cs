using Refit;

namespace GitTrends.Common;

[Headers("User-Agent: " + nameof(GitTrends), "Accept-Encoding: gzip", "Accept: application/json")]
public interface IGitHubApiV3
{
	[Get("/repos/{owner}/{repo}/traffic/views")]
	Task<RepositoryViewsResponseModel> GetRepositoryViewStatistics(string owner, string repo, [Header("Authorization")] string authorization, CancellationToken token);

	[Get("/repos/{owner}/{repo}/traffic/clones")]
	Task<RepositoryClonesResponseModel> GetRepositoryCloneStatistics(string owner, string repo, [Header("Authorization")] string authorization, CancellationToken token);

	[Get("/repos/{owner}/{repo}/traffic/popular/referrers")]
	Task<IReadOnlyList<ReferrersResponseModel>> GetReferringSites(string owner, string repo, [Header("Authorization")] string authorization, CancellationToken token);

	[Get("/repos/{owner}/{repo}")]
	Task<HttpResponseMessage> GetGitHubApiResponse_Authenticated([Header("Authorization")] string authorization, CancellationToken token, string owner = GitHubConstants.GitTrendsRepoOwner, string repo = GitHubConstants.GitTrendsRepoName);

	[Get("/repos/{owner}/{repo}")]
	Task<HttpResponseMessage> GetGitHubApiResponse_Unauthenticated(CancellationToken token, string owner = GitHubConstants.GitTrendsRepoOwner, string repo = GitHubConstants.GitTrendsRepoName);

	[Get("/repos/{owner}/{repo}/contributors")]
	Task<IReadOnlyList<Contributor>> GetContributors(string owner, string repo, CancellationToken token);

	[Get("/repos/{owner}/{repo}/contributors")]
	Task<IReadOnlyList<Contributor>> GetContributors(string owner, string repo, [Header("Authorization")] string authorization, CancellationToken token);

	[Get("/repos/brminnick/gittrends/contents{filePath}")]
	Task<RepositoryFile> GetGitTrendsFile(string filePath, CancellationToken token);

	[Get("/repos/brminnick/gittrends/contents{filePath}")]
	Task<RepositoryFile> GetGitTrendsFile(string filePath, [Header("Authorization")] string authorization, CancellationToken token);

	[Get("/repos/{owner}/{repo}")]
	Task<GetRepositoryResponse> GetRepository(string owner, string repo, CancellationToken token);

	[Get("/repos/{owner}/{repo}")]
	Task<GetRepositoryResponse> GetRepository(string owner, string repo, [Header("Authorization")] string authorization, CancellationToken token);

	[Get("/repos/{owner}/{repo}/stargazers")]
	Task<IReadOnlyList<StarGazer>> GetStarGazers(string owner, string repo, [AliasAs("page")] int currentPageNumber, [Header("Authorization")] string authorization, CancellationToken token, [AliasAs("per_page")] int starGazersPerRequest = 100, [Header("Accept")] string accept = "application/vnd.github.v3.star+json");
}