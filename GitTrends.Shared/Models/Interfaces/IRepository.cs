using System;

namespace GitTrends.Shared
{
    public interface IRepository
    {
        DateTimeOffset DataDownloadedAt { get; }
        string OwnerLogin { get; }
        string OwnerAvatarUrl { get; }
        long StarCount { get; }
        long IssuesCount { get; }
        string Name { get; }
        string Description { get; }
        long ForkCount { get; }
        string Url { get; }
        public long TotalViews { get; }
        public long TotalUniqueViews { get; }
        public long TotalClones { get; }
        public long TotalUniqueClones { get; }
    }
}
