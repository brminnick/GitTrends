using System.Web;

namespace GitTrends.Shared
{
    public static class GitHubConstants
    {
        public const string GitHubBaseUrl = "https://github.com";
        public const string GitHubRestApiUrl = "https://api.github.com";
        public const string GitHubGraphQLApi = "https://api.github.com/graphql";
        public const string GitHubRateLimitingDocs = "https://developer.github.com/v3/#rate-limiting";

        public const string GitTrendsRepoName = nameof(GitTrends);
        public const string GitTrendsRepoOwner = "brminnick";

        public static string OAuthScope { get; } = HttpUtility.UrlEncode("public_repo read:user");
    }
}
