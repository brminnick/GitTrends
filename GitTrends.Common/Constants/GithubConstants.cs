using System.Web;

namespace GitTrends.Common;

public static class GitHubConstants
{
	public const string GitHubBaseUrl = "https://github.com";
	public const string GitHubRestApiUrl = "https://api.github.com";
	public const string GitHubGraphQLApi = "https://api.github.com/graphql";
	public const string GitHubApiAbuseDocs = "https://docs.github.com/rest/overview/resources-in-the-rest-api#abuse-rate-limits";
	public const string GitHubRateLimitingDocs = "https://docs.github.com/rest/reference/rate-limit";

	public const string GitTrendsRepoName = nameof(GitTrends);
	public const string GitTrendsRepoOwner = "brminnick";
	public const string GitTrendsAvatarUrl = "https://avatars3.githubusercontent.com/u/61480020?s=400&u=b1a900b5fa1ede22af9d2d9bfd6c49a072e659ba&v=4";

	public const string AppScheme = "github://";

	public static string OAuthScope { get; } = HttpUtility.UrlEncode("public_repo read:user read:org");
}