using System;
using Newtonsoft.Json;

namespace GitTrends.Shared
{
    public record Contributor
    {
        public Contributor(string login, Uri html_url, Uri avatar_url, long contributions, DateTimeOffset? dataDownloadedAt = null)
        {
            Login = login;
            GitHubUrl = html_url;
            AvatarUrl = avatar_url;
            ContributionCount = contributions;
            DataDownloadedAt = dataDownloadedAt ?? DateTimeOffset.UtcNow;
        }

        [JsonProperty("login")]
        public string Login { get; }

        [JsonProperty("avatar_url")]
        public Uri AvatarUrl { get; }

        [JsonProperty("html_url")]
        public Uri GitHubUrl { get; }

        [JsonProperty("contributions")]
        public long ContributionCount { get; }

        [JsonProperty("dataDownloadedAt")]
        public DateTimeOffset DataDownloadedAt { get; }
    }
}
