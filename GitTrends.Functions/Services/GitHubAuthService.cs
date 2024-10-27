using GitTrends.Common;

namespace GitTrends.Functions;

class GitHubAuthService(IGitHubAuthApi gitHubAuthApi)
{
	public Task<GitHubToken> GetGitHubToken(string clientId, string clientSecret, string loginCode, string state, CancellationToken token) =>
		gitHubAuthApi.GetAccessToken(clientId, clientSecret, loginCode, state, token);
}