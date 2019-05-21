using Octokit;

namespace GitTrends.Shared
{
    public class GitHubOAuthModel
    {
        public GitHubOAuthModel(GitHubClient gitHubClient, string loginCode) => (GitHubClient, LoginCode) = (gitHubClient, loginCode);

        public GitHubClient GitHubClient { get; }
        public string LoginCode { get; }
    }
}
