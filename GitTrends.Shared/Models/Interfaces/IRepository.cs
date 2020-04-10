using System;

namespace GitTrends.Shared
{
    interface IRepository
    {
        DateTimeOffset DataDownloadedAt { get; }
        string OwnerLogin { get; }
        string OwnerAvatarUrl { get; }
        int StarCount { get; }
        int IssuesCount { get; }
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
