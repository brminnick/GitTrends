using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Refit;

namespace GitTrends.Shared;

[Headers("User-Agent: " + nameof(GitTrends), "Accept-Encoding: gzip", "Accept: application/json")]
public interface IGitHubApiV3
{
	[Get("/repos/{owner}/{repo}/traffic/views")]
	Task<RepositoryViewsResponseModel> GetRepositoryViewStatistics(string owner, string repo, [Header("Authorization")] string authorization);

	[Get("/repos/{owner}/{repo}/traffic/clones")]
	Task<RepositoryClonesResponseModel> GetRepositoryCloneStatistics(string owner, string repo, [Header("Authorization")] string authorization);

	[Get("/repos/{owner}/{repo}/traffic/popular/referrers")]
	Task<IReadOnlyList<ReferringSiteModel>> GetReferingSites(string owner, string repo, [Header("Authorization")] string authorization);

	[Get("/repos/{owner}/{repo}")]
	Task<HttpResponseMessage> GetGitHubApiResponse_Authenticated([Header("Authorization")] string authorization, string owner = GitHubConstants.GitTrendsRepoOwner, string repo = GitHubConstants.GitTrendsRepoName);

	[Get("/repos/{owner}/{repo}")]
	Task<HttpResponseMessage> GetGitHubApiResponse_Unauthenticated(string owner = GitHubConstants.GitTrendsRepoOwner, string repo = GitHubConstants.GitTrendsRepoName);

	[Get("/repos/{owner}/{repo}/contributors")]
	Task<IReadOnlyList<Contributor>> GetContributors(string owner, string repo);

	[Get("/repos/{owner}/{repo}/contributors")]
	Task<IReadOnlyList<Contributor>> GetContributors(string owner, string repo, [Header("Authorization")] string authorization);

	[Get("/repos/brminnick/gittrends/contents{filePath}")]
	Task<RepositoryFile> GetGitTrendsFile(string filePath);

	[Get("/repos/brminnick/gittrends/contents{filePath}")]
	Task<RepositoryFile> GetGitTrendsFile(string filePath, [Header("Authorization")] string authorization);

	[Get("/repos/{owner}/{repo}")]
	Task<GetRepositoryResponse> GetRepository(string owner, string repo);

	[Get("/repos/{owner}/{repo}")]
	Task<GetRepositoryResponse> GetRepository(string owner, string repo, [Header("Authorization")] string authorization);
}