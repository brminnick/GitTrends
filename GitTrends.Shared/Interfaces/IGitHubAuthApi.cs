using Refit;

namespace  GitTrends.Common;

[Headers("User-Agent: " + nameof(GitTrends), "Accept-Encoding: gzip", "Accept: application/json")]
interface IGitHubAuthApi
{
	[Get("/login/oauth/access_token")]
	Task<GitHubToken> GetAccessToken([AliasAs("client_id")] string clientId, [AliasAs("client_secret")] string clientSecret, [AliasAs("code")] string loginCode, [AliasAs("state")] string state, CancellationToken token);
}