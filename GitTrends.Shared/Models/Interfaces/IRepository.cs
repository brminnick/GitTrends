using System.Collections.Generic;

namespace GitTrends.Shared
{
    interface IRepository
    {
        string OwnerLogin { get; }
        string OwnerAvatarUrl { get; }
        int StarCount { get; }
        int IssuesCount { get; }
        string Name { get; }
        string Description { get; }
        long ForkCount { get; }
        string Url { get; }
    }
}
