using System;
namespace GitTrends.Shared
{
    interface IRepository
    {
         string OwnerLogin { get; }
         Uri OwnerAvatarUrl { get; }
         int StarCount { get; }
         int IssuesCount { get; }
         string Name { get; }
         string Description { get; }
         long ForkCount { get; }
         Uri Uri { get; }
    }
}
