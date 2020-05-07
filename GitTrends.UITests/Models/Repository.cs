using System;
using GitTrends.Shared;

namespace GitTrends.UITests
{
    public class Repository : IRepository
    {
        public DateTimeOffset DataDownloadedAt { get; set; }

        public string OwnerLogin { get; set; } = string.Empty;

        public long StarCount { get; set; }

        public long IssuesCount { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public long ForkCount { get; set; }

        public string OwnerAvatarUrl { get; set; } = string.Empty;

        public string Url { get; set; } = string.Empty;

        public long TotalViews { get; set; }

        public long TotalUniqueViews { get; set; }

        public long TotalClones { get; set; }

        public long TotalUniqueClones { get; set; }

        public bool IsTrending { get; set; }
    }
}
