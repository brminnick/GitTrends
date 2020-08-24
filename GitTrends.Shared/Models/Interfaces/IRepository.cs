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
        long TotalViews { get; }
        long TotalUniqueViews { get; }
        long TotalClones { get; }
        long TotalUniqueClones { get; }
        bool IsFavorite { get; }
    }
}
