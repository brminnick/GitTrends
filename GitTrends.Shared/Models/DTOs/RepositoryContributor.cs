using System;

namespace GitTrends.Shared
{
    public record Contributor
    {
        public Contributor(string login, Uri url, Uri avatarUrl, long contributions)
        {
            Login = login;
            GitHubUrl = url;
            AvatarUrl = avatarUrl;
            ContributionCount = contributions;
            DataDownloadedAt = DateTimeOffset.UtcNow;
        }

        public string Login { get; }
        public Uri AvatarUrl { get; }
        public Uri GitHubUrl { get; }
        public long ContributionCount { get; }
        public DateTimeOffset DataDownloadedAt { get; }
    }
}
